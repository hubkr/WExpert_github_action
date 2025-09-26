using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WExpert;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.Views.Base;
using WExpert.Helpers;

namespace wai.Views.ContentDialogs;

public sealed partial class ForgetPasswordDialog : WEXBaseContentDialog, IServerResponseHandler
{
    public ContentDialogResult ForcedResult = ContentDialogResult.None;

    public ForgetPasswordContentViewModel ViewModel { get;}

    public ForgetPasswordDialog()
    {
        InitializeComponent();

        ViewModel = App.GetService<ForgetPasswordContentViewModel>();
        DataContext = ViewModel;
        ViewModel.ResponseHandler = this;
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.DialogTitle = "StringForgotPassword".GetLocalized();
    }

    private async void EmailTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            if (ViewModel.EnableRequestCode)
            {
                await ViewModel.RequestOTPCodeCommand.ExecuteAsync(null);
            }
        }
    }

    private void CodeTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        // 숫자만 입력 되도록 처리
        args.Cancel = !args.NewText.All(char.IsDigit);
    }

    private async void ChangePasswordBtn_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;
        // 보안상 직접 전달 하여 처리
        await ViewModel.RequestChangePasswordCommand.ExecuteAsync((newPasswordBox, confirmPasswordBox));
        button.IsEnabled = true;

        if (ViewModel.GetResult())
        {
            ForcedResult = ContentDialogResult.Primary;
            Hide();
        }
    }

    private async void CodeTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            if (ViewModel.EnableRequestCertification)
            {
                await ViewModel.RequestOTPVerificationCommand.ExecuteAsync(null);
            }
        }
    }

    private void IdTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            EmailTextBox.Focus(FocusState.Programmatic);
        }
    }

    public void HandleServerResponse(string type, object? responseData)
    {
        switch (type)
        {
            case "RequestOTPButton":
                ForgetPasswordErrorTeachingTip.Target = RequestOTPButton;
                ForgetPasswordErrorTeachingTip.PreferredPlacement = TeachingTipPlacementMode.BottomLeft;
                ForgetPasswordErrorTeachingTip.PlacementMargin = new Thickness(10);
                ErrorMessage.Text = responseData as string;
                ViewModel.IsOpenErrorTeachingTip = true;
                break;
            case "OTPVerification1":
                ForgetPasswordErrorTeachingTip.Target = OTPCodeTextBox;
                ForgetPasswordErrorTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
                ForgetPasswordErrorTeachingTip.PlacementMargin = new Thickness(20);
                ErrorMessage.Text = responseData as string;
                ViewModel.IsOpenErrorTeachingTip = true;
                break;
            case "OTPVerification2":
                ForgetPasswordErrorTeachingTip.Target = RequestCertificationButton;
                ForgetPasswordErrorTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
                ForgetPasswordErrorTeachingTip.PlacementMargin = new Thickness(20);
                ErrorMessage.Text = responseData as string;
                ViewModel.IsOpenErrorTeachingTip = true;
                break;
            case "ChangePassword1":
                ForgetPasswordErrorTeachingTip.Target = ChangePasswordButton;
                ForgetPasswordErrorTeachingTip.PreferredPlacement = TeachingTipPlacementMode.Center;
                ForgetPasswordErrorTeachingTip.PlacementMargin = new Thickness(20);
                ErrorMessage.Text = responseData as string;
                ViewModel.IsOpenErrorTeachingTip = true;
                break;
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
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            // new password 와 confirm password 와 비교
            var otherPasswordBox = passwordBox.Name == "newPasswordBox" ? confirmPasswordBox : newPasswordBox;
            ViewModel.EnableRequestChangePassword = passwordBox.Password.Equals(otherPasswordBox.Password) && CommonUtils.IsValidPassword(passwordBox.Password);

            // 비밀번호 보이기(RevealButton) 에 대한 처리
            var result = CommonUtils.FindChildElementByName(passwordBox, "RevealButton");
            if (result is Button btn)
            {
                btn.Visibility = passwordBox.Password.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
