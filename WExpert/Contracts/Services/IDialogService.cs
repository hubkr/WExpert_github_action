using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml;
using WExpert.Code;
using WExpert.Models.Dto.Data;

namespace WExpert.Contracts.Services;

public interface IDialogService
{
    Task<bool> ShowMessageDialogAsync(FrameworkElement element, string title, string message,
                                       IconType type = IconType.INFO, bool isTwoButton = false,
                                       string? primaryText = null, string? secondaryText = null, bool isHideExitButton = false);

    Task<CreatePatientOut?> ShowRegistrationDialogAsync(FrameworkElement element, string? filePath = null);
    Task<string?> ShowPasteImageDialogAsync(FrameworkElement element, BitmapImage bitmapImage);
    Task<bool> ShowForgetPasswordDialogAsync(FrameworkElement element);
    Task ShowAboutDialogAsync(FrameworkElement element);
    Task ShowAccountDialogAsync(FrameworkElement element);
    Task<CreateConsultationOut?> ShowNewConsultationDialogAsync(FrameworkElement element, string? sonographyId, int quota, int used);

    void CloseCurrentDialog(); // 외부에서 닫기 위한 메서드
}
