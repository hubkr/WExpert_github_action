using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WExpert.Utils;

namespace WExpert.Controls;

// Cursor 처리를 위한 상속 후 처리
public class WEXCheckBox : CheckBox
{
    private readonly InputCursor _handCursor;

    public static readonly DependencyProperty CursorTypeProperty =
        DependencyProperty.Register(
            nameof(CursorType),
            typeof(InputSystemCursorShape),
            typeof(WEXCheckBox),
            new PropertyMetadata(InputSystemCursorShape.Arrow, OnCursorTypeChanged));

    public InputSystemCursorShape CursorType
    {
        get => (InputSystemCursorShape)GetValue(CursorTypeProperty);
        set => SetValue(CursorTypeProperty, value);
    }

    public WEXCheckBox()
    {
        DefaultStyleKey = typeof(WEXCheckBox);
        PointerEntered += CustomControl_PointerEntered;
        PointerExited += CustomControl_PointerExited;

        _handCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private static void OnCursorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WEXCheckBox checkbox)
        {
            checkbox.UpdateCursor(); // 커서 타입이 변경될 때 처리
        }
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateCursor();
    }

    private void CustomControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = _handCursor;
    }

    private void CustomControl_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        // 기본 커서로 복원
        if (ProtectedCursor != null)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
    }

    private void UpdateCursor()
    {
        try
        {
            var cursor = InputSystemCursor.Create(CursorType);
            ProtectedCursor = _handCursor;
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"CheckBox update cursor error : {e}");
        }
    }
}