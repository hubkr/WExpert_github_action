using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Models.Dto.Data;
using WExpert.Services;
using WExpert.Utils;

namespace WExpert.ViewModels;

public partial class LoginViewModel : ObservableRecipient
{
    private readonly FrameworkElement _visualRoot;
    private readonly INavigationService _navigationService;
    private readonly IRestApiService _restApiService;
    private readonly IStatusMonitoringService _statusMonitorService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    public IAsyncRelayCommand<(TextBox UserIDTextBox, PasswordBox PasswordBox, bool forceLogin)> LoginCommand { get; }

    public ICommand? ForgotPasswordCommand { get; }

    [ObservableProperty]
    private string? id = null;

    [ObservableProperty]
    private bool rememberId = false;

    [ObservableProperty]
    private bool activeLoginProgress = false;

    [ObservableProperty]
    private Visibility showErrorMessage = Visibility.Collapsed;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool enabledLoginButton = true;

    [ObservableProperty]
    private string programVersion = string.Empty;

    public LoginViewModel(INavigationService navigationService, IRestApiService restApiService,
        IStatusMonitoringService statusMonitorService, INotificationService notificationService, IDialogService dialogService)
    {
        _visualRoot             = (FrameworkElement)App.MainWindow.Content;
        _navigationService      = navigationService;
        _restApiService         = restApiService;
        _statusMonitorService   = statusMonitorService;
        _notificationService    = notificationService;
        _dialogService          = dialogService;

        LoginCommand = new AsyncRelayCommand<(TextBox, PasswordBox, bool)>(parameter => OnLoginAsync(parameter));
        ForgotPasswordCommand = new AsyncRelayCommand(OnForgotPassword);

        var account = SettingUtils.GetAccountInfo();
        RememberId  = account.Rememberid;
        Id          = account.Id;

        ProgramVersion = $"V {WExpertDefine.GetVersion()} (Build {WExpertDefine.GetBuildNumber()})";

#if !PROD
        ProgramVersion += $" ({SettingUtils.GetMode().ToString().ToLower()})";
#endif
    }

