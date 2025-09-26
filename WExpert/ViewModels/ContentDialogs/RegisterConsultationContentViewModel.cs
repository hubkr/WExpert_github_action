using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Services;
using WExpert.Utils;
using WinUIEx.Messaging;

namespace WExpert.ViewModels;

public partial class RegisterConsultationContentViewModel : ObservableRecipient
{
    private readonly IRestApiService _restApiService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    public ObservableCollection<InquiryExampleItem> ExampleQuestions { get; set; }    

    public ICommand CloseCommand { get; }
    
    [ObservableProperty]
    private bool enableRegisterButton = false;

    [ObservableProperty]
    private string questionComboText = string.Empty;

    [ObservableProperty]
    private string questionTexPlaceholder = string.Empty;

    [ObservableProperty]
    private string maxQuestionCount = $"/ {WExpertDefine.MAX_QUESTION_INPUT}";

    [ObservableProperty]
    private int maxQuestionLength = WExpertDefine.MAX_QUESTION_INPUT;

    [ObservableProperty]
    private int currentInputQuestionLength = 0;

    [ObservableProperty]
    private int consultationUsed = 0;

    [ObservableProperty]
    private int consultationQuota;

    [ObservableProperty]
    private int exceededCount = 0;

    [ObservableProperty]
    private bool registeringProgressRing = false;

    private string _questionText = string.Empty;
    public string QuestionText
    {
        get => _questionText;
        set
        {
            if (_questionText != value)
            {
                SetProperty(ref _questionText, value);
                if ((value.Trim().Length > 0 && !EnableRegisterButton) || (value.Trim().Length == 0 && EnableRegisterButton))
                {
                    UpdateEnableRegisterButton();
                }
                CurrentInputQuestionLength = value.Length;
            }
        }
    }

    public RegisterConsultationContentViewModel(IRestApiService restApiService, IDialogService dialogService, INavigationService navigationService)
    {
        _restApiService = restApiService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        CloseCommand = new RelayCommand(OnClose);

        // Question Textbox 의 Placeholder 추가
        QuestionTexPlaceholder = string.Format("StringQuestionTextBoxHolder".GetLocalized(), WExpertDefine.MAX_QUESTION_INPUT);
        ExampleQuestions =
        [
            new() { ExampleText = "Are the AI analysis results correct?" },
            new() { ExampleText = "Are the results of area marking through AI analysis, correct?" },
            new() { ExampleText = "Could you tell me your opinion about this image?" },
            new() { ExampleText = "Could you recommend the treatment?" },
            new() { ExampleText = "Does this patient need a capsulectomy?" }
        ];
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    private void OnClose()
    {
    }

    private void UpdateEnableRegisterButton()
    {
        EnableRegisterButton = QuestionText.Trim().Length > 0;
    }

    public async Task<APIResponse<CreateConsultationOut>?> RegisterQuestion(CreateConsultationIn content)
    {
        var token = _restApiService.GetLoginInfo().AccessToken;
        var response = await _restApiService.DataRequestAsync<CreateConsultationOut>(
            ApiRoutes.CONSULTATION_QUESTION_CREATE.Method,
            ApiRoutes.CONSULTATION_QUESTION_CREATE.Path,
            ApiRoutes.CONSULTATION_QUESTION_CREATE.RequiresFormData,
            token,
            content
        );
                
        switch(response.Result)
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
