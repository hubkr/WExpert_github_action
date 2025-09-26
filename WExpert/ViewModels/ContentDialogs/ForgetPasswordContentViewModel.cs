using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models.Dto.Data;
using WExpert.Utils;

namespace WExpert.ViewModels;

public partial class ForgetPasswordContentViewModel : ObservableRecipient
{
    private CancellationTokenSource? _cancellationOTPInputTokenSource;
    private readonly IRestApiService _restApiService;

    public IServerResponseHandler? ResponseHandler { get; set; }

    //public ICommand CloseCommand { get; }

    //public ICommand OKCommand { get; }

    public IAsyncRelayCommand RequestOTPVerificationCommand { get; }

    public IAsyncRelayCommand RequestOTPCodeCommand { get; }

    public IAsyncRelayCommand<(PasswordBox newPasswordBox, PasswordBox confirmPasswordBox)> RequestChangePasswordCommand { get; }

    [ObservableProperty]
    private string dialogTitle = string.Empty;

    [ObservableProperty]
    private bool enableRequestCode = false;

    [ObservableProperty]
    private bool enableRequestCertification = false;

    [ObservableProperty]
    private bool requestChangePasswordProgress = false;

    [ObservableProperty]
    private bool enableRequestChangePassword = false;

    [ObservableProperty]
    private string requestButtonText = "StringSendCode".GetLocalized();

    [ObservableProperty]
    private bool enableEmailTextBox = true;

    [ObservableProperty]
    private bool enableIdTextBox = true;    

    [ObservableProperty]
    private string inputValidTime = string.Empty;

    [ObservableProperty]
    private bool isOpenErrorTeachingTip = false;

    [ObservableProperty]
    private bool requestCodeProgress = false;

    [ObservableProperty]
    private bool certificationCodeProgress = false;

    [ObservableProperty]
    private Visibility showInputEmail = Visibility.Visible;

    [ObservableProperty]
    private Visibility showInputOTP = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility showInputPwd = Visibility.Collapsed;

    private ResetPwdModeType _currentDisplayMode = ResetPwdModeType.INPUT_EMAIL;
    public ResetPwdModeType CurrentDisplayMode
    {
        get => _currentDisplayMode;
        set
        {
            if (_currentDisplayMode != value)
            {
                SetProperty(ref _currentDisplayMode, value);

                switch (value)
                {
                    case ResetPwdModeType.INPUT_EMAIL:
                        ShowInputEmail  = Visibility.Visible;
                        ShowInputOTP    = Visibility.Collapsed;
                        ShowInputPwd    = Visibility.Collapsed;
                        EnableIdTextBox = true;
                        EnableEmailTextBox = true;
                        RequestButtonText = "StringSendCode".GetLocalized();

                        break;
                    case ResetPwdModeType.INPUT_OTP:
                        ShowInputEmail = Visibility.Visible;
                        ShowInputOTP = Visibility.Visible;
                        ShowInputPwd = Visibility.Collapsed;

                        EnableIdTextBox = false;
                        EnableEmailTextBox = false;
                        RequestButtonText = "StringResendCode".GetLocalized();                        
                        break;
                    case ResetPwdModeType.INPUT_PWD:
                        ShowInputEmail = Visibility.Collapsed;
                        ShowInputOTP = Visibility.Collapsed;
                        ShowInputPwd = Visibility.Visible;

                        EnableIdTextBox = false;
                        EnableEmailTextBox = false;
                        RequestButtonText = "StringResendCode".GetLocalized();
                        break;
                }
            }
        }
    }

