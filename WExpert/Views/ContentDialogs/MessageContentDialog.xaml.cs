using Microsoft.UI.Xaml.Controls;
using WExpert.Code;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.Views.Base;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WExpert.Views.ContentDialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MessageContentDialog : WEXBaseContentDialog
{
    public MessageContentViewModel ViewModel { get; }

    public MessageContentDialog()
    {
        InitializeComponent();

        ViewModel = App.GetService<MessageContentViewModel>();
        DataContext = ViewModel;
    }


    public void SetContents(bool isWarnType, string title, string message)
    {
        ViewModel.DialogTitle = title;
        ViewModel.DialogContent = message;
        ViewModel.TitleIcon = isWarnType ?
            "ms-appx:///Assets/images/TitleIconYellow.png" : "ms-appx:///Assets/images/LineCheckBlue.png";
    }


    public void SetContents2(IconType type, string title, string message)
    {
        ViewModel.DialogTitle = title;
        ViewModel.DialogContent = message;

        switch (type)
        {
            case IconType.INFO:
                ViewModel.TitleIcon = "ms-appx:///Assets/images/IconInfo.png";
                break;
            case IconType.CHECK:
                ViewModel.TitleIcon = "ms-appx:///Assets/images/IconCheck.png";
                break;
            case IconType.ERROR:
                ViewModel.TitleIcon = "ms-appx:///Assets/images/IconError.png";
                break;
            case IconType.WARN:
                ViewModel.TitleIcon = "ms-appx:///Assets/images/IconWarn.png";
                break;
        }
    }

    private void Dialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // 페이지 시작시 특정 컨트롤에 포커스가 가있는
        // 현상을 방지하기 위해 페이지를 disable 후 enable 처리
        //IsEnabled = false;
        //IsEnabled = true;

        // PrimaryButton에 포커스 설정
        var result = CommonUtils.FindChildElementByName(this, "PrimaryButton");
        if (result is Button btn)
        {
            btn?.Focus(Microsoft.UI.Xaml.FocusState.Programmatic); 
        }
        else
        {            
            // PrimaryButton이 없을 경우, SecondaryButton에 포커스 설정
            result = CommonUtils.FindChildElementByName(this, "SecondaryButton");
            if (result is Button secondaryBtn)
            {
                secondaryBtn?.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            }
        }
    }

    private void OnClose(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Hide();
    }
}
