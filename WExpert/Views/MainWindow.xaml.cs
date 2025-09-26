using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WExpert.Contracts.Services;
using WExpert.Controls;
using WExpert.Helpers;
using WExpert.Utils;
using Windows.UI.ViewManagement;

namespace WExpert;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;
    private readonly UISettings settings = new();

    public MainWindow()
    {
        InitializeComponent();

         Content = null;

        Title = $"{"AppDisplayName".GetLocalized()}";

        /*
        // 윈도우 타이틀 설정 (버전 정보 추가)
        Title = $"{"AppDisplayName".GetLocalized()} {WExpertDefine.GetVersion()} (Build {WExpertDefine.GetBuildNumber()})";        
#if !PROD
        if (SettingUtils.GetMode() != ServerModeType.PROD)
        {
            Title += $" ({SettingUtils.GetMode().ToString().ToLower()})";
        }
#endif
        */

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
        
        // 윈도우 창 상태 저장 or 복원을 위한 윈도우 등록
        WindowStateHelper.RegisterWindow(this);

        dispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
        {
            try
            {
                // 약간의 지연 후 아이콘 설정 (간헐 적으로 아이콘이 표시 안되는 오류 수정)
                var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico");
                if (File.Exists(iconPath))
                {
                    AppWindow.SetIcon(iconPath);
                }

                // 기존 MainWindow 화면 Layer 에 Notification 화면 항목 추가
                var currentContent = App.MainWindow.Content as UIElement;
                if (currentContent is not null)
                {
                    var grdNew = new Grid();
                    App.MainWindow.Content = grdNew;
                    grdNew.Children.Add(currentContent);

                    var notificationControl = new NotificationControl();
                    grdNew.Children.Add(notificationControl);

                    // NotificationService 초기화
                    var notificationService = App.GetService<INotificationService>();
                    notificationService.Initialize(notificationControl); // notification service에 control 추가
                }
            }
            catch (Exception e)
            {
                WExpertLogger.Instance.Error($"Error setting app icon: {e}");
            }
        });
    }

    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // 서비스의 경우 Dispose()를 호출(로그아웃 등 처리를 위해)
        (App.Current as App)!.Host.Dispose();
    }

#if false // TODO..추후 마무리
    private static ContentDialog? _progressDialog;
    private static bool _isDialogVisible = false;

    private static async Task ShowProgressDialogAsync(string message)
    {
        var _visualRoot = (FrameworkElement)App.MainWindow.Content;
        // 이전 다이얼로그가 있다면 닫습니다.
        if (_progressDialog != null)
        {
            _progressDialog.Hide();
            _progressDialog = null;
        }

        var progressControl = new ProgressDialogControl
        {
            IsActive = true,
            Message = message
        };

        _progressDialog = new ContentDialog
        {
            Content = progressControl,
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = null,
            XamlRoot = _visualRoot.XamlRoot  // XamlRoot를 명시적으로 설정
        };

        // Opened 이벤트 핸들러 추가
        _progressDialog.Opened += ProgressDialog_Opened;

        // 기존에 추가했던 다른 이벤트 핸들러들도 여기에 추가
        _progressDialog.Closed += ProgressDialog_Closed;

        _isDialogVisible = true;

        await _progressDialog.ShowAsync(ContentDialogPlacement.InPlace);
    }

    private static void ProgressDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        // 다이얼로그가 열릴 때 수행할 작업을 여기에 구현합니다
        // 예: 포커스 설정
        if (_progressDialog?.Content is ProgressDialogControl progressDialog)
        {
            progressDialog.Focus(FocusState.Programmatic);

            // 포커스 변경 이벤트를 구독합니다
            //Window.Current.CoreWindow.GetAsyncKeyState(Windows.System.VirtualKey.Tab);
            //Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        // 추가적인 초기화 작업이나 이벤트 구독 등을 여기서 수행할 수 있습니다
    }

    private static void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
    {
        if (_isDialogVisible && args.VirtualKey == Windows.System.VirtualKey.Tab)
        {
            // Tab 키를 누르면 다시 다이얼로그로 포커스를 가져옵니다
            //(_progressDialog.Content as ProgressDialog)?.Focus(FocusState.Programmatic);
            args.Handled = true;
        }
    }

    private static void ProgressDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _isDialogVisible = false;

        if (_progressDialog != null)
        {
            // Opened 이벤트 핸들러 제거
            _progressDialog.Opened -= ProgressDialog_Opened;
        }

        // 다른 정리 작업 수행
        _progressDialog = null;
    }

    private static void HideProgressDialog()
    {
        _progressDialog?.Hide();
        _progressDialog = null;  // 다이얼로그 참조를 제거합니다.
        _isDialogVisible = false;
    }

    public static async Task PerformLongRunningTaskAsync()
    {
        await ShowProgressDialogAsync("작업 진행 중...");
        try
        {
            // 여기에 실제 작업 수행 코드 작성
            await Task.Delay(5000); // 예시: 5초 대기
        }
        finally
        {
            HideProgressDialog();
        }
    }
#endif

}
