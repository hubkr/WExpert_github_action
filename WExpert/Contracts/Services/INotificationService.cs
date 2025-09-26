using Microsoft.UI.Xaml.Controls;
using WExpert.Controls;

namespace WExpert.Contracts.Services;

public interface INotificationService
{
    void Initialize(NotificationControl notificationControl);
    void ShowNotification(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int durationInSeconds = 3);
}