using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WExpert.Activation;
using WExpert.Contracts.Services;
using WExpert.Core.Contracts.Services;
using WExpert.Core.Services;
using WExpert.Models;
using WExpert.Services;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.ViewModels.ContentDialogs;
using WExpert.Views;
using Windows.UI;

namespace WExpert;

#region Preventing duplicate executions (Native methods for Win32 API calls)(중복 실행 방지 처리)
internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    internal static extern bool IsIconic(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // ShowWindow 명령 상수
    internal const int SW_RESTORE = 9;
    internal const int SW_SHOW = 5;
}
#endregion

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host  { get; }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IRestApiService, RestApiService>();
            services.AddSingleton<IStatusMonitoringService, StatusMonitoringService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IDialogService, DialogService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IApiService, ApiService>();

            // Views and ViewModels
            services.AddTransient<AnalysisViewerViewModel>();
            services.AddTransient<AnalysisViewerPage>();
            services.AddTransient<PatientListViewModel>();
            services.AddTransient<PatientListPage>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<SplashViewModel>();
            services.AddTransient<RegistrationPatientContentViewModel>();
            services.AddTransient<LoginPage>();
            services.AddTransient<PasteImageContentViewModel>();
            services.AddTransient<MessageContentViewModel>();
            services.AddTransient<ForgetPasswordContentViewModel>();
            services.AddTransient<AboutContentViewModel>();
            services.AddTransient<AccountContentViewModel>();
            services.AddTransient<AccountSubPage1ViewModel>();
            services.AddTransient<AccountSubPage2ViewModel>();
            services.AddTransient<AccountSubPage3ViewModel>();
            services.AddTransient<RegisterConsultationContentViewModel>();
            services.AddTransient<ConfirmUpdateViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).Build();

        // Exception Handling
        UnhandledException += App_UnhandledException;        
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        if (e is null)
        {
            return;
        }

        if (e.Exception is not null && e.Exception.StackTrace is not null)
        {
            WExpertLogger.Instance.Fatal($"Exception StackTrace : {e.Exception.StackTrace}");
        }
        else if (e.Message is not null)
        {
            WExpertLogger.Instance.Fatal($"Exception Message : {e}");
        }
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        #region Preventing duplicate executions (중복 실행 방지 처리)
        bool createdNew;
        _executionMutex = new Mutex(true, WExpertDefine.ExecutionMutexName, out createdNew);

        if (!createdNew) // Already have a running instance
        {
            // Find an existing window and get its focus
            BringExistingInstanceToFront();
            Environment.Exit(1);
            return;
        }
        #endregion

        base.OnLaunched(args);

        await GetService<IActivationService>().ActivateAsync(args);

        // Titlebar Theme 적용
        var content = (MainWindow.Content as FrameworkElement)!;
        content.ActualThemeChanged += (s, e) => SetTitlebarTheme(MainWindow.AppWindow.TitleBar, content.ActualTheme);
        SetTitlebarTheme(MainWindow.AppWindow.TitleBar, content.ActualTheme);
    }

    /// <summary>
    /// Titlebar Theme 적용
    /// </summary>
    private static void SetTitlebarTheme(AppWindowTitleBar titleBar, ElementTheme theme)
    {
        try
        {
            if (theme == ElementTheme.Light)
            {
                titleBar.ForegroundColor = Colors.Black;
                titleBar.BackgroundColor = Colors.White;
                titleBar.InactiveForegroundColor = Colors.Gray;
                titleBar.InactiveBackgroundColor = Colors.White;

                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.White;
                titleBar.ButtonInactiveForegroundColor = Colors.Gray;
                titleBar.ButtonInactiveBackgroundColor = Colors.White;

                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 245, 245, 245);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Colors.White;
            }
            else if (theme == ElementTheme.Dark)
            {
                var backgroundColor = Colors.DarkGray;
                var foregroundColor = Colors.White;
                var inactiveForegroundColor = Colors.Gray;
                var buttonHoverColor = Color.FromArgb(255, 56, 58, 61); ;
                var buttonPressedColor = Color.FromArgb(255, 50, 52, 55);

                // 리소스에서 SolidColorBrush 가져오기
                if (Application.Current.Resources.TryGetValue("BgBaseAlt", out var resource) && resource is Color color1)
                {
                    backgroundColor = color1;
                }

                if (Application.Current.Resources.TryGetValue("TextPrimary", out var resource2) && resource2 is Color color2)
                {
                    foregroundColor = color2;
                }

                titleBar.ForegroundColor = foregroundColor;
                titleBar.BackgroundColor = backgroundColor;
                titleBar.InactiveForegroundColor = inactiveForegroundColor;
                titleBar.InactiveBackgroundColor = backgroundColor;

                titleBar.ButtonForegroundColor = foregroundColor;
                titleBar.ButtonBackgroundColor = backgroundColor;
                titleBar.ButtonInactiveForegroundColor = inactiveForegroundColor;
                titleBar.ButtonInactiveBackgroundColor = backgroundColor;

                titleBar.ButtonHoverForegroundColor = foregroundColor;
                titleBar.ButtonHoverBackgroundColor = buttonHoverColor;     //Color.FromArgb(255, 33, 45, 64);
                titleBar.ButtonPressedForegroundColor = foregroundColor;
                titleBar.ButtonPressedBackgroundColor = buttonPressedColor; // Color.FromArgb(255, 28, 38, 54);
            }

            // 아이콘 배경 색이 적용 안되는 오류 처리
            titleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            titleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"Set titlebar theme error : {e}");
        }
    }

    #region Preventing duplicate executions (중복 실행 방지 처리)
    private static Mutex? _executionMutex = null;

    /// <summary>
    /// Native Win32 API calls to find an existing window and get its focus
    /// </summary>
    private static void BringExistingInstanceToFront()
    {
        try
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != System.Diagnostics.Process.GetCurrentProcess().Id)
                {
                    var hWnd = process.MainWindowHandle;
                    // 창이 최소화되어 있는지 확인
                    if (NativeMethods.IsIconic(hWnd))
                    {                        
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // 창 복원
                    }
                                        
                    NativeMethods.SetForegroundWindow(hWnd); // 창을 전면으로 가져오기                    
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOW); // 창 활성화
                    break;
                }
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"Failed to bring the existing instance to the front : {e}");
        }
    }
    #endregion
}