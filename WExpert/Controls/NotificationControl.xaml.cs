using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using WExpert.Utils;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;

namespace WExpert.Controls;

public sealed partial class NotificationControl : UserControl
{
    private readonly DispatcherQueue _dispatcherQueue;

    public ObservableCollection<NotificationItem> NotificationItems { get; } = new();

    public NotificationControl()
    {
        InitializeComponent();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    private void NotificationPopup_Loaded(object sender, RoutedEventArgs e)
    {
        WExpertLogger.Instance.Debug("NotificationControl loaded...");
        if (sender is InfoBar infoBar)
        {
            NotificationPopupAnimation(infoBar);
        }
    }

    private void NotificationPopupAnimation(InfoBar infoBar)
    {
        // TranslateTransform 찾기 - 이름으로 찾는 대신 RenderTransform 속성 사용
        var transform = infoBar.RenderTransform as TranslateTransform;
        if (transform == null)
        {
            return;
        }

        // 애니메이션 생성 및 실행
        var animation = new DoubleAnimation
        {
            From = 100,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, transform);
        Storyboard.SetTargetProperty(animation, "Y");
        storyboard.Begin();
    }

    private void NotificationPopup_CloseButtonClick(InfoBar sender, object args)
    {
        // InfoBar에 연결된 NotificationItem 찾기
        var notification = NotificationItems.FirstOrDefault(n => n.Message == (sender.Message as string));
        if (notification != null)
        {
            NotificationItems.Remove(notification);
            WExpertLogger.Instance.Debug($"Manually removed notification. Current count: {NotificationItems.Count}");
        }
    }


    public void ShowNotification(string title, string message, InfoBarSeverity severity, int durationInSeconds)
    {
        WExpertLogger.Instance.Debug($"ShowNotification called with message: {message}");

        var notification = new NotificationItem
        {
            Title       = title,
            Message     = message,
            Severity    = severity
        };

        _dispatcherQueue.TryEnqueue(() =>
        {
            NotificationItems.Add(notification);
            WExpertLogger.Instance.Debug($"Added notification. Current count: {NotificationItems.Count}");

            /*
            // 애니메이션 대상이 되는 변환
            var transform = PopupTranslateTransform;

            var animation = new DoubleAnimation
            {
                From = 100,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);

            Storyboard.SetTarget(animation, transform);
            Storyboard.SetTargetProperty(animation, "Y");
            storyboard.Begin();
            */

            // 일정 시간 후 알림 제거
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationInSeconds)
            };
            timer.Tick += (s, e) =>
            {
                NotificationItems.Remove(notification);
                WExpertLogger.Instance.Debug($"Removed notification. Current count: {NotificationItems.Count}");
                timer.Stop();
            };
            timer.Start();
        });
    }
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;
}