using CommunityToolkit.Mvvm.ComponentModel;

namespace WExpert.ViewModels;

public partial class ConfirmUpdateViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string windowTitle = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;
}