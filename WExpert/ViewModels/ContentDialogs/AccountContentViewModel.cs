using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models.Dto.Data;
using WExpert.Utils;

namespace WExpert.ViewModels;

public partial class AccountContentViewModel : ObservableRecipient
{
    private readonly IRestApiService _restApiService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    public ICommand CloseCommand { get; }

    public ICommand OKCommand { get; }

    [ObservableProperty]
    private string? hospital;

    [ObservableProperty]
    private bool activeProgressBar = false;

    public ProfileOut? ProfileInfo = null;

    public AccountContentViewModel(IRestApiService restApiService, INavigationService navigationService, IDialogService dialogService)
    {
        _restApiService = restApiService;
        _navigationService = navigationService;
        _dialogService = dialogService;

        CloseCommand    = new RelayCommand(OnClose);
        OKCommand       = new RelayCommand(OnOK);
    }

    public async Task GetProfileInfoAsync()
    {
        try
        {
            ActiveProgressBar = true;
            await Task.Delay(500);

            var token = _restApiService.GetLoginInfo().AccessToken;
            var url = ApiRoutes.PROFILE.Path;
            var response = await _restApiService.DataRequestAsync<ProfileOut>(ApiRoutes.PROFILE.Method, url, ApiRoutes.PROFILE.RequiresFormData, token);
            if (response.Result == APIResultType.SUCCESS)
            {
                ProfileInfo = response.Data;
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
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);

        }
        //catch (Exception e)
        //{
        // 별도 처리 하지 않음
        //}
        finally
        {
            // 프로필 정보를 못 가져왔을시 로그인시 전달 받은 기본 정보로 대체
            if (ProfileInfo is null)
            {
                // 토큰으로부터 라이선스 정보 획득
                var tokenInfo = _restApiService.GetLicenseInfo();

                var info = _restApiService.GetLoginInfo();
                var account = new ProfileAccountOut { LoginId = info?.LoginId, Name = info?.UserName };
                var hospital = new ProfileHospitalOut { Name = info?.HospitalName };
                var license = new ProfileLicenseOut
                {
                    ValidFrom = tokenInfo.ValidFrom,
                    ExpiresAt = tokenInfo.ExpiresAt,
                    AlgorithmPlanName = tokenInfo.ModelPlan,
                    ConsultationPlanName = tokenInfo.ConsultationPlan
                };
                ProfileInfo = new ProfileOut { Account = account, Hospital = hospital, License = license };
            }

            ActiveProgressBar = false;
        }
    }

    private void OnOK()
    {
    }

    private void OnClose()
    {
    }
}
