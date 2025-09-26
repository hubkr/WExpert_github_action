using WExpert.Contracts.Services;
using Microsoft.UI.Dispatching;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using WExpert.Utils;
using WExpert.Helpers;
using WExpert.ViewModels;
using WExpert.Code;

namespace WExpert.Services;

public partial class StatusMonitoringService : IStatusMonitoringService, IDisposable
{
    private int _statusCheckCount = 0; // 오류 상태 카운트 
    private bool _isWindowActive = true;
    private Timer? _sessionTimer = null;
    private DateTime _lastInteractionTime;
    // 사용자 비활동 상태 체크 시간(해당 시간동안 사용자 반응이 없을경우 로그아웃 처리)
    private readonly TimeSpan _interactionTimeout = TimeSpan.FromHours(1); /*TimeSpan.FromSeconds(30);*/
    // Timer check 주기(10초)
    private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(10);

    private readonly DispatcherQueue _dispatcherQueue;
    private readonly IRestApiService _restApiService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public StatusMonitoringService(INavigationService navigationService, IRestApiService restApiService, IDialogService dialogService)
    {
        _navigationService = navigationService;
        _restApiService = restApiService;
        _dialogService = dialogService;
        _dispatcherQueue = App.MainWindow.DispatcherQueue;//DispatcherQueue.GetForCurrentThread();

        App.MainWindow.Activated += OnWindowActivated;
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
    {
        // 윈도우가 포커스를 받은 상태인지 확인 
        if (e.WindowActivationState == WindowActivationState.CodeActivated ||
            e.WindowActivationState == WindowActivationState.PointerActivated)
        {
            // 윈도우가 포커스를 받음 (활성화됨)
            _isWindowActive = true;
        }
        else
        {
            // 윈도우가 포커스를 잃음 (비활성화됨)
            _isWindowActive = false;
        }
    }

    /// <summary>
    /// Service 종료 시 처리
    /// </summary>
    public void Dispose() // App 종료 시 처리  
    {
        StopMonitoring();
        App.MainWindow.Activated -= OnWindowActivated;
        GC.SuppressFinalize(this); // CA1816: Dispose 메서드에서 GC.SuppressFinalize 호출 추가  
    }

    /// <summary>
    /// 모니터링 시작
    /// </summary>
    public void StartMonitoring()
    {
        _lastInteractionTime = DateTime.Now;
        _sessionTimer = new Timer(StatusChecking, null, _timerInterval, _timerInterval);
        WExpertLogger.Instance.Debug("[StatusMonitorService]Start monitoring...");
    }

    /// <summary>
    /// 모니터링 중지
    /// </summary>
    public void StopMonitoring()
    {
        _sessionTimer?.Dispose();
        _sessionTimer = null;
        WExpertLogger.Instance.Debug("[StatusMonitorService]Stop monitoring...");
    }

    /// <summary>
    /// 사용자 활동 확인
    /// </summary>
    public void UserInteractionEvent()
    {
        if (_isWindowActive)
        {
            // 활동이 있을 경우 마지막 활동  시간 Update
            _lastInteractionTime = DateTime.Now;
            WExpertLogger.Instance.Debug("[StatusMonitorService]Notify user interaction...");
        }
    }

    private async void StatusChecking(object? state)
    {
        // 1. 사용자 Interaction(활동 상태) 체크
        if (DateTime.Now - _lastInteractionTime > _interactionTimeout)
        {
            WExpertLogger.Instance.Debug("[StatusMonitorService]StatusChecking. User interactions are inactive.");

            await _dispatcherQueue.EnqueueAsync(async () =>
            {
                var _visualRoot = (FrameworkElement)App.MainWindow.Content;
                // Explicitly specify the namespace to resolve ambiguity  
                var message = ResourceExtensions.GetLocalized("StringLogoutNotAction");
                await _restApiService.LogoutAsync();
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, message, true);
            });
            return;
        }

        // 2. Token 유효성 체크    
        var resultType = await _restApiService.CheckTokenValid();
        if (resultType == APIResultType.SUCCESS)
        {
            _statusCheckCount = 0;
        }
        else
        {
            var message = string.Empty;
            switch (resultType)
            {
                case APIResultType.UNAUTHORIZED: // 토큰 만료 또는 유효하지 않음
                    message = ResourceExtensions.GetLocalized("StringInvalidLoginInfo");
                    break;
                case APIResultType.METHOD_NOT_ALLOWED: // 다른 기기에서 로그인 하여 로그아웃 되었을 때 발생
                    message = ResourceExtensions.GetLocalized("StringDuplicatelogin");
                    break;
                case APIResultType.NOT_ACCEPTABLE: // 라이선스가 변경되어 로그아웃 되었을 때 발생
                    message = ResourceExtensions.GetLocalized("StringUpdatePlan");
                    break;
                default:
                    // TODO...추후 네트워크 연결끊길시 처리문제 협의후 추가
                    //_statusCheckCount++;
                    //message = _statusCheckCount > 4 ? CommonUtils.MakeHTTPErrorMessage(ResourceExtensions.GetLocalized("StringCommonErrorMessage"), (int)resultType) : string.Empty;
                    break;
            }

            if (!string.IsNullOrEmpty(message))
            {
                WExpertLogger.Instance.Debug($"[StatusMonitorService]StatusChecking. Error code({resultType})");
                await _dispatcherQueue.EnqueueAsync(async () =>
                {
                    await _restApiService.LogoutAsync();
                    _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, message, true);
                });                
                return;
            }
        }
    }
}
