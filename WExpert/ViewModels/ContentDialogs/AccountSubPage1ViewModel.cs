using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models.Dto.Data;
using WExpert.Services;
using WExpert.Utils;
using Windows.UI;

namespace WExpert.ViewModels.ContentDialogs;

public partial class AccountSubPage1ViewModel : ObservableRecipient
{
    private readonly IRestApiService _restApiService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string? id;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? mail;

    [ObservableProperty]
    private Visibility showUpdatePasswordButton = Visibility.Visible;

    [ObservableProperty]
    private bool enableUpdatePasswordButton = false;

    [ObservableProperty]
    private bool isOpenNewPasswordTeachingTip = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private SolidColorBrush errorMessageColor = new(Colors.Transparent);

    [ObservableProperty]
    private string newPasswordTeachingTipMessage = string.Empty;

    [ObservableProperty]
    private Style? newPasswordTeachingTipStyle = null;

    [ObservableProperty]
    private ImageSource? newPasswordTeachingTipIcon = null;

    [ObservableProperty]
    private bool updatePasswordProgress = false;

    [ObservableProperty]
    private FrameworkElement? errorTeachingTipTarget;

    [ObservableProperty]
    private bool isCurrentPasswordBoxFocused = false;

    [ObservableProperty]
    private bool isNewPasswordBoxFocused = false;

    private Visibility _showUpdatePassword = Visibility.Collapsed;
    public Visibility ShowUpdatePassword
    {
        get => _showUpdatePassword;
        set
        {
            SetProperty(ref _showUpdatePassword, value);

            if (value == Visibility.Visible)
            {
                IsCurrentPasswordBoxFocused = true;
            }
        }
    }

    public ICommand UpdatePasswordModeCommand
    {
        get;
    }

    public IAsyncRelayCommand<(PasswordBox currentPasswordBox, PasswordBox newPasswordBox, PasswordBox confirmPasswordBox, Button updatePasswrodButton)> UpdatePasswordCommand
    {
        get;
    }

    public AccountSubPage1ViewModel(IRestApiService restApiService, IDialogService dialogService, INavigationService navigationService)
    {
        _restApiService = restApiService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        UpdatePasswordModeCommand = new RelayCommand(OnUpdatePasswordMode);
        UpdatePasswordCommand = new AsyncRelayCommand<(PasswordBox currentPasswordBox, PasswordBox newPasswordBox, PasswordBox confirmPasswordBox, Button updatePasswrodButton)>(OnUpdatePassword);
    }

    private void OnUpdatePasswordMode()
    {
        ShowUpdatePasswordButton = Visibility.Collapsed;
        ShowUpdatePassword = Visibility.Visible;
    }

    // 비밀 번호 변경 요청
    private async Task<bool> OnUpdatePassword((PasswordBox currentPasswordBox, PasswordBox newPasswordBox, PasswordBox confirmPasswordBox, Button updatePasswrodButton) parameters)
    {
        try
        {
            UpdatePasswordProgress = true;
            ErrorMessage = string.Empty;
            EnableUpdatePasswordButton = false;
            
            await Task.Delay(500);

            var currentPassword = parameters.currentPasswordBox?.Password;
            var newPassword = parameters.newPasswordBox?.Password;

            var token = _restApiService.GetLoginInfo().AccessToken;
            var content = new ChangePasswordIn() { OldPassword = currentPassword, NewPassword = newPassword };
            var response = await _restApiService.DataRequestAsync<object>(ApiRoutes.CHANGE_PASSWORD.Method, ApiRoutes.CHANGE_PASSWORD.Path,
                                                                        ApiRoutes.CHANGE_PASSWORD.RequiresFormData, token, content);
            if (response.Result == APIResultType.SUCCESS)
            {
                ErrorMessage = "StringChangePassowdMessage".GetLocalized();
                ErrorMessageColor =
                    Application.Current.Resources.TryGetValue("ComponentsBlue", out var colorResource) && colorResource is Color color
                    ? new SolidColorBrush(color)
                    : new SolidColorBrush(Colors.Transparent);

                // 비밀 번호 입력값들 초기화
                if (parameters.currentPasswordBox is not null)
                {
                    parameters.currentPasswordBox.Password = string.Empty;
                }

                if (parameters.newPasswordBox is not null)
                {
                    parameters.newPasswordBox.Password = string.Empty;
                }

                if (parameters.confirmPasswordBox is not null)
                {
                    parameters.confirmPasswordBox.Password = string.Empty;
                }

                return true;
            }
            else if (response.Result == APIResultType.BAD_REQUEST) // 신규 암호가 암호 규칙에 맞지않은경우
            {
                throw new BadRequestException();
            }
            else if (response.Result == APIResultType.FORBIDDEN) // 기존 암호가 틀린 경우
            {
                throw new ForbiddenException();
            }
            else if (response.Result == APIResultType.CONFLICT) // new password 가 현재 비밀번호와 같은 경우
            {
                throw new ConflictException();
            }
            else if (response.Result == APIResultType.UNAUTHORIZED)
            {
                throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
            }
            else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
            {
                throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
            }
            else if (response.Result == APIResultType.NOT_ACCEPTABLE)
            {
                throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
            }
            else
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new UnexpactedException(message);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (BadRequestException)
        {
            // new password 가 validation 오류(입력 규칙이 맞지 않음)
            //ErrorMessage = "StringInvalidPasswordRule".GetLocalized();
            //ErrorMessageColor =
            //    Application.Current.Resources.TryGetValue("ComponentsRed", out var colorResource) && colorResource is Color color
            //    ? new SolidColorBrush(color)
            //    : new SolidColorBrush(Colors.Transparent);
           
            IsNewPasswordBoxFocused = true;
        }
        catch (ForbiddenException)
        {
            // 기존 암호가 틀린 경우
            ErrorMessage = "StringNotMachCurrentPassword".GetLocalized();
            ErrorMessageColor =
                Application.Current.Resources.TryGetValue("ComponentsRed", out var colorResource) && colorResource is Color color
                ? new SolidColorBrush(color)
                : new SolidColorBrush(Colors.Transparent);
            
            IsCurrentPasswordBoxFocused = true;
        }
        catch (ConflictException)
        {
            // new password 가 현재 비밀번호와 같은 경우
            ErrorMessage = "StringSamePassword".GetLocalized();
            ErrorMessageColor =
                Application.Current.Resources.TryGetValue("ComponentsRed", out var colorResource) && colorResource is Color color
                ? new SolidColorBrush(color)
                : new SolidColorBrush(Colors.Transparent);

            //IsNewPasswordBoxFocused = true;
        }
        catch (UnexpactedException ue)
        {
            // 기타 오류 메시지 출력
            ErrorMessage = ue.Message;
            ErrorMessageColor =
                Application.Current.Resources.TryGetValue("ComponentsRed", out var colorResource) && colorResource is Color color
                ? new SolidColorBrush(color)
                : new SolidColorBrush(Colors.Transparent);
        }
        catch (Exception)
        {
            // 이외 분류 되지 않은 내부 오류
            ErrorMessage = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
            ErrorMessageColor =
                Application.Current.Resources.TryGetValue("ComponentsRed", out var colorResource) && colorResource is Color color
                ? new SolidColorBrush(color)
                : new SolidColorBrush(Colors.Transparent);
        }
        finally
        {
            // 표시 프로그래스 비활성화
            UpdatePasswordProgress = false;
            // update password 버튼 활성화
            EnableUpdatePasswordButton = true;
            IsNewPasswordBoxFocused = false;
            IsCurrentPasswordBoxFocused = false;
        }

        return false;
    }
}
