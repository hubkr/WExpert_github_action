using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WExpert.ViewModels;

namespace WExpert.Views;

public sealed partial class SplashPage : Page
{
    private readonly FrameworkElement _visualRoot;

    public SplashViewModel ViewModel
    {
        get;
    }

    public SplashPage()
    {
        InitializeComponent();

        _visualRoot = (FrameworkElement)App.MainWindow.Content;
        ViewModel = App.GetService<SplashViewModel>();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        FadeAnimation.Begin();
        ViewModel.Loaded();
    }
}
