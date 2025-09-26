using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WExpert.Controls;
public sealed partial class ProgressDialogControl : UserControl
{
    public ProgressDialogControl()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ProgressDialogControl), new PropertyMetadata(true));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(ProgressDialogControl), new PropertyMetadata(string.Empty));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}