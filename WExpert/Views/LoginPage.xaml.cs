using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using WExpert.Utils;
using WExpert.ViewModels;

namespace WExpert.Views;

public sealed partial class LoginPage : Page
{
    private readonly FrameworkElement _visualRoot;

    public LoginViewModel ViewModel
    {
        get;
    }

    public LoginPage()
    {
        InitializeComponent();

        _visualRoot = (FrameworkElement)App.MainWindow.Content;
        ViewModel = App.GetService<LoginViewModel>();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // 다른 Page 에서 Login Page 로 진입 시 Page Cache 는 초기화
        if (Frame != null)
        {
            Frame.CacheSize = 0;
        }

        ViewModel.NavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        // 로그인 후 다른 페이지 진입 시 Cache size 재 설정
        if (Frame != null)
        {
            Frame.CacheSize = 10;
        }

        ViewModel.NavigatedFrom();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // 입력 상태에 따른 focus 및 캐럿 위치 조정
        if (!string.IsNullOrEmpty(ViewModel.Id))
        {
            loginIdTextBox.SelectionStart = ViewModel.Id.Length;
            passwordBox.Focus(FocusState.Programmatic);
        }
        else
        {
            loginIdTextBox.Focus(FocusState.Programmatic);
        }
    }

    private void LoginIdTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            passwordBox.Focus(FocusState.Programmatic);
        }
    }

    private void LoginIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var result = CommonUtils.FindChildElementByName(textBox, "ClearButton");
        if (result is Button deleteButton)
        {
            deleteButton.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoginCommand.ExecuteAsync((loginIdTextBox, passwordBox, false));
    }

    private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            LoginButton_Click(loginButton, new RoutedEventArgs());
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
            // 비밀번호 보이기(RevealButton) 에 대한 처리
            var result = CommonUtils.FindChildElementByName(passwordBox, "RevealButton");
            if (result is Button btn)
            {
                btn.Visibility = passwordBox.Password.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
