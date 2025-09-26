using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;

namespace WExpert.ViewModels;

public partial class SplashViewModel : ObservableRecipient
{
    private readonly FrameworkElement _visualRoot;
    private readonly INavigationService _navigationService;
    private readonly IRestApiService _restApiService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private int progressPercentage = 0;

    [ObservableProperty]
    private string progressText = string.Empty;

    public SplashViewModel(INavigationService navigationService, IRestApiService restApiService, IDialogService dialogService)
    {
        _visualRoot             = (FrameworkElement)App.MainWindow.Content;
        _navigationService      = navigationService;
        _restApiService         = restApiService;
        _dialogService          = dialogService;
    }

    public void NavigatedTo(NavigationEventArgs e)
    {
    }

    public void NavigatedFrom()
    {
    }

    public async void Loaded()
    {
        var progressStepText = new List<string>
        {
            "StringStartingUp".GetLocalized(),
            "StringInitializing".GetLocalized(),
            "StringCheckingUpdate".GetLocalized(),
            "StringAlmostReady".GetLocalized(),
            "StringReadyToBegin".GetLocalized()
        };

        for (var processStep = 0; processStep < 5; processStep++)
        {
            ProgressPercentage = (processStep + 1) * 20;
            ProgressText = progressStepText[processStep];

            if (processStep == 2) // 업데이트 체크
            {
                // 얻데이트 프로세스 진행한 상태 인 경우 (그냥 바로 로그인 화면으로 이동)
                if (await AppUpdateCheck()) 
                {
                    break;
                }
            }

            await Task.Delay(500); // 0.3초 대기
        }

        // 앱 시작 시 초기화 작업 수행
        _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, "");
    }

    // true: 업데이트 프로그래스 진행한 상태, false: 업데이트 프로그래스 진행 안한 상태
    private async Task<bool> AppUpdateCheck()
    {
        var ret = false;
        CheckVersionOut? checkVersionOut = null;
        var retryCount = 0;

        // Update 가능 버전 체크
        while (checkVersionOut == null)
        {
            retryCount++;

            // 최대 3회 자동 재시도
            if (retryCount > 3)
            {
                var title = "StringConnectionErrorTitle".GetLocalized();
                var message = "StringConnectionErrorMessage".GetLocalized();
                var result = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.ERROR, true, "StringRetry".GetLocalized());

                // 재시도 Cancel 버튼시 프로그램 종료
                if (result == false)
                {
                    Environment.Exit(0);
                }

                retryCount = 0;
            }

            checkVersionOut = await CheckNewVersionAPIAsync();
            if (checkVersionOut != null)
            {
                break;
            }

            await Task.Delay(1000); // 재시도 전 1초 대기
        }

        // 최신 버전 존재 유무 알림
        if (checkVersionOut.HasNewVersion)
        {
            var parts = checkVersionOut.LatestVersion.Split('.');
            var shortVersion = parts.Length > 3 ? string.Join(".", parts.Take(3)) : checkVersionOut.LatestVersion;

            var confirmUpdate = false;
            // 강제 or 일반 업데이트 유무 확인
            if (checkVersionOut.IsForceUpdate)
            {
                var title = "StringWExpertForceUpdate".GetLocalized();
                var message = string.Format("StringWExpertForceUpdateMessage".GetLocalized(), shortVersion);
                confirmUpdate = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.WARN, false, "StringUpdate".GetLocalized(), null, true);
            }
            else
            {
                var title = "StringWExpertUpdate".GetLocalized();
                var message = string.Format("StringWExpertUpdateMessage".GetLocalized(), shortVersion);
                confirmUpdate = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.CHECK, true, "StringUpdate".GetLocalized(), "StringCancel".GetLocalized());
            }

            if (confirmUpdate)
            {
                ret = true;
                ProgressPercentage = 0;
                ProgressText = "Preparing to download...";

                try
                {
                    var progress = new Progress<UpdateFileDownloadProgress>(OnDownloadProgress);
                    var downloadedFile = await DownloadUpdateAsync(shortVersion, checkVersionOut.DownloadUrl, progress);

                    // 디버깅 임시
                    //downloadedFile = @"C:\Users\WinHome\AppData\Local\Temp\WExpert\WExpertSetup.exe";

                    // Setup 파일 실행 (외부 프로세스 실행)
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadedFile,
                        UseShellExecute = true
                    });

                    Environment.Exit(0);
                }
                catch (Exception e)
                {
                    await Task.Delay(1000); // 잠시 대기 후 에러 메시지 표시

                    WExpertLogger.Instance.Error($"{e}");

                    var title = "StringCommonErrorTitle".GetLocalized();
                    var message = string.Format("StringInstallErrorMessage".GetLocalized(), shortVersion);
                    var retry = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.ERROR, true, "StringRetry".GetLocalized(), "StringCancel".GetLocalized());

                    if (retry)
                    {
                        await AppUpdateCheck();
                    }
                    else
                    {
                        if (checkVersionOut.IsForceUpdate)
                        {
                            Environment.Exit(0); // 강제 업데이트인데 설치 취소를 할경우 프로그램 종료
                        }
                    }
                }
            }
            else
            {
                ret = false;
            }
        }

        return ret;
    }

    // 최신 버전 존재 유무 체크
    private async Task<CheckVersionOut?> CheckNewVersionAPIAsync()
    {
        try
        {
            var url = string.Format(ApiRoutes.CHECK_NEW_VERSION.Path, WExpertDefine.GetVersion(true));
            var response = await _restApiService.DataRequestAsync<CheckVersionOut>(ApiRoutes.CHECK_NEW_VERSION.Method, url,
                                                          ApiRoutes.CHECK_NEW_VERSION.RequiresFormData);
            if (response.Result is APIResultType.SUCCESS)
            {
                var checkVersionOut = response.Data as CheckVersionOut;
                return checkVersionOut;
            }

            throw new Exception($"Response is {response.Result}");
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"App version check error : {e}");
        }

        return null;
    }

    private void OnDownloadProgress(UpdateFileDownloadProgress progress)
    {
        // UI 스레드에서 실행되므로 직접 UI 업데이트 가능
        ProgressPercentage = progress.ProgressPercentage;
        ProgressText = progress.ProgressPercentage < 100 ? $"Downloading (v{progress.Version})" : "Download Complete!";
        //ProgressTotalRateText = $"{progress.ProgressPercentage}% ({progress.FormattedDownloadedSize} / {progress.FormattedTotalSize})";
    }

    private async Task<string> DownloadUpdateAsync(string version, string downloadUrl, IProgress<UpdateFileDownloadProgress>? progress = null)
    {
        try
        {            
            var filePath = Path.Combine(Path.GetTempPath(), "WExpert");
            Directory.CreateDirectory(filePath);

            var fileName = "WUpdate.exe";
            filePath = Path.Combine(filePath, fileName);

            using (var httpClient = new HttpClient())
            {
                using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var downloadedBytes = 0L;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                var buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (progress != null && totalBytes > 0)
                    {
                        var progressPercentage = (int)((downloadedBytes * 100) / totalBytes);
                        progress.Report(new UpdateFileDownloadProgress
                        {
                            ProgressPercentage = progressPercentage,
                            DownloadedBytes = downloadedBytes,
                            TotalBytes = totalBytes,
                            FileName = fileName,
                            Version = version,
                        });
                    }
                }
            }

            return filePath;
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"The update could not be completed.({ex.StatusCode})", ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"The update could not be completed.(1002)", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"The update could not be completed.(1001)", ex);
        }
    }
}
