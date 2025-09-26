using Microsoft.UI.Xaml;

using WExpert.Contracts.Services;
using WExpert.ViewModels;

namespace WExpert.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        //_navigationService.NavigateTo(typeof(LoginViewModel).FullName!, args.Arguments);
        _navigationService.NavigateTo(typeof(SplashViewModel).FullName!, args.Arguments);
        await Task.CompletedTask;
    }
}
