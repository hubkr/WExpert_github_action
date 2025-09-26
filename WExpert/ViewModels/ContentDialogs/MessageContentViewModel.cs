using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WExpert.ViewModels;

public partial class MessageContentViewModel : ObservableRecipient
{
    public ICommand CloseCommand { get; }

    public ICommand OKCommand { get; }

    [ObservableProperty]
    private string dialogTitle = string.Empty;

    [ObservableProperty]
    private string dialogContent = string.Empty;

    [ObservableProperty]
    private string titleIcon = string.Empty;

    public MessageContentViewModel()
    {
        CloseCommand    = new RelayCommand(OnClose);
        OKCommand       = new RelayCommand(OnOK);
    }

    private void OnOK()
    {
    }

    private void OnClose()
    {
    }
}
