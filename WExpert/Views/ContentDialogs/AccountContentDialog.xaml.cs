using Microsoft.UI.Xaml.Controls;
using WExpert;
using WExpert.ViewModels;
using WExpert.Views.Base;
using WExpert.Views.ContentDialogs;

namespace wai.Views.ContentDialogs;

public sealed partial class AccountContentDialog : WEXBaseContentDialog
{
    public AccountContentViewModel ViewModel
    {
        get;
    }

    public AccountContentDialog()
    {
        InitializeComponent();

        ViewModel = App.GetService<AccountContentViewModel>();
        DataContext = ViewModel;
    }

    private void OnClose(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Hide();
    }

    private async void Dialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        menuList.SelectedIndex = 0;
        await ViewModel.GetProfileInfoAsync();
        // 초기 시작시는 정보 가져오기 전에 tab 이 호출 되므로 수동으로 첫 번째 출력 되는 tab의 정보 update
        var page1 = DetailFrame.Content as AccountSubPage1;
        page1?.UpdateAccountInfo(ViewModel.ProfileInfo?.Account);
    }

    private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        if (listView?.SelectedItem is ListViewItem item && item.Tag is string tag)
        {
            switch (tag)
            {
                case "0": // Account
                    DetailFrame.Navigate(typeof(AccountSubPage1), ViewModel.ProfileInfo?.Account);
                    break;
                case "1": // Hospital
                    DetailFrame.Navigate(typeof(AccountSubPage2), ViewModel.ProfileInfo?.Hospital);
                    break;
                case "2": // License
                    DetailFrame.Navigate(typeof(AccountSubPage3), ViewModel.ProfileInfo?.License);
                    break;
            }
        }
    }
}
