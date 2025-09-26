using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WExpert.ViewModels;

public partial class AboutContentViewModel : ObservableRecipient
{
    public ICommand CloseCommand { get; }

    public ICommand OKCommand { get; }

    [ObservableProperty]
    private string version = "1.5.0"; //WExpertDefine.GetVersion();

    [ObservableProperty]
    private string buildDate = WExpertDefine.GetBuildDate();

    public AboutContentViewModel()
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
