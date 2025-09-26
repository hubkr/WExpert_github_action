using Microsoft.UI.Xaml.Navigation;

namespace WExpert.Contracts.ViewModels;

public interface INavigationAware
{
    void OnNavigatedTo(NavigationMode mode, object? parameter);

    void OnNavigatedFrom();
}
