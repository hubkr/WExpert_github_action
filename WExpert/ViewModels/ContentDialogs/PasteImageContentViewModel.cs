using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WExpert.ViewModels;

public partial class PasteImageContentViewModel : ObservableRecipient
{
    public ICommand CloseCommand { get; }

    public ICommand OKCommand { get; }

    [ObservableProperty]
    private BitmapImage sourceImage = new();

    [ObservableProperty]
    private bool enableAddButton = false;

    [ObservableProperty]
    private int fileNameSelectionStart = 0;

    private string _fileName = string.Empty;

    public string FileName
    {
        get => _fileName;
        set
        {
            SetProperty(ref _fileName, value);
            // File name, 이미지가 존재 하는 경우만 버튼 활성화
            EnableAddButton = (!string.IsNullOrWhiteSpace(value) && SourceImage != null);
        }
    }

    public PasteImageContentViewModel()
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
