using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WExpert.Contracts.Services;

namespace WExpert.Views.Base;

public partial class WEXBasePage : Page
{
    private DateTime _lastPointerMoveNotifyTime = DateTime.MinValue;

    private readonly IStatusMonitoringService _statusMonitoringService;
    private readonly TimeSpan _pointerMoveThrottleInterval = TimeSpan.FromSeconds(1); // PointerMoved 이벤트 처리 빈도(1초에 한 번)

    public WEXBasePage()
    {
        _statusMonitoringService = App.GetService<IStatusMonitoringService>();

        Loaded += (s, e) =>
        {
            AddHandler(KeyDownEvent, new KeyEventHandler(OnAnyKeyDown), true);
            AddHandler(PointerPressedEvent, new PointerEventHandler(OnAnyPointerPressed), true);
            AddHandler(PointerMovedEvent, new PointerEventHandler(OnAnyPointerMoved), true);
        };

        Unloaded += (s, e) =>
        {
            RemoveHandler(KeyDownEvent, new KeyEventHandler(OnAnyKeyDown));
            RemoveHandler(PointerPressedEvent, new PointerEventHandler(OnAnyPointerPressed));
            RemoveHandler(PointerMovedEvent, new PointerEventHandler(OnAnyPointerMoved));
        };
    }

    private void OnAnyPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        NotifyInteraction();
    }

    private void OnAnyKeyDown(object sender, KeyRoutedEventArgs e)
    {
        NotifyInteraction();
    }

    private void OnAnyPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var now = DateTime.UtcNow;
        // 마지막 알림 시간으로부터 충분한 시간이 지났는지 확인
        if (now - _lastPointerMoveNotifyTime > _pointerMoveThrottleInterval)
        {
            //WExpertLogger.Instance.Debug($"WEXBasePage: Pointer Moved Detected (Throttled) at {now}");
            NotifyInteraction();
            _lastPointerMoveNotifyTime = now; // 마지막 알림 시간 갱신
        }
        // else: 아직 시간이 안됐으므로 무시
    }

    private void NotifyInteraction()
    {
        _statusMonitoringService?.UserInteractionEvent();
        // PointerMoved 외의 이벤트 발생 시, PointerMoved의 마지막 알림 시간도 갱신하여 불필요한 PointerMoved 알림을 줄임
        _lastPointerMoveNotifyTime = DateTime.UtcNow;
    }
}
