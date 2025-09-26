using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WExpert.Controls;

public sealed partial class ProgressRingControl : UserControl
{
    public ProgressRingControl()
    {
        InitializeComponent();
    }

    [Category("IsActiveProgressRing"), Description("Show progress")]
    public bool IsActiveProgressRing
    {
        get => Visibility == Visibility.Visible;
        set
        {
            ViewModel.Active = value;
            ViewModel.Show = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    [Category("ProgressMessage"), Description("Display progress message")]
    public string ProgressMessage
    {
        get => ViewModel.ProgressMessage;
        set => ViewModel.ProgressMessage = value ?? string.Empty;
    }
}
