using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using WExpert.Code;

namespace WExpert.Models;

public class DiagnosticMenu : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Category { get; set; } = string.Empty;

    public string? Name { get; set; }

    public WExpertAlgorithmsType Id { get; set; }

    public WExpertAlgorithmsType? ParentId { get; set; }

    public string? ROIIcon { get; set; } = string.Empty;

    //public SolidColorBrush ROIIconBg { get; set; } = new(Colors.Transparent);

    public bool ROIEnable = false;

    private SolidColorBrush _ROIIconBg = new(Colors.Transparent);
    public SolidColorBrush ROIIconBg
    {
        get => _ROIIconBg;
        set
        {
            if (_ROIIconBg != value)
            {
                _ROIIconBg = value;
                NotifyPropertyChanged(nameof(ROIIconBg));
            }
        }
    }

    private SolidColorBrush? _resultTextColor = null;
    public SolidColorBrush? ResultTextColor
    {
        get => _resultTextColor;
        set
        {
            if (_resultTextColor != value)
            {
                _resultTextColor = value;
                NotifyPropertyChanged(nameof(ResultTextColor));
            }
        }
    }

    private bool _menuEnable;
    public bool MenuEnable
    {
        get => _menuEnable;
        set
        {
            if (_menuEnable != value)
            {
                _menuEnable = value;
                NotifyPropertyChanged(nameof(MenuEnable));
            }
        }
    }

    private string? _Result;
    public string? Result
    {
        get => _Result;
        set
        {
            if (_Result != value)
            {
                _Result = value;
                NotifyPropertyChanged(nameof(Result));
            }
        }
    }

    public void ResultReset()
    {
        Result = "-"; 
    }

    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
