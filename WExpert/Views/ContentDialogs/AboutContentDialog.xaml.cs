using WExpert;
using WExpert.ViewModels;
using WExpert.Views.Base;

namespace wai.Views.ContentDialogs;

public sealed partial class AboutContentDialog : WEXBaseContentDialog
{
    public AboutContentViewModel ViewModel
    {
        get;
    }

    public AboutContentDialog()
    {
        InitializeComponent();

        ViewModel = App.GetService<AboutContentViewModel>();
        DataContext = ViewModel;
    }

    private void OnClose(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Hide();
    }

    private void Dialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
    }
}
