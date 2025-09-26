using CommunityToolkit.Mvvm.ComponentModel;

namespace WExpert.ViewModels.ContentDialogs;

public partial class AccountSubPage2ViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string? hospital;

    [ObservableProperty]
    private string? address;

    [ObservableProperty]
    private string? country;

    [ObservableProperty]
    private string? contact;

    public AccountSubPage2ViewModel()
    {
    }
}

