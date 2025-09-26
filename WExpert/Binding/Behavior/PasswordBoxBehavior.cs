using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using WExpert.Utils;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WExpert.Binding.Behavior;

public class PasswordRevealBehavior : Behavior<Button>
{
    private PointerEventHandler? _pointerEventHandler;

    protected override void OnAttached()
    {
        base.OnAttached();
        _pointerEventHandler = new PointerEventHandler(Button_PointerPressed);
        AssociatedObject.AddHandler(Button.PointerPressedEvent, _pointerEventHandler, true);
    }

    protected override void OnDetaching()
    {
        if (_pointerEventHandler != null)
        {
            AssociatedObject.RemoveHandler(Button.PointerPressedEvent, _pointerEventHandler);
            _pointerEventHandler = null;
        }

        base.OnDetaching();
    }

    private void Button_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button revealButton)
        {
            return;
        }

        if (CommonUtils.FindParentControl<PasswordBox>(revealButton) is PasswordBox passwordBox)
        {
            var isPasswordVisible = passwordBox.PasswordRevealMode == PasswordRevealMode.Visible;
            passwordBox.PasswordRevealMode = isPasswordVisible ? PasswordRevealMode.Hidden : PasswordRevealMode.Visible;

            if (CommonUtils.FindChildElementByName(passwordBox, "RevealButtonImage") is Microsoft.UI.Xaml.Controls.Image image)
            {
                var imageUri = isPasswordVisible
                    ? "ms-appx:///Assets/Images/EyeOff.png"
                    : "ms-appx:///Assets/Images/eye.png";

                image.Source = new BitmapImage(new Uri(imageUri));
            }
        }

        // 이벤트 전파 중단
        e.Handled = true;
    }
}