
namespace WExpert.Helpers;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;
using Windows.Graphics;
using WinUIEx;
using WExpert.Utils;

/// <summary>
/// WExpert 윈도우 상태 값 Class
/// </summary>
public class WExpertWindowState
{
    public WExpertWindowState()
    {
        Position = new RectInt32(0, 0, 600, 600); // 기본 창 사이즈 600 x 600
        State    = WindowState.Normal;            // 기본 윈도우 상태 Normal
    }

    public RectInt32 Position;

    public WindowState State // Normal, Maximized, Minimized
    {
        get; set;
    } = WindowState.Normal;
}

/// <summary>
/// 윈도우 창 저장 or 복원 처리를 위한 Class
/// </summary>
public class WindowStateHelper
{
    private readonly WeakReference<Window> _windowWeakReference;
    private readonly WExpertWindowState _wexpertWindowState;
    private static WindowStateHelper? _windowStateHelper;

    /// <summary>
    /// 처리 하고자 하는윈도우 등록(윈도우 상태 저장 or 복원)
    /// </summary>
    /// <param name="window">윈도우 객체</param>
    /// <returns></returns>
    public static void RegisterWindow(Window window)
    {
        // singleton..객체가 하나만 등록이 되도록 처리
        _windowStateHelper ??= new WindowStateHelper(window);
    }

    private WindowStateHelper(Window window)
    {
        _windowWeakReference = new(window);
        WExpertAppWindow.Changed += OnWindowChanged;
        window.Closed += OnWindowClosed;

        _wexpertWindowState = SettingUtils.GetWindowSettings();

        var size = new SizeInt32() { Height = _wexpertWindowState.Position.Height, Width = _wexpertWindowState.Position.Width };
        var position = new PointInt32() { X = _wexpertWindowState.Position.X, Y = _wexpertWindowState.Position.Y };

        if (_wexpertWindowState.State == WindowState.Maximized)
        {
            WExpertAppWindow.Move(position);
            OverlappedPresenter.Maximize();
        }
        else
        {
            var startInRange = DisplayUtils.IsInRangeScreen(position.X, position.Y);
            var endInRange = DisplayUtils.IsInRangeScreen(position.X + size.Width, position.Y + size.Height);
            if (!startInRange && !endInRange)
            {
                position.X = 0;
                position.Y = 0;
            }

            WExpertAppWindow.Resize(size);
            WExpertAppWindow.Move(position);
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        SettingUtils.SetWindowSettings(_wexpertWindowState);
    }

    private void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
    {
        switch (OverlappedPresenter.State)
        {
            case OverlappedPresenterState.Maximized:
                _wexpertWindowState.State = WindowState.Maximized;
                break;
            case OverlappedPresenterState.Restored:
                {
                    var size        = WExpertAppWindow.Size;
                    var position    = WExpertAppWindow.Position;

                    _wexpertWindowState.Position.Height = size.Height;
                    _wexpertWindowState.Position.Width  = size.Width;
                    _wexpertWindowState.Position.X      = position.X;
                    _wexpertWindowState.Position.Y      = position.Y;
                    _wexpertWindowState.State           = WindowState.Normal;
                }
                break;
            // 최소화 상태 or default 창 상태 저장 하지 않음
            case OverlappedPresenterState.Minimized:
            default:
                break;
        }
    }

    private Window WExpertWindow
    {
        get
        {
            if (_windowWeakReference.TryGetTarget(out var window))
            {
                return window;
            }
            else
            {
                throw new NullReferenceException("There is no reference to the Window object that is the target of WindowStateSaver.");
            }
        }
    }

    private AppWindow WExpertAppWindow
    {
        get
        {
            var hWnd    = WindowNative.GetWindowHandle(WExpertWindow);
            var wndId   = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
    }

    private OverlappedPresenter OverlappedPresenter => (OverlappedPresenter)WExpertAppWindow.Presenter;
}