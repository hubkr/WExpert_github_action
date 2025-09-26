using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using WExpert.Code;
using WExpert.Helpers;
using WExpert.Models;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.Views.Base;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Expander = Microsoft.UI.Xaml.Controls.Expander;

namespace WExpert.Views;

public sealed partial class AnalysisViewerPage : WEXBasePage, IServerResponseHandler
{
    private readonly FrameworkElement _visualRoot;
    public AnalysisViewerViewModel ViewModel { get; }
    private bool isDraggable = false;

    #region FileList Navigation 버튼 Fade In/Out 처리를 위한 설정
    private CancellationTokenSource? _fileListNavigationFadeOutCancellationTokenSource;
    private bool _isFileListNavigationBtnVisible = false;
    #endregion

    #region 멀티 선택(재분석,삭제)시 미리보기창 처리를 위한 설정
    private readonly DispatcherTimer? _flyoutTimer; // Flyout 표시를 위한 타이머
    private UIElement? _currentElement;   // 현재 PointerEntered된 요소 추적
    #endregion

    public AnalysisViewerPage()
    {
        InitializeComponent();

        _visualRoot = (FrameworkElement)App.MainWindow.Content;
        ViewModel = App.GetService<AnalysisViewerViewModel>();
        DataContext = ViewModel;

        // 리포트 화면 초기화
        ViewModel.InitAnalysisViewer(usViewer, AnalysisMenuColViewSource);

        // 이벤트 헨들러 등록
        ViewModel.ResponseHandler = this;

        #region 멀티 선택(재분석,삭제)시 미리보기창 처리를 위한 설정
        // 타이머 초기화
        _flyoutTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500) // 지연 시간 설정 (예: 500ms)
        };
        _flyoutTimer.Tick += FlyoutTimer_Tick;
        #endregion

        /*
        grdSpliter.PointerEntered += (s, e) =>
        {
            var gridSplitter = s as GridSplitter;

            if (gridSplitter != null)
            {
                gridSplitter.Pro GripperCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
            }
            App.MainWindow.AppWindow.Po = new CoreCursor(CoreCursorType.SizeWestEast, 0);
        };
        grdSpliter.PointerExited += (s, e) =>
        {
            App.MainWindow.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        };
        */

        // ViewModel 로 WebMessageReceived 이벤트 등록
        ReportWebView.WebMessageReceived += (sender, e) =>
        {
            ViewModel.HandleWebMessageReceived(e.WebMessageAsJson, ReportWebView);
        };

        // GridView 크기 변경 시 ScrollViewer의 가로 스크롤바 상태 확인 ///
        FileListScrollViewer.ViewChanged += (s, e) =>
        {            
            ViewModel.IsShowFileListHorizontalScrollBar = FileListScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        };

        FileListScrollViewer.SizeChanged += (s, e) =>
        {
            ViewModel.IsShowFileListHorizontalScrollBar = FileListScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        };

        FileListScrollViewer.Loaded += (s, e) =>
        {
            ViewModel.IsShowFileListHorizontalScrollBar = FileListScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        };

        FileListGridView.SizeChanged += (s, e) =>
        {
            ViewModel.IsShowFileListHorizontalScrollBar = FileListScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        };
        /////////////////////////////////////////////////////////////////////////////
    }


    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RequestPatientDataCommand.Execute(null);
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
#if false // TODO ... progress popup 추후 필요시 사용
        await MainWindow.PerformLongRunningTaskAsync();