    private async Task<APIResultType> LoginAsync((TextBox UserIDTextBox, PasswordBox PasswordBox, bool forceLogin) parameters)
    {
        try
        {
            var userIDTextBox = parameters.UserIDTextBox;
            // MVVM 구조에 맞지는 않지만 보안상 password 는 메모리에 저장 하지 않고 직접 parameter 전달 방식으로 처리.
            var passwordBox = parameters.PasswordBox;
            var password = passwordBox.Password;

            // 로그인 중 표시 프로그래스 활성화
            ActiveLoginProgress = true;
            EnabledLoginButton = false;

            await Task.Delay(1000);

            var message = string.Empty;

            if (!string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(password))
            {
                var response = await _restApiService.LoginAsync(Id, password, parameters.forceLogin);
                switch (response.Result)
                {
                    case APIResultType.SUCCESS:
                        {
#if false // 디버깅 확인용

                            var menusString = @"[
		                    {
			                    'category': 'Implant Information',
			                    'algorithms':
			                    [
				                    { 'name': 'Pocket position', 'id': 'pocket_position', 'enabled': false, 'roi': true },
				                    { 'name': 'Shell type', 'id': 'shell_type', 'enabled': true, 'roi': false },
				                    { 'name': 'Shape type', 'id': 'shape_type', 'enabled': false, 'roi': null },
				                    { 'name': 'Manufacturer', 'id': 'manufacturer', 'enabled': false, 'roi': null },
				                    { 'name': 'Constituent', 'id': 'constituent', 'enabled': false, 'roi': null }
			                    ]
		                    },
		                    {
			                    'category': 'Complication',
			                    'algorithms':
			                    [
				                    { 'name': 'Rupture', 'id': 'rupture', 'enabled': true, 'roi': true },
				                    { 'name': 'Folding', 'id': 'folding', 'enabled': false, 'roi': null },
				                    { 'name': 'Fluid collection', 'id': 'fluid_collection', 'enabled': false, 'roi': null },
				                    { 'name': 'Thickened capsule', 'id': 'thickened_capsule', 'enabled': false, 'roi': null },
				                    { 'name': 'Upside-down rotation', 'id': 'upside_down_rotation', 'enabled': false, 'roi': null },
				                    { 'name': 'Capsular Mass', 'id': 'capsular_mass', 'enabled': false, 'roi': null },
				                    { 'name': 'Capsular Calcification', 'id': 'capsular_calcification', 'enabled': false, 'roi': null },
				                    { 'name': 'Silicone invasion to Capsule', 'id': 'silicone_invasion_to_capsule', 'enabled': false, 'roi': null },
				                    { 'name': 'Silicone invasion to LN',  'id': 'silicone_invasion_to_ln',  'enabled': false, 'roi': null}
			                    ]
		                    }
	                    ]";

                        if (response.Data is LoginDataOut loginDataOut)
                        {
                            loginDataOut.AnalysisMenus = JsonConvert.DeserializeObject<List<AnalysisMenusOut>>(menusString);
                        }
#endif
                            SettingUtils.SetAccountInfo(RememberId, Id); // ID 저장 설정                                                                         
                            _navigationService.NavigateTo(typeof(PatientListViewModel).FullName!); // 환자 목록으로 이동
                            return APIResultType.SUCCESS;
                        }
                    case APIResultType.BAD_REQUEST: // 아이디 또는 패스워드 항목이 비어있음
                        message = "StringLoginFailNotMatch".GetLocalized();
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                    case APIResultType.UNAUTHORIZED: // 아이디 또는 비밀번호 불일치
                        message = "StringLoginFailNotMatch".GetLocalized();
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                    case APIResultType.PAYMENT_REQUIRED: // 라이선스가 없거나 만료된지 30일 이상 경과
                        message = "StringLoginFailPaymentRequired".GetLocalized();
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                    case APIResultType.FORBIDDEN: // 라이선스가 만료된지 30일 이내
                        message = "StringLoginFailForbidden".GetLocalized();
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                    case APIResultType.CONFLICT: // 중복 로그인 시(이미 로그인되어 있는 경우)
                        return APIResultType.CONFLICT;
                    case APIResultType.GONE: // 계정 비활성화 상태
                        message = "StringLoginFailGone".GetLocalized();
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                    case APIResultType.UNPROCESSABLE_ENTITY: // 계정 일시정지 상태
                        message = "StringLoginFailUnProcessableContent".GetLocalized();
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                    case APIResultType.LOCKED: // 로그인 잠김 상태, 비밀번호 재설정 필요
                        {
                            // Lock 인경우 계정 잠금 상태 체크를 위해 data 값을 전달 받음.
                            var jsonString = JsonConvert.SerializeObject(response.Data); // response.Data를 JSON으로 직렬화된 문자열로 변환
                            var loginDataLockOut = JsonConvert.DeserializeObject<LoginDataLockOut>(jsonString); // JSON 문자열을 객체로 변환
                            message = loginDataLockOut?.LoginLockReset > -1 ? string.Format("StringLoginFailLockedTempMessage".GetLocalized(),
                                TimeSpan.FromSeconds((double)loginDataLockOut.LoginLockReset).ToString(@"mm\:ss")) : "StringLoginFailLockedForeverMessage".GetLocalized();
                            userIDTextBox?.Focus(FocusState.Programmatic);
                            break;
                        }
                    default: // 기타 오류 시 
                        message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode, true);
                        userIDTextBox?.Focus(FocusState.Programmatic);
                        break;
                }

                // 화면 하단에 메시지 출력 영역에 표시(popup 아님)
                ShowErrorMessage = Visibility.Visible;
                ErrorMessage = message;

                return response.Result;
            }
            else
            {
                if (string.IsNullOrEmpty(Id))
                {
                    userIDTextBox?.Focus(FocusState.Programmatic);
                }
                else
                {
                    passwordBox?.Focus(FocusState.Programmatic);
                }

                // 화면 하단에 메시지 출력 영역에 표시(popup 아님)
                ShowErrorMessage = Visibility.Visible;
                ErrorMessage = string.IsNullOrEmpty(Id) ? "StringEnterID".GetLocalized() : "StringEnterPassword".GetLocalized();

                return APIResultType.NONE;
            }
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringInternalServerErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
            return APIResultType.INTERNAL_SERVER_ERROR;
        }
        finally
        {
            // 로그인 중 표시 프로그래스 비활성화
            ActiveLoginProgress = false;
            EnabledLoginButton = true;
        }
    }

    private async Task OnLoginAsync((TextBox UserIDTextBox, PasswordBox PasswordBox, bool forceLogin) parameter)
    {
        if (parameter is ValueTuple<TextBox, PasswordBox, bool> param)
        {
            EnabledLoginButton = false;
            var result = await LoginAsync(parameter);
            if (result == APIResultType.CONFLICT)
            {
                // 중복 로그인 확인
                var confirm = await _dialogService.ShowMessageDialogAsync(_visualRoot,
                                                                       "StringDuplicateLoginTitle".GetLocalized(),
                                                                       "StringDuplicateLoginMessage".GetLocalized(),
                                                                       IconType.WARN,
                                                                       true);
                if (confirm)
                {
                    // forceLogin 시도
                    await LoginAsync((parameter.UserIDTextBox, parameter.PasswordBox, true));
                }
            }
        }
    }

    private async Task OnForgotPassword()
    {
        var result = await _dialogService.ShowForgetPasswordDialogAsync(_visualRoot);

        if (result)
        {
            var title = "StringPasswordChangedTitle".GetLocalized();
            var message = "StringPasswordChangedMessage".GetLocalized();
            await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.INFO);
        }
    }

    public void NavigatedTo(NavigationEventArgs e)
    {
        // 로그아웃시 monitoring 중지
        _statusMonitorService.StopMonitoring();

        if (e.Parameter is string message && !string.IsNullOrEmpty(message))
        {
            var dispatcherQueue = App.MainWindow.DispatcherQueue;
            dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(async () =>
            {
                _dialogService.CloseCurrentDialog(); // 기존에 열려 있는 Content Dialog 닫기
                await Task.Delay(500);
                var title = "StringLoggedOut".GetLocalized();
                await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.WARN);
            }));
        }
    }

    public void NavigatedFrom()
    {
        // 로그인시 monitoring 시작
        _statusMonitorService.StartMonitoring();
    }
}