    private string _otpCode = string.Empty;
    public string OtpCode
    {
        get => _otpCode;
        set
        {
            if (_otpCode != value)
            {
                SetProperty(ref _otpCode, value);
                EnableRequestCertification = (value.Length == 6);
            }
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                SetProperty(ref _email, value);
                EnableRequestCode = !string.IsNullOrEmpty(Id) && CommonUtils.IsValidEmail(value);
            }
        }
    }

    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                SetProperty(ref _id, value);
                EnableRequestCode = !string.IsNullOrEmpty(value) &&  CommonUtils.IsValidEmail(Email);
            }
        }
    }

    private string? CsrfToken = string.Empty;
    private bool IsSuccessResult = false;

    public ForgetPasswordContentViewModel(IRestApiService restApiService)
    {
        _restApiService = restApiService;
        //CloseCommand                    = new RelayCommand(OnClose);
        //OKCommand                       = new RelayCommand(OnOK);
        RequestOTPVerificationCommand   = new AsyncRelayCommand(OnRequestOTPVerification);
        RequestOTPCodeCommand           = new AsyncRelayCommand(OnRequestOTPCode);
        RequestChangePasswordCommand    = new AsyncRelayCommand<(PasswordBox newPasswordBox, PasswordBox confirmPasswordBox)>(OnRequestChangePassword);
    }

    //private void OnOK()
    //{        
    //}

    //private void OnClose()
    //{
    //}

    public bool GetResult()
    {
        return IsSuccessResult;
    }

    private async void StartOTPInputTimer(int elapsedSeconds)
    {
        // 기존 작업 중단
        _cancellationOTPInputTokenSource?.Cancel();
        _cancellationOTPInputTokenSource = new CancellationTokenSource();

        try
        {
            while (true)
            {
                if (elapsedSeconds == 0)
                {
                    break;
                }

                // Task.Delay로 1초 대기
                await Task.Delay(1000, _cancellationOTPInputTokenSource.Token);

                // 남은 시간 업데이트
                elapsedSeconds--;
                var timeSpan = TimeSpan.FromSeconds(elapsedSeconds);
                InputValidTime = string.Format("StringInputValidityTime".GetLocalized(), $"{timeSpan:mm\\:ss}");
            }
        }
        catch (TaskCanceledException)
        {
            InputValidTime = string.Empty;
        }
    }

    // Email 로 OTP 코드 발송 요청
    private async Task OnRequestOTPCode()
    {
        try
        {
            IsOpenErrorTeachingTip = false;
            RequestCodeProgress = true;
            EnableRequestCode = !RequestCodeProgress;
            await Task.Delay(500);

            var content = new ReceiveOtpMailIn() {LoginId= this.Id, Email = this.Email };
            var response = await _restApiService.DataRequestAsync<object>(ApiRoutes.RECEIVE_OTP_MAIL.Method, ApiRoutes.RECEIVE_OTP_MAIL.Path,
                                                                       ApiRoutes.RECEIVE_OTP_MAIL.RequiresFormData, null, content);
            if (response.Result == APIResultType.SUCCESS)
            {
                var jsonString = JsonConvert.SerializeObject(response.Data); // response.Data를 JSON으로 직렬화된 문자열로 변환
                var jsonData = JsonConvert.DeserializeObject<ReceiveOtpMailOut>(jsonString); // JSON 문자열을 객체로 변환

                CurrentDisplayMode = ResetPwdModeType.INPUT_OTP;

                if (jsonData is not null)
                {
                    var remainSec = jsonData.OtpExpireAt - DateTime.UtcNow;
                    StartOTPInputTimer(remainSec.TotalSeconds < 0 ? 0 : (int)remainSec.TotalSeconds);
                }
                else
                {
                    StartOTPInputTimer(0);
                }
            }
            else if (response.Result == APIResultType.BAD_REQUEST)
            {
                // 등록된 ID 또는 email 이 아닌 경우
                throw new BadRequestException("StringInvalidIdEmail".GetLocalized());
            }
            else if (response.Result == APIResultType.TOO_MANY_REQUESTS)
            {
                var jsonString = JsonConvert.SerializeObject(response.Data); // response.Data를 JSON으로 직렬화된 문자열로 변환
                var jsonData = JsonConvert.DeserializeObject<ReceiveOtpMailOut2>(jsonString); // JSON 문자열을 객체로 변환

                var requestQuota = 5;
                var rateLimitReset = 30;

                if (jsonData is not null)
                {
                    requestQuota = jsonData.RequestQuota;
                    rateLimitReset = jsonData.RateLimitReset;
                }

                // 특정 시간동안 요청 횟수가 초과 시
                var message = string.Format("StringTooManyRequest".GetLocalized(), requestQuota, TimeSpan.FromSeconds(rateLimitReset).ToString(@"mm\:ss"));
                throw new TooManyRequestException(message);
            }
            else
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new UnexpactedException(message);
            }
        }
        catch (BadRequestException be)
        {
            ResponseHandler?.HandleServerResponse("RequestOTPButton", be.Message);
        }
        catch (TooManyRequestException te)
        {
            ResponseHandler?.HandleServerResponse("RequestOTPButton", te.Message);
        }
        catch (UnexpactedException ue)
        {
            ResponseHandler?.HandleServerResponse("RequestOTPButton", ue.Message);
        }
        catch (Exception)
        {
            var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
            ResponseHandler?.HandleServerResponse("RequestOTPButton", message);
        }
        finally
        {
            RequestCodeProgress = false;
            EnableRequestCode = !RequestCodeProgress;
        }
    }

    // OTP 코드 인증 요청
    private async Task OnRequestOTPVerification()
    {
        try
        {
            IsOpenErrorTeachingTip = false;
            CertificationCodeProgress = true;
            EnableRequestCertification = !CertificationCodeProgress;
            await Task.Delay(500);

            var content = new VerifyOtpIn() {LoginId = this.Id, Email = this.Email, Otp = this.OtpCode };
            var response = await _restApiService.DataRequestAsync<VerifyOtpOut>(ApiRoutes.VERIFICATION_OTP.Method, ApiRoutes.VERIFICATION_OTP.Path,
                                                                       ApiRoutes.VERIFICATION_OTP.RequiresFormData, null, content);
            if (response.Result == APIResultType.SUCCESS)
            {
                _cancellationOTPInputTokenSource?.Cancel();

                var verifyOtpOut = response.Data as VerifyOtpOut;
                CsrfToken = verifyOtpOut?.CsrfToken;

                DialogTitle = "Change password";
                CurrentDisplayMode = ResetPwdModeType.INPUT_PWD;
            }
            else if (response.Result == APIResultType.UNAUTHORIZED)
            {
                // 유효 시간 초과 or 유효하지 않은 코드
                var message = "StringInvalidAuthenticationCode".GetLocalized();
                throw new UnauthorizedException(message);
            }
            else
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new UnexpactedException(message);
            }
        }
        catch (UnauthorizedException uae)
        {
            ResponseHandler?.HandleServerResponse("OTPVerification1", uae.Message);
        }
        catch (UnexpactedException ue)
        {
            ResponseHandler?.HandleServerResponse("OTPVerification2", ue.Message);
        }
        catch (Exception)
        {
            var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
            ResponseHandler?.HandleServerResponse("OTPVerification2", message);
        }
        finally
        {
            CertificationCodeProgress = false;
            EnableRequestCertification = !CertificationCodeProgress;
        }
    }

    // 비밀 번호 변경 요청
    private async Task OnRequestChangePassword((PasswordBox newPasswordBox, PasswordBox confirmPasswordBox) parameters)
    {
        try
        {
            IsOpenErrorTeachingTip = false;
            RequestChangePasswordProgress = true;
            EnableRequestChangePassword = false;

            var newPassword = parameters.newPasswordBox?.Password;
            var confirmPassword = parameters.confirmPasswordBox?.Password;

            var content = new ResetPasswordIn() { Email = this.Email, CsrfToken = this.CsrfToken, Password = newPassword };
            var response = await _restApiService.DataRequestAsync<ResetPasswordOut>(ApiRoutes.RESET_PASSWORD.Method, ApiRoutes.RESET_PASSWORD.Path,
                                                                        ApiRoutes.RESET_PASSWORD.RequiresFormData, null, content);
            if (response.Result == APIResultType.SUCCESS)
            {
                IsSuccessResult = true;
            }
            else if (response.Result == APIResultType.BAD_REQUEST)
            {
                // email 또는 new password 가 유효하지 않은 경우
                throw new BadRequestException("StringInvalidPasswordRule".GetLocalized());
            }
            else if (response.Result == APIResultType.UNAUTHORIZED)
            {
                // csrf token 이 유효하지 않은 경우
                throw new UnauthorizedException("StringAuthenticationTimeExpired".GetLocalized());
            }
            else if (response.Result == APIResultType.CONFLICT)
            {
                // new password 가 현재 비밀번호와 같은 경우
                throw new ConflictException("StringSamePassword".GetLocalized());
            }
            else
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new UnexpactedException(message);
            }
        }
        catch (BadRequestException be)
        {
            ResponseHandler?.HandleServerResponse("ChangePassword1", be.Message);
        }
        catch (UnauthorizedException uae)
        {
            ResponseHandler?.HandleServerResponse("ChangePassword1", uae.Message);
        }
        catch (ConflictException ce)
        {
            ResponseHandler?.HandleServerResponse("ChangePassword1", ce.Message);
        }
        catch (UnexpactedException ue)
        {
            ResponseHandler?.HandleServerResponse("ChangePassword1", ue.Message);
        }
        catch (Exception)
        {
            var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
            ResponseHandler?.HandleServerResponse("RequestOTPButton", message);
        }
        finally
        {
            // 표시 프로그래스 비활성화
            RequestChangePasswordProgress = false;
            EnableRequestChangePassword = true;
        }
    }
}
