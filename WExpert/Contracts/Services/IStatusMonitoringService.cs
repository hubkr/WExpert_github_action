namespace WExpert.Contracts.Services;

public interface IStatusMonitoringService
{
    void StartMonitoring();
    void StopMonitoring();
    void UserInteractionEvent();
}
