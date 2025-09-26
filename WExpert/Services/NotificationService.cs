// Services/NotificationService.cs
using Microsoft.UI.Xaml.Controls;
using WExpert.Contracts.Services;
using WExpert.Controls;
using System.Diagnostics;
using WExpert.Utils;

namespace WExpert.Services;

public class NotificationService : INotificationService
{
    private NotificationControl? _notificationControl;

    public NotificationService()
    {
        WExpertLogger.Instance.Debug("NotificationService created");
    }

    public void Initialize(NotificationControl notificationControl)
    {
        WExpertLogger.Instance.Debug("NotificationService initialized with control");
        _notificationControl = notificationControl;
    }

    public void ShowNotification(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int durationInSeconds = 3)
    {
        WExpertLogger.Instance.Debug($"NotificationService.ShowNotification called: {message}");
        if (_notificationControl == null)
        {
            WExpertLogger.Instance.Debug("ERROR: _notificationControl is null!");
            return;
        }

        _notificationControl.ShowNotification(title, message, severity, durationInSeconds);
    }

    /*
    // 다른 메서드들은 그대로 유지
    public void ShowSuccess(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Success, int durationInSeconds = 3)
    {
        ShowNotification(title, message, severity, durationInSeconds);
    }

    public void ShowError(string title, string message, int durationInSeconds = 3)
    {
        ShowNotification(title, message, InfoBarSeverity.Error, durationInSeconds);
    }

    public void ShowWarning(string title, string message, int durationInSeconds = 3)
    {
        ShowNotification(title, message, InfoBarSeverity.Warning, durationInSeconds);
    }

    public void ShowInformation(string title, string message, int durationInSeconds = 3)
    {
        ShowNotification(title, message, InfoBarSeverity.Informational, durationInSeconds);
    }
    */
}