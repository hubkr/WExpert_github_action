using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using wai.Views.ContentDialogs;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using WExpert.Views.ContentDialogs;

namespace WExpert.Services;

public class DialogService : IDialogService
{
    private bool _isDialogOpen = false;
    private ContentDialog? _currentDialog;

    public async Task<bool> ShowMessageDialogAsync(FrameworkElement element, string title, string message,
                                                   IconType type = IconType.INFO, bool isTwoButton = false,
                                                   string? primaryText = null, string? secondaryText = null,
                                                   bool isHideExitButton = false)
    {
        if (_isDialogOpen)
        {
            return false;
        }

        _isDialogOpen = true;

        primaryText ??= "StringOK".GetLocalized();

        var dialog = new MessageContentDialog
        {
            Style = Application.Current.Resources["MessageContentDialogStyle1"] as Style,
            PrimaryButtonText = primaryText,
            PrimaryButtonStyle = Application.Current.Resources["MessageDialogPrimaryButtonStyle"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        if (isHideExitButton)
        {
            dialog.Closing += (sender, e) =>
            {
                if (e.Result == ContentDialogResult.Primary)
                {
                    // Primary(Update) 버튼을 누르면 닫히게 허용
                    e.Cancel = false;
                }
                else
                {
                    // X 버튼 클릭 시 닫히지 않도록 취소(종료 버튼은 hide 를 지원 하지않아 작동 되지 않도록 처리)
                    // X 버튼, ESC키, Secondary, 기타 닫기 등은 모두 막음
                    e.Cancel = true;
                }
            };
        }

        if (isTwoButton)
        {
            secondaryText ??= "StringCancel".GetLocalized();
            dialog.SecondaryButtonText = secondaryText;
            dialog.SecondaryButtonStyle = Application.Current.Resources["MessageDialogSecondaryButtonStyle"] as Style;
        }

        dialog.SetContents2(type, title, message);
        dialog.Focus(FocusState.Programmatic);

        _currentDialog = dialog;
        var result = await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;

        return result == ContentDialogResult.Primary;
    }

    public async Task<CreatePatientOut?> ShowRegistrationDialogAsync(FrameworkElement element, string? filePath = null)
    {
        if (_isDialogOpen)
        {
            return null;
        }

        _isDialogOpen = true;

        var dialog = new RegistrationPatientContentDialog
        {
            Style = Application.Current.Resources["DialogStyle3"] as Style,
            PrimaryButtonText = "StringRegistration".GetLocalized(),
            PrimaryButtonStyle = Application.Current.Resources["DialogPrimaryButtonStyle1"] as Style,
            SecondaryButtonText = "StringCancel".GetLocalized(),
            SecondaryButtonStyle = Application.Current.Resources["DialogSecondaryButtonStyle1"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        // 클립보드 붙여 넣기에서에서 호출된 경우..사용 file path 추가 
        filePath?.Let(fp => dialog.AddSourceImage(fp, Path.GetFileName(fp)));

        _currentDialog = dialog;
        var result = await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;

        return result;
    }

    public async Task<string?> ShowPasteImageDialogAsync(FrameworkElement element, BitmapImage bitmapImage)
    {
        if (_isDialogOpen)
        {
            return null;
        }

        _isDialogOpen = true;

        var dialog = new PasteImageContentDialog
        {
            Style = Application.Current.Resources["DialogStyle3"] as Style,
            PrimaryButtonText = "StringAdd".GetLocalized(),
            PrimaryButtonStyle = Application.Current.Resources["DialogPrimaryButtonStyle1"] as Style,
            SecondaryButtonText = "StringCancel".GetLocalized(),
            SecondaryButtonStyle = Application.Current.Resources["DialogSecondaryButtonStyle1"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        dialog.ViewModel.SourceImage = bitmapImage;

        _currentDialog = dialog;
        var result = await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;

        return result == ContentDialogResult.Primary ? dialog.GetResult() : null;
    }

    public async Task<bool> ShowForgetPasswordDialogAsync(FrameworkElement element)
    {
        if (_isDialogOpen)
        {
            return false;
        }

        _isDialogOpen = true;

        var dialog = new ForgetPasswordDialog
        {
            Style = Application.Current.Resources["DialogStyle4"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        _currentDialog = dialog;
        await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;

        return dialog.ForcedResult == ContentDialogResult.Primary;
    }

    public async Task ShowAboutDialogAsync(FrameworkElement element)
    {
        if (_isDialogOpen)
        {
            return;
        }

        _isDialogOpen = true;

        var dialog = new AboutContentDialog
        {
            Style = Application.Current.Resources["DialogStyle3"] as Style,
            //PrimaryButtonText = "StringOk".GetLocalized(),
            //PrimaryButtonStyle = Application.Current.Resources["DialogSecondaryButtonStyle1"] as Style,
            SecondaryButtonText = "StringOk".GetLocalized(),
            SecondaryButtonStyle = Application.Current.Resources["DialogSecondaryButtonStyle2"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        _currentDialog = dialog;
        await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;
    }

    public async Task ShowAccountDialogAsync(FrameworkElement element)
    {
        if (_isDialogOpen)
        {
            return;
        }

        _isDialogOpen = true;

        var dialog = new AccountContentDialog
        {
            Style = Application.Current.Resources["DialogStyle2"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        _currentDialog = dialog;
        await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;
    }

    public async Task<CreateConsultationOut?> ShowNewConsultationDialogAsync(FrameworkElement element, string? sonographyId, int quota, int used)
    {
        if (_isDialogOpen)
        {
            return null;
        }

        _isDialogOpen = true;

        var dialog = new RegisterConsultationContentDialog
        {
            Style = Application.Current.Resources["DialogStyle3"] as Style,
            PrimaryButtonText = "StringRegistration".GetLocalized(),
            PrimaryButtonStyle = Application.Current.Resources["DialogPrimaryButtonStyle1"] as Style,
            SecondaryButtonText = "StringCancel".GetLocalized(),
            SecondaryButtonStyle = Application.Current.Resources["DialogSecondaryButtonStyle1"] as Style,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        dialog.SetInformation(sonographyId, quota, used);

        _currentDialog = dialog;
        var result = await dialog.ShowAsync();
        _currentDialog = null;
        _isDialogOpen = false;

        return result;
    }

    public void CloseCurrentDialog()
    {
        if (_currentDialog is null)
        {
            return;
        }

        if (_currentDialog.DispatcherQueue.HasThreadAccess)
        {
            _currentDialog.Hide();
        }
        else
        {
            _currentDialog.DispatcherQueue.TryEnqueue(() => _currentDialog.Hide());
        }

        _currentDialog = null;
        _isDialogOpen = false;
    }
}
