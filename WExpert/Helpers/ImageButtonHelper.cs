using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace WExpert.Helpers;

public class ImageButtonHelper
{
    #region Common Properties
    // CornerRadius
    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ImageButtonHelper), new PropertyMetadata(new CornerRadius(0)));

    // Image Width
    public static double GetImageWidth(DependencyObject obj) => (double)obj.GetValue(ImageWidthProperty);
    public static void SetImageWidth(DependencyObject obj, double value) => obj.SetValue(ImageWidthProperty, value);

    public static readonly DependencyProperty ImageWidthProperty =
        DependencyProperty.RegisterAttached("ImageWidth", typeof(double), typeof(ImageButtonHelper), new PropertyMetadata(0.0));

    // Image Height
    public static double GetImageHeight(DependencyObject obj) => (double)obj.GetValue(ImageHeightProperty);
    public static void SetImageHeight(DependencyObject obj, double value) => obj.SetValue(ImageHeightProperty, value);

    public static readonly DependencyProperty ImageHeightProperty =
        DependencyProperty.RegisterAttached("ImageHeight", typeof(double), typeof(ImageButtonHelper), new PropertyMetadata(0.0));

    // Button Width
    public static double GetButtonWidth(DependencyObject obj) => (double)obj.GetValue(ButtonWidthProperty);
    public static void SetButtonWidth(DependencyObject obj, double value) => obj.SetValue(ButtonWidthProperty, value);

    public static readonly DependencyProperty ButtonWidthProperty =
        DependencyProperty.RegisterAttached("ButtonWidth", typeof(double), typeof(ImageButtonHelper), new PropertyMetadata(0.0));

    // Button Height
    public static double GetButtonHeight(DependencyObject obj) => (double)obj.GetValue(ButtonHeightProperty);
    public static void SetButtonHeight(DependencyObject obj, double value) => obj.SetValue(ButtonHeightProperty, value);

    public static readonly DependencyProperty ButtonHeightProperty =
        DependencyProperty.RegisterAttached("ButtonHeight", typeof(double), typeof(ImageButtonHelper), new PropertyMetadata(0.0));

    #endregion Common Properties

    #region Source Properties
    // normal
    public static string GetNSource(DependencyObject obj) => (string)obj.GetValue(NSourceProperty);
    public static void SetNSource(DependencyObject obj, string value) => obj.SetValue(NSourceProperty, value);

    public static readonly DependencyProperty NSourceProperty =
        DependencyProperty.RegisterAttached("NSource", typeof(string), typeof(ImageButtonHelper), new PropertyMetadata(null));

    // Hover
    public static string GetHSource(DependencyObject obj) => (string)obj.GetValue(HSourceProperty);
    public static void SetHSource(DependencyObject obj, string value) => obj.SetValue(HSourceProperty, value);

    public static readonly DependencyProperty HSourceProperty =
        DependencyProperty.RegisterAttached("HSource", typeof(string), typeof(ImageButtonHelper), new PropertyMetadata(null));

    // Pressed
    public static string GetPSource(DependencyObject obj) => (string)obj.GetValue(PSourceProperty);
    public static void SetPSource(DependencyObject obj, string value) => obj.SetValue(PSourceProperty, value);

    public static readonly DependencyProperty PSourceProperty =
        DependencyProperty.RegisterAttached("PSource", typeof(string), typeof(ImageButtonHelper), new PropertyMetadata(null));

    // Disabled
    public static string GetDSource(DependencyObject obj) => (string)obj.GetValue(DSourceProperty);
    public static void SetDSource(DependencyObject obj, string value) => obj.SetValue(DSourceProperty, value);

    public static readonly DependencyProperty DSourceProperty =
        DependencyProperty.RegisterAttached("DSource", typeof(string), typeof(ImageButtonHelper), new PropertyMetadata(null));
    #endregion Source Properties

    #region Background Properties
    // Normal
    public static Brush GetNBackground(DependencyObject obj) => (Brush)obj.GetValue(NBackgroundProperty);
    public static void SetNBackground(DependencyObject obj, Brush value) => obj.SetValue(NBackgroundProperty, value);

    public static readonly DependencyProperty NBackgroundProperty =
        DependencyProperty.RegisterAttached("NBackground", typeof(Brush), typeof(ImageButtonHelper), new PropertyMetadata(null));

    // Hover
    public static Brush GetHBackground(DependencyObject obj) => (Brush)obj.GetValue(HBackgroundProperty);
    public static void SetHBackground(DependencyObject obj, Brush value) => obj.SetValue(HBackgroundProperty, value);

    public static readonly DependencyProperty HBackgroundProperty =
        DependencyProperty.RegisterAttached("HBackground", typeof(Brush), typeof(ImageButtonHelper), new PropertyMetadata(null));

    // Pressed
    public static Brush GetPBackground(DependencyObject obj) => (Brush)obj.GetValue(PBackgroundProperty);
    public static void SetPBackground(DependencyObject obj, Brush value) => obj.SetValue(PBackgroundProperty, value);

    public static readonly DependencyProperty PBackgroundProperty =
        DependencyProperty.RegisterAttached("PBackground", typeof(Brush), typeof(ImageButtonHelper), new PropertyMetadata(null));

    // Disabled
    public static Brush GetDBackground(DependencyObject obj) => (Brush)obj.GetValue(DBackgroundProperty);
    public static void SetDBackground(DependencyObject obj, Brush value) => obj.SetValue(DBackgroundProperty, value);

    public static readonly DependencyProperty DBackgroundProperty =
        DependencyProperty.RegisterAttached("DBackground", typeof(Brush), typeof(ImageButtonHelper), new PropertyMetadata(null));

    #endregion Background Properties
}
