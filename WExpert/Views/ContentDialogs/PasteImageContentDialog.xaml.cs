using WExpert.ViewModels;
using WExpert.Views.Base;

namespace WExpert.Views.ContentDialogs;

public sealed partial class PasteImageContentDialog : WEXBaseContentDialog
{

    public PasteImageContentViewModel ViewModel
    {
        get;
    }

    public PasteImageContentDialog()
    {
        InitializeComponent();

        ViewModel = App.GetService<PasteImageContentViewModel>();
        DataContext = ViewModel;
    }

    private void Dialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var fileName = string.Format("W{0}", DateTime.Now.ToString("MMddHHmmss"));
        ViewModel.FileName = fileName;
        ViewModel.FileNameSelectionStart = fileName.Length;
    }

    public string? GetResult()
    {
        if (string.IsNullOrEmpty(ViewModel.FileName) || string.IsNullOrWhiteSpace(ViewModel.FileName))
        {
            return null;
        }

        return ViewModel.FileName + ".jpg";
    }

    private void OnClose(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Hide();
    }
}