#endif
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        //HideProgressPopup();
        ViewModel.StopAnalysisResultMonitoring();
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        #region FileList Navigation 버튼 Fade In/Out 처리를 위한 설정
        _fileListNavigationFadeOutCancellationTokenSource?.Cancel();
        _fileListNavigationFadeOutCancellationTokenSource?.Dispose();
        _fileListNavigationFadeOutCancellationTokenSource = null;
        #endregion
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private void DiagnosticMenu_Loaded(object sender, RoutedEventArgs e)
    {
        var listView = sender as ListView;
        if (listView != null)
        {
            listView.SelectedItem = null; // 선택된 항목 초기화
        }
    }

    private void DiagnosticMenu_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is DiagnosticMenu menu)
        {
            var lvItem = args.ItemContainer as ListViewItem;
            if (lvItem != null)
            {
                lvItem.IsEnabled = menu.MenuEnable;
                lvItem.IsHitTestVisible = menu.ROIEnable;
            }
        }
    }

    private async void FileList_DragEnter(object sender, DragEventArgs e)
    {
        // WExpertLogger.Instance.Debug("====================== FileList_DragEnter1");
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            // WExpertLogger.Instance.Debug("====================== FileList_DragEnter2");
            var files = items.OfType<StorageFile>();

            var supportedTypes = new[] { ".jpg", ".jpeg", ".png" };
            isDraggable = files.Any(file => supportedTypes.Contains(Path.GetExtension(file.Name).ToLowerInvariant()));
        }
        // WExpertLogger.Instance.Debug("====================== FileList_DragEnter3");
    }

    private void FileList_DragOver(object sender, DragEventArgs e)
    {
        // WExpertLogger.Instance.Debug(string.Format("====================== FileList_DragOver isDraggable : {0}", isDraggable));
        if (isDraggable)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "StringDropFile".GetLocalized();
            e.DragUIOverride.IsCaptionVisible = true;   // Sets if the caption is visible
            e.DragUIOverride.IsContentVisible = true;   // Sets if the dragged content is visible
            e.DragUIOverride.IsGlyphVisible = false;    // Sets if the glyph is visible  
        }
    }

    private async void FileList_Drop(object sender, DragEventArgs e)
    {
        // WExpertLogger.Instance.Debug(string.Format("====================== FileList_DragOver isDraggable : {0}", isDraggable));
        if (isDraggable && e.DataView.Contains(StandardDataFormats.StorageItems))
        {   
            var items = await e.DataView.GetStorageItemsAsync();
            var files = items.OfType<StorageFile>();          
            ViewModel.AddNewFileFromDropCommand.Execute(files.ToList());
        }
    }

    private void SliderZoom_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        var slider = (Slider)sender;
        if (slider.FocusState == FocusState.Unfocused)
        {
            return;
        }

        // 사용자 상호작용(사용자가 직접 슬라이드 조작)으로 인한 변경
        var zoomValue = (float)(e.NewValue / 100);
        usViewer.SetZoom(null, null, zoomValue,  false);
    }

    private void ConsultAnswer_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Image image)
        {
            // 대체 이미지 소스 설정 (BitmapImage 사용)
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/NoImageBig.png"));
        }
    }

    private void Thumbnail_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Image image)
        {
            ViewModel.ThumbnailOpenFailedCommand.Execute(image);
        }
    }

    private void Thumbnail_ImageOpened(object sender, RoutedEventArgs e)
    {
        if (sender is Image image)
        {
            ViewModel.ThumbnailOpenedCommand.Execute(image);
        }
    }

    private void Toolbar_Flyout_Opened(object sender, object e)
    {
        // Flyout 객체 가져오기
        var flyout = sender as Flyout;
        if (flyout != null)
        {
            // Flyout의 Target을 통해 버튼 객체 가져오기
            if (flyout.Target is Button parentButton)
            {
                // 리소스에서 SolidColorBrush 가져오기
                if (Application.Current.Resources.TryGetValue("BrushComponentsLtBrand", out var resource) && resource is SolidColorBrush brush)
                {
                    parentButton.Background = brush;
                }
            }
        }
    }

    private void Toolbar_Flyout_Closed(object sender, object e)
    {
        // Flyout 객체 가져오기
        var flyout = sender as Flyout;
        if (flyout != null)
        {
            // Flyout의 Target을 통해 버튼 객체 가져오기
            if (flyout.Target is Button parentButton)
            {
                parentButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }
    }

    /*
    private void ROIOutlineColorButton_Click(object sender, RoutedEventArgs e)
    {
        var toggle = sender as ToggleButton;
        if (toggle == null || toggle.IsChecked == null)
        {
            return;
        }

        var outlineType = ROIColorType.NONE;
        if ((bool)toggle.IsChecked && Enum.TryParse(toggle.Tag as string, out ROIColorType type))
        {
            outlineType = type;
            var toggleList = CommonUtils.FindVisualChildren<ToggleButton>(AreaOutlineGrid);
            foreach (var t in toggleList)
            {
                // 현재 토글 버튼이 아닌 경우 체크 해제
                if (t != toggle)
                {
                    t.IsChecked = false;
                }
            }
        }

        // ViewModel에 색상 설정
        ViewModel.ROIOutlineColorType = outlineType;
    }
    */

    private void MultiSelectionCheckAll_Click(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        if (checkBox == null)
        {
            return;
        }

        if (checkBox.IsChecked == true)
        {
            MultiSelectGridView.SelectAll();
        }
        else
        {
            MultiSelectGridView.SelectedItems.Clear();
        }
    }

    private void MultiSelectionPopup_Opened(object sender, object e)
    {
        #region 애니메이션 효과 정의
        var animation = new DoubleAnimation
        {
            From = 300, // 초기 Y 위치 (화면 하단)
            To = 0,     // 최종 Y 위치 (화면 상단, 본래 위치)
            Duration = new Duration(TimeSpan.FromMilliseconds(500)), // 애니메이션 지속 시간
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } // 부드러운 감속 효과
        };

        // 애니메이션 타겟 설정
        Storyboard.SetTarget(animation, MultiSelectListGridTransform);
        Storyboard.SetTargetProperty(animation, "Y");

        // Storyboard 실행
        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);

        // 애니메이션 완료 후 처리 (선택사항)
        animation.Completed += (sender, e) =>
        {
            MultiSelectListGridTransform.Y = 0;
        };

        storyboard.Begin();
        #endregion

        #region GridView Item Style 설정
        var style = ViewModel.CurrentMultiSelectionType switch
        {
            MultiSelectionType.DELETE => (Style)Application.Current.Resources["WEXBaseGridViewItemStyle1"],
            MultiSelectionType.RE_ANALYSIS => (Style)Application.Current.Resources["WEXBaseGridViewItemStyle2"],
            _ => null,
        };

        MultiSelectGridView.ItemContainerStyle = style;
        #endregion

        // Popup Open 시 OK 버튼 비활성 화
        ViewModel.EnableMultiSelectionTaskOKButton = false;
    }

    private void MultiSelectionPopup_Closed(object sender, object e)
    {
        // Popup이 닫힐 때 원래 위치로 복구(에니메이션 효과)
        MultiSelectListGridTransform.Y = 300;
    
        // 기존 사용 했던 값들을 초기화
        ViewModel.CheckAllMultiSelection = false;

        MultiSelectGridView.SelectedItems.Clear();
        foreach (var file in ViewModel.UltrasoundFiles)
        {
            file.Check = false;
            file.MultiSelectionMode = MultiSelectionType.NONE;
        }

        ViewModel.MultiSelectionStatusText = string.Empty;
        ViewModel.CurrentMultiSelectionType = MultiSelectionType.NONE;
    }


    private async void ReportPopup_Opened(object sender, object e)
    {
        try
        {
            ViewModel.IsActiveLoadingReportProgress = true;

            // WebView 초기화
            if (ReportWebView != null)
            {
#if true
                 ViewModel.ReportOptionType = AnalysisReportOptionType.ALL;

                // App의 쓰기 가능한 경로를 지정 (ex: C:\Users\<Username>\AppData\Local\WExpert)
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "WExpert");

                var webView2Environment = await CoreWebView2Environment.CreateWithOptionsAsync(
                    null, // browserExecutableFolder
                    userDataFolder, // userDataFolder
                    null // options
                );

                if (ReportWebView.CoreWebView2 == null)
                {
                    await ReportWebView.EnsureCoreWebView2Async(webView2Environment);
                    // 캐시 삭제
                    await ReportWebView.CoreWebView2?.Profile.ClearBrowsingDataAsync();
                }

                // Report URL로 이동
                ReportWebView.CoreWebView2?.Navigate(WExpertDefine.GetReportServerUrl());
#else // 디버깅용(로컬 HTML 파일 로드)
                // 실행 파일이 위치한 폴더 경로 가져오기
                var exeFolder = AppDomain.CurrentDomain.BaseDirectory;

                // 로드할 HTML 파일 경로 (실행파일 옆 assets 폴더 내 index.html이라 가정)
                var localHtmlPath = Path.Combine(exeFolder, "assets/html", "test.html");

                // file:/// 형식 URL로 변환 (슬래시 표기 주의)
                var localHtmlUrl = new Uri(localHtmlPath).AbsoluteUri;

                // WebView2에 로드
                ReportWebView.CoreWebView2.Navigate(localHtmlUrl);
#endif

                ReportWebView.Focus(FocusState.Programmatic);
            }
        }
        catch (Exception ex)
        {
            WExpertLogger.Instance.Error($"Report Popup Opened Error: {ex.Message}");
        }
        finally
        {
            await Task.Delay(500);
            ViewModel.IsActiveLoadingReportProgress = false;
        }
    }

    private void ReportPopup_Closed(object sender, object e)
    {
        // 기존 설정값 초기화
        if (ReportWebView != null)
        {
            ReportWebView.NavigateToString("");
            ViewModel.ReportChartNo = string.Empty;
            ViewModel.ReportOptionType = AnalysisReportOptionType.ALL;
            ViewModel.ReportAssessment = string.Empty;
            ViewModel.ReportBirthYear = null;
            ViewModel.ReportBirthMonth = null;
            ViewModel.ReportBirthDay = null;
            ViewModel.IsEnableReportControl = true;
        }

        ViewModel.IsActiveLoadingReportProgress = false;
    }

    private void ContentArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.ContentAreaWidth = ContentArea.ActualWidth;
        ViewModel.ContentAreaHeight = ContentArea.ActualHeight;
    }

    #region 멀티 선택(재분석,삭제)시 미리보기창 처리를 위한 설정
    private void FlyoutTimer_Tick(object? sender, object? e)
    {
        // 타이머 종료
        _flyoutTimer?.Stop();

        // 현재 요소가 유효한지 확인
        if (_currentElement != null)
        {
            // 현재 요소의 ContextFlyout 가져오기
            if (_currentElement is FrameworkElement element && element.ContextFlyout is Flyout flyout)
            {
                // Flyout 표시
                flyout.ShowAt(element);
                //WExpertLogger.Instance.Debug("FlyoutTimer_Tick flyout show ################");
            }
        }
    }

    private void MultiSelectGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // 현재 요소 저장
        //_currentElement = sender as UIElement;
        _currentElement = sender as Grid;

        WExpertLogger.Instance.Debug("PointerEntered ################");

        // 타이머 시작
        _flyoutTimer?.Start();
    }

    private void MultiSelectGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // 타이머 정지 및 초기화
        _flyoutTimer?.Stop();
        _currentElement = null;

        // Flyout 숨기기
        if (_currentElement is FrameworkElement element && element.ContextFlyout is Flyout flyout)
        //if (sender is Grid grd && grd.ContextFlyout is Flyout flyout)
        {
            flyout.Hide();

            WExpertLogger.Instance.Debug("PointerExited flyout hide ################");
        }
    }
    #endregion

    private void ResultConsultationTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var tabView = sender as TabView;
        if (tabView == null)
        {
            return;
        }

        ViewModel.ResultConsultationTabSelectIndex = tabView.SelectedIndex;

        // Consultation tab 선택시
        if (ViewModel.ResultConsultationTabSelectIndex == 1)
        {
            ViewModel.RefreshConsultationListCommand.Execute(null);
        }
    }


    private void ConsultationExpander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
    {
        // Expander가 펼쳐질 때 전체 텍스트 표시
        var expander = sender as Expander;
        var headerRichTextBlock = expander?.Header as RichTextBlock;

        if (headerRichTextBlock != null)
        {
            headerRichTextBlock.TextWrapping = TextWrapping.Wrap;
            headerRichTextBlock.TextTrimming = TextTrimming.None;
            headerRichTextBlock.MaxLines = int.MaxValue; // 제한 없음
        }
    }

    private void ConsultationExpander_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
    {
        // Expander가 접힐 때 한 줄로 말줄임표 표시
        var expander = sender as Expander;
        var headerRichTextBlock = expander?.Header as RichTextBlock;

        if (headerRichTextBlock != null)
        {
            headerRichTextBlock.TextWrapping = TextWrapping.NoWrap;
            headerRichTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            headerRichTextBlock.MaxLines = 1; // 한 줄로 제한
        }
    }

    private void FileListGridView_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var scrollViewer = FileListScrollViewer;
        if (scrollViewer != null)
        {
            var delta = e.GetCurrentPoint(scrollViewer).Properties.MouseWheelDelta;
            var scrollAmount = scrollViewer.ViewportWidth / 8;

            scrollViewer.ChangeView(scrollViewer.HorizontalOffset - Math.Sign(delta) * scrollAmount, null, null);
            e.Handled = true;
        }
    }

    private void FileListGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        #region 선택된 항목이 ScrollViewer의 뷰포트에서 벗어났을 경우 스크롤 이동
        var selectedItem = FileListGridView.SelectedItem;
        if (selectedItem == null)
        {
            return;
        }

        // 컨테이너(실제 아이템 Control) 찾기
        var container = FileListGridView.ContainerFromItem(selectedItem) as FrameworkElement;
        if (container == null)
        {
            return;
        }

        // ScrollViewer의 현재 뷰포트 정보
        var scrollViewerWidth = FileListScrollViewer.ViewportWidth;
        var currentHorizontalOffset = FileListScrollViewer.HorizontalOffset;

        // 컨테이너의 ScrollViewer 상대 좌표 구하기
        var transform = container.TransformToVisual(FileListScrollViewer);
        var containerPosition = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
        var containerWidth = container.ActualWidth;

        // 항목의 오른쪽 끝 좌표
        var containerRight = containerPosition.X + containerWidth;

        var targetHorizontalOffset = currentHorizontalOffset;
        // 항목이 왼쪽으로 벗어난 경우
        if (containerPosition.X < 0)
        {
            targetHorizontalOffset = currentHorizontalOffset + containerPosition.X;
        }
        // 항목이 오른쪽으로 벗어난 경우
        else if (containerRight > scrollViewerWidth)
        {
            targetHorizontalOffset = currentHorizontalOffset + (containerRight - scrollViewerWidth);
        }

        // 스크롤 위치가 변경되어야 하는 경우에만 이동
        if (Math.Abs(targetHorizontalOffset - currentHorizontalOffset) > 1)
        {
            FileListScrollViewer.ChangeView(targetHorizontalOffset, null, null, false);
        }
        #endregion
    }

    private void FileListScrollViewMultiSelect_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var scrollViewer = FileListScrollViewMultiSelect;
        if (scrollViewer != null)
        {
            var delta = e.GetCurrentPoint(scrollViewer).Properties.MouseWheelDelta;
            var scrollAmount = scrollViewer.ViewportWidth / 8;

            scrollViewer.ChangeView(scrollViewer.HorizontalOffset - Math.Sign(delta) * scrollAmount, null, null);
            e.Handled = true;
        }
    }

    private void MultiSelectDimArea_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        // dim 영역 클릭시 popup 닫기
        if (ViewModel.IsOpenMultiSelectPopup && !ViewModel.MultiSelectionTaskProcessing)
        {
            ViewModel.IsOpenMultiSelectPopup = false;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ReportOptionType))
        {
            ViewModel.SaveReportInformationCommand.Execute(ReportWebView);
        }
    }

    private void ResultConsult_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var width = SettingUtils.GetAnalysisWidth();
        if (e.NewSize.Width != width)
        {
            SettingUtils.SetAnalysisWidth(e.NewSize.Width);
        }
    }

    private void CloseAdminNoteFlyout_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn is not null && CommonUtils.FindParentControl<FlyoutPresenter>(btn) is FlyoutPresenter flyoutPresenter)
        {
            // FlyoutPresenter에서 Flyout 찾기
            if (flyoutPresenter.Parent is Popup popup)
            {
                popup.IsOpen = false;
                return;
            }
        }
    }

    public void HandleServerResponse(string type, object? responseData)
    {
        switch (type)
        {
            case "ClearAlgorithmsMenuListSelection":
                AlgorithmsMenuList.SelectedItems.Clear();
                break;
        }
    }

    private void AlgorithmsMenuList_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        //var ctrlDown = (App.MainWindow.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        //if (ctrlDown && e.Key == VirtualKey.A)
        if (e.Key == VirtualKey.A)
        {
            e.Handled = true; // Ctrl+A 무시
        }
        /*
        else if (e.Key == VirtualKey.Space)
        {
            ListView view = sender as ListView;
            DiagnosticMenu menu = view.SelectedItem as DiagnosticMenu;
            if (menu?.ROIEnable == false)
            {
                e.Handled = true;
            }            
        }
        */
    }


    private void GridUSViewer_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        #region FileList Navigation 버튼 Fade In/Out 처리를 위한 설정
        // 기존 fade out 작업 취소
        _fileListNavigationFadeOutCancellationTokenSource?.Cancel();
        _fileListNavigationFadeOutCancellationTokenSource = null;

        // 이미 보이는 상태가 아니면 fade in 시작
        if (!_isFileListNavigationBtnVisible)
        {
            _isFileListNavigationBtnVisible = true;
            FadeInStoryboardPrevious.Begin();
            FadeInStoryboardNext.Begin();
        }
        #endregion
    }

    private async void GridUSViewerGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        #region FileList Navigation 버튼 Fade In/Out 처리를 위한 설정
        // 새로운 CancellationTokenSource 생성
        _fileListNavigationFadeOutCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _fileListNavigationFadeOutCancellationTokenSource.Token;

        try
        {
            // 2초 대기 (취소 가능)
            await Task.Delay(2000, cancellationToken);

            // 취소되지 않았다면 fade out 실행
            if (!cancellationToken.IsCancellationRequested)
            {
                _isFileListNavigationBtnVisible = false;
                FadeOutStoryboardPrevious.Begin();
                FadeOutStoryboardNext.Begin();
            }
        }
        catch (OperationCanceledException)
        {
            // 취소된 경우 - 아무것도 하지 않음
        }
        finally
        {
            // 완료되면 CancellationTokenSource 정리
            if (_fileListNavigationFadeOutCancellationTokenSource != null && _fileListNavigationFadeOutCancellationTokenSource.Token == cancellationToken)
            {
                _fileListNavigationFadeOutCancellationTokenSource.Dispose();
                _fileListNavigationFadeOutCancellationTokenSource = null;
            }
        }
        #endregion
    }
}