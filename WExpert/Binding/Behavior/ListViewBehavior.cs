using Microsoft.UI.Xaml;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;
using Windows.System;

namespace WExpert.Binding.Behavior;

#region PatientList behaviors
public class PatientListRowTabBehavior : Behavior<ListViewBase>
{
    private TappedEventHandler? _tappedEventHandler;

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        _tappedEventHandler = new TappedEventHandler(TappedEventHandler);
        AssociatedObject.AddHandler(ListView.TappedEvent, _tappedEventHandler, true);
    }

    protected override void OnDetaching()
    {
        if (_tappedEventHandler != null)
        {
            AssociatedObject.RemoveHandler(ListView.TappedEvent, _tappedEventHandler);
            _tappedEventHandler = null;
        }
    }

    private void TappedEventHandler(object sender, TappedRoutedEventArgs e)
    {
        Command?.Execute(sender as ListView);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command", typeof(ICommand), typeof(PatientListRowTabBehavior), new PropertyMetadata(default(ICommand)));
}

public class PatientListRowDoubleTabBehavior : Behavior<ListViewBase>
{
    private DoubleTappedEventHandler? _doubleTappedEventHandler;

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        _doubleTappedEventHandler = new DoubleTappedEventHandler(DoubleTappedHandler);
        AssociatedObject.AddHandler(ListView.DoubleTappedEvent, _doubleTappedEventHandler, true);
    }

    protected override void OnDetaching()
    {
        if (_doubleTappedEventHandler != null)
        {
            AssociatedObject.RemoveHandler(ListView.DoubleTappedEvent, _doubleTappedEventHandler);
            _doubleTappedEventHandler = null;
        }
    }

    private void DoubleTappedHandler(object sender, DoubleTappedRoutedEventArgs e)
    {
        Command?.Execute(sender as ListView);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command", typeof(ICommand), typeof(PatientListRowDoubleTabBehavior), new PropertyMetadata(default(ICommand)));
}

public class PatientListKeyBehavior : Behavior<ListViewBase>
{ 
    private KeyEventHandler? _keyEventHandler;

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        _keyEventHandler = new KeyEventHandler(KeyHandler);
        AssociatedObject.AddHandler(ListView.KeyDownEvent, _keyEventHandler, true);
    }

    protected override void OnDetaching()
    {
        if (_keyEventHandler != null)
        {
            AssociatedObject.RemoveHandler(ListView.KeyDownEvent, _keyEventHandler);
            _keyEventHandler = null;
        }
    }

    private void KeyHandler(object sender, KeyRoutedEventArgs e)
    {
        var listView = sender as ListView;
        if (e.Key == VirtualKey.Enter && listView?.SelectedItem != null)
        {
            Command?.Execute(listView);
        }
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command", typeof(ICommand), typeof(PatientListKeyBehavior), new PropertyMetadata(default(ICommand)));
}
#endregion

#region AnalysisMenu behaviors
public class AnalysisMenuTabBehavior : Behavior<ListViewBase>
{
    private object? preSelectedItem = null;
    private PointerEventHandler? _pointerEventHandler;
    private TappedEventHandler? _tappedEventHandler;

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttached()
    {
        _pointerEventHandler = new PointerEventHandler(PointerHandler);
        AssociatedObject.AddHandler(ListView.PointerPressedEvent, _pointerEventHandler, true);

        _tappedEventHandler = new TappedEventHandler(TappedHandler);
        AssociatedObject.AddHandler(ListView.TappedEvent, _tappedEventHandler, true);
    }

    protected override void OnDetaching()
    {
        if (_pointerEventHandler != null)
        {
            AssociatedObject.RemoveHandler(ListView.PointerPressedEvent, _pointerEventHandler);
            _pointerEventHandler = null;
        }

        if (_tappedEventHandler != null)
        {
            AssociatedObject.RemoveHandler(ListView.TappedEvent, _tappedEventHandler);
            _tappedEventHandler = null;
        }

        //AssociatedObject.RemoveHandler(ListView.PointerPressedEvent, new PointerEventHandler(PointerHandler));
        //AssociatedObject.RemoveHandler(ListView.TappedEvent, new TappedEventHandler(TappedHandler));
    }

    private void PointerHandler(object sender, PointerRoutedEventArgs e)
    {
        var listView = sender as ListView;
        if (listView != null)
        {
            preSelectedItem = listView.SelectedItem;
        }        
    }

    private void TappedHandler(object sender, TappedRoutedEventArgs e)
    {
        var listView = sender as ListView;

        // 기존 선택되어져 있는 메뉴와 동일 메뉴 선택 시 parameter 로 true 가 내려 가도록 
        //Command?.Execute(sender as ListView);
        Command?.Execute(preSelectedItem == listView?.SelectedItem);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(AnalysisMenuTabBehavior), new PropertyMetadata(default(ICommand)));
}
#endregion