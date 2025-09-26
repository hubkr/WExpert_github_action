using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WExpert.ViewModels;

public partial class RegistrationPatientContentViewModel : ObservableRecipient
{
    private readonly IRestApiService _restApiService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    public ICommand CloseCommand { get; }
    public ICommand AddFileItemCommand { get; }
    public ICommand DeleteFileItemCommand { get; }
    public ICommand FileSelectorCommand { get; }
    public ICommand ThumbnailOpenFailedCommand { get; }

    [ObservableProperty]
    private string registrationErrorMessage = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int registrationFileCount = 0;

    [ObservableProperty]
    private Visibility showNewRegListHint = Visibility.Visible;

    [ObservableProperty]
    private bool enableRegisterButton = false;

    [ObservableProperty]
    private bool registeringProgressRing = false;

    [ObservableProperty]
    private string maxRegistrationFileCount = $"/ {WExpertDefine.MAX_REGISTRATION_COUNT}";

    [ObservableProperty]
    private string fileListOutline = "6,6";

    [ObservableProperty]
    private PatientType type = PatientType.AESTHETIC;

    private bool _isFileOpenPickerOpen = false; // 중복 open 방지용 flag

    public RegistrationPatientContentViewModel(IRestApiService restApiService,IDialogService dialogService, INavigationService navigationService)
    {
        _restApiService     = restApiService;
        _dialogService      = dialogService;
        _navigationService  = navigationService;

        CloseCommand = new RelayCommand(OnClose);
        AddFileItemCommand = new RelayCommand<object>(parameter => OnAddFileItem(parameter));
        DeleteFileItemCommand = new RelayCommand<object>(parameter => OnDeleteFileItem(parameter));
        FileSelectorCommand = new RelayCommand<object>(parameter => OnFileSelector(parameter));
        ThumbnailOpenFailedCommand = new RelayCommand<object>(parameter => OnThumbnailOpenFailed(parameter));
    }

    private void UpdateEnableRegisterButton()
    {
        var enable = false;

        // jpg,png 파일이 존재 유무
        var mimeExist = NewRegistrationFiles.FirstOrDefault(f => f.MimeType.Equals("image/jpeg") || f.MimeType.Equals("image/png"));
        if (mimeExist != null)
        {
            enable = true;
        }

        ShowNewRegListHint = NewRegistrationFiles.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        // 선택된 jpg 파일 항목이 존재 하는 경우만 버튼 활성화
        EnableRegisterButton = enable;

        // 파일 등록 상태 업데이트
        RegistrationFileCount = NewRegistrationFiles.Count;
    }

    public ObservableCollection<NewRegistrationFileInfo> NewRegistrationFiles
    {
        get;
    } = [];

    private void OnClose()
    {
    }

    private void OnAddFileItem(object? parameter)
    {
        var addFiles = new List<NewRegistrationFileInfo>();
        // Registration 창에서 Drag & Drop 또는 파일 선택창을 통해 파일 추가시 적용
        if (parameter is List<StorageFile> newRegistrationFiles && newRegistrationFiles.Count > 0)
        {
            var supportedTypes = new[] { ".jpg", ".jpeg", ".png" };
            var count = newRegistrationFiles.Count(file => supportedTypes.Contains(Path.GetExtension(file.Name).ToLowerInvariant()));

            // 최대 등록 가능 갯수 Check(최대 등록 가능 갯수 2배수 이상인 경우만 체크)
            if (count > (WExpertDefine.MAX_REGISTRATION_COUNT * 2))
            {
                RegistrationErrorMessage = "StringRegistrationFileLimit".GetLocalized();
                return;
            }
            else
            {
                RegistrationErrorMessage = string.Empty;
            }

            foreach (var file in newRegistrationFiles)
            {
                // 동일 path item 존재 확인
                var itemToExist = NewRegistrationFiles.FirstOrDefault(f => f.FilePath.Equals(file.Path));
                // 기존 동일 항목이 존재하지 않는 경우
                if (itemToExist == null)
                {
                    addFiles.Add(new NewRegistrationFileInfo(file.Path));
                }
            }
        }
        // Patients list 에서 이미지 클립 보드 붙여넣기 통해서 추가시 적용
        else if (parameter is List<NewRegistrationFileInfo> newRegistrationFiles2 && newRegistrationFiles2.Count > 0)
        {
            foreach (var file in newRegistrationFiles2)
            {
                // 동일 path item 존재 확인
                var itemToExist = NewRegistrationFiles.FirstOrDefault(f => f.FilePath.Equals(file.FilePath));
                // 기존 동일 항목이 존재하지 않는 경우
                if (itemToExist == null)
                {
                    addFiles.Add(file);
                }
            }
        }

        if (addFiles.Count > 0)
        {
            // 최대 등록 가능 갯수 Check
            if ((NewRegistrationFiles.Count + addFiles.Count) > WExpertDefine.MAX_REGISTRATION_COUNT)
            {
                RegistrationErrorMessage = "StringRegistrationFileLimit".GetLocalized();
                return;
            }
            else
            {
                RegistrationErrorMessage = string.Empty;
            }

            foreach (var file in addFiles)
            {
                NewRegistrationFiles.Add(file);
            }

            FileListOutline = "0";
            UpdateEnableRegisterButton();
        }
    }

    private void OnDeleteFileItem(object? parameter)
    {
        if (parameter is NewRegistrationFileInfo newRegistrationFiles)
        {
            // 동일 path item 삭제
            var itemToRemove = NewRegistrationFiles.FirstOrDefault(f => f.FilePath.Equals(newRegistrationFiles.FilePath));
            if (itemToRemove != null)
            {
                NewRegistrationFiles.Remove(itemToRemove);
                UpdateEnableRegisterButton();

                FileListOutline = NewRegistrationFiles.Count > 0 ? "0" : "6,6";
            }
        }
    }

    private async void OnFileSelector(object? parameter)
    {
        // 중복 open 방지
        if (_isFileOpenPickerOpen)
        {
            return;
        }
                
        try
        {
            _isFileOpenPickerOpen = true;

            var hwnd = App.MainWindow.GetWindowHandle();
            var filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                ViewMode = PickerViewMode.Thumbnail
            };
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            //filePicker.FileTypeFilter.Add(".dcm");
            InitializeWithWindow.Initialize(filePicker, hwnd);

            var files = await filePicker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                OnAddFileItem(files.ToList());
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"FileSelector open error : {e}");
        }
        finally
        {
            _isFileOpenPickerOpen = false;
        }
    }

    private void OnThumbnailOpenFailed(object? parameter)
    {
        if (parameter is Image image)
        {
            // 이미지 로딩 실패시, 대체 이미지 소스 설정 (BitmapImage 사용)
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/NoImageSmall.png"));
        }
    }

    public async Task<APIResponse<CreatePatientOut>?> RegisterPatient(MultipartFormDataContent multipartContent)
    {
        var token = _restApiService.GetLoginInfo().AccessToken;
        var response = await _restApiService.DataRequestAsync<CreatePatientOut>(
            ApiRoutes.PATIENT_CREATE.Method,
            ApiRoutes.PATIENT_CREATE.Path,
            ApiRoutes.PATIENT_CREATE.RequiresFormData,
            token,
            multipartContent
        );

        switch (response.Result)
        {
            case APIResultType.SUCCESS:
                return response;
            case APIResultType.UNAUTHORIZED:// 로그인 정보가 유효하지 않은 경우
                await _restApiService.LogoutAsync();
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, "StringInvalidLoginInfo".GetLocalized(), true);
                return null;
            case APIResultType.METHOD_NOT_ALLOWED: // 중복 로그인
                await _restApiService.LogoutAsync();
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, "StringDuplicatelogin".GetLocalized(), true);
                return null;
            case APIResultType.NOT_ACCEPTABLE: // 라이센스 정보 업데이트 필요
                await _restApiService.LogoutAsync();
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, "StringUpdatePlan".GetLocalized(), true);
                return null;
            default:
                // 이외 오류 발생 시 창을 닫지 않고 오류를 출력
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new Exception(message);
        }
    }
}
