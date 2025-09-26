using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using WExpert.Helpers;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using WExpert.ViewModels.ContentDialogs;
using WExpert.Views.Base;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WExpert.Views.ContentDialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AccountSubPage1 : WEXBasePage
{

    public AccountSubPage1ViewModel ViewModel
    {
        get;
    }

    public AccountSubPage1()
    {
        InitializeComponent();

        ViewModel = App.GetService<AccountSubPage1ViewModel>();
        DataContext = ViewModel;
    }

    public void UpdateAccountInfo(ProfileAccountOut? account)
    {
        if (account != null)
        {
            ViewModel.Id = account.LoginId;
            ViewModel.Name = account.Name;
            ViewModel.Mail = account.Email;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        UpdateAccountInfo(e.Parameter as ProfileAccountOut);
    }

    private void UpdatePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdatePasswordCommand.Execute((CurrentPasswordBox, NewPasswordBox, ConfirmPasswordBox, UpdatePasswrodButton));
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.EnableUpdatePasswordButton =
            !string.IsNullOrEmpty(CurrentPasswordBox.Password) &&
            !string.IsNullOrEmpty(NewPasswordBox.Password) &&
            !string.IsNullOrEmpty(ConfirmPasswordBox.Password) &&
            NewPasswordBox.Password == ConfirmPasswordBox.Password;

        if (sender is PasswordBox passwordBox)
        {
            // 비밀번호 보이기(RevealButton) 에 대한 처리
            var result = CommonUtils.FindChildElementByName(passwordBox, "RevealButton");
            if (result is Button btn)
            {
                btn.Visibility = passwordBox.Password.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (passwordBox.Name.Equals("NewPasswordBox"))
            {
                if (CommonUtils.IsValidPassword(passwordBox.Password))
                {
                    ViewModel.NewPasswordTeachingTipStyle =
                        Application.Current.Resources.TryGetValue("TeachingTipSuccessStyle1", out var styleResource1) && styleResource1 is Style style1
                        ? style1
                        : null;

                    ViewModel.NewPasswordTeachingTipIcon = new BitmapImage(new Uri("ms-appx:///Assets/Images/LineCheckWhite.png"));
                    ViewModel.NewPasswordTeachingTipMessage = "Your password is valid.";
                }
                else
                {
                    ViewModel.NewPasswordTeachingTipStyle =
                        Application.Current.Resources.TryGetValue("TeachingTipErrorStyle1", out var styleResource2) && styleResource2 is Style style2
                        ? style2
                        : null;
                    ViewModel.NewPasswordTeachingTipIcon = new BitmapImage(new Uri("ms-appx:///Assets/Images/TitleIconWarnWhite.png"));
                    ViewModel.NewPasswordTeachingTipMessage = passwordBox.Password.Length < 8 ? "Password must be at least 8 characters." : "StringChangePasswordDesciption".GetLocalized();
                }

                ViewModel.IsOpenNewPasswordTeachingTip = passwordBox.Password.Length > 0;
            }
        }
    }

    private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            // 비밀번호 보이기(RevealButton) 에 대한 처리
            var result = CommonUtils.FindChildElementByName(passwordBox, "RevealButton");
            if (result is Button btn)
            {
                btn.Visibility = passwordBox.Password.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (passwordBox.Name.Equals("NewPasswordBox"))
            {
                ViewModel.IsOpenNewPasswordTeachingTip = passwordBox.Password.Length > 0;
            }
        }
    }

    private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            // 비밀번호 보이기(RevealButton) 에 대한 처리
            var result = CommonUtils.FindChildElementByName(passwordBox, "RevealButton");
            if (result is Button btn)
            {
                btn.Visibility = Visibility.Collapsed;
            }

            if (passwordBox.Name.Equals("NewPasswordBox"))
            {
                ViewModel.IsOpenNewPasswordTeachingTip = false;
            }
        }
    }

    private void PasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                if (passwordBox.Name.Equals("CurrentPasswordBox"))
                {
                    NewPasswordBox.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
                else if(passwordBox.Name.Equals("NewPasswordBox"))
                {
                    ConfirmPasswordBox.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
                else if (passwordBox.Name.Equals("ConfirmPasswordBox"))
                {
                    UpdatePasswrodButton.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }                
            }
        }
    }
}
