using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace WExpert.ViewModels;

public partial class ProgressRingViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string progressMessage = string.Empty;

    [ObservableProperty]
    private Visibility show = Visibility.Collapsed;

    [ObservableProperty]
    private bool active = false;

    public ProgressRingViewModel()
    {
    }
}
