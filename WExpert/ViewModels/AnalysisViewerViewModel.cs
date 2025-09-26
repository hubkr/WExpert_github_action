using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Contracts.ViewModels;
using WExpert.Controls;
using WExpert.Core.Models;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WExpert.ViewModels;

public partial class AnalysisViewerViewModel : ObservableRecipient, INavigationAware
{
    private readonly FrameworkElement _visualRoot;
    private readonly INavigationService _navigationService;
    private readonly IRestApiService _restApiService;
    private readonly IDialogService _dialogService;

    private readonly Dictionary<string, AnalysisResultOut> _analysisResults = [];
    private USViewerControl? usViewer = null;

    public IServerResponseHandler? ResponseHandler { get; set; }

    public ObservableCollection<DiagnosticMenu> DiagnosticMenuSource { get; } = [];
    public ObservableCollection<UltrasoundFileInfo> UltrasoundFiles { get; } = [];
    public ObservableCollection<ConsultationListItem> ConsultationListItems { get; } = [];
    public ObservableCollection<int> BirthYears { get; } = [];
    public ObservableCollection<int> BirthMonths { get; } = [];
    public ObservableCollection<int> BirthDays { get; } = [];

    public ICommand GoBackCommand { get; }
    public ICommand AnalysisMenuSelectionChangedCommand { get; }
    public ICommand PasteKeyDownCommand { get; }
    public IAsyncRelayCommand AddNewFileCommand { get; }
    public ICommand ReAnalysisFileCommand { get; }
    public ICommand DeleteFileCommand { get; }
    public ICommand ReportExportCommand { get; }
    public ICommand AddNewFileFromDropCommand { get; }
    public ICommand FlipVerticalCommand { get; }
    public ICommand FlipHorizontalCommand { get; }
    public ICommand FitToScreenCommand { get; }
    //public ICommand RealSizeCommand { get; }
    public ICommand RequestPatientDataCommand { get; }
    public ICommand ThumbnailOpenedCommand { get; }
    public ICommand ThumbnailOpenFailedCommand { get; }
    public ICommand ViewAdminNoteCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }
    public ICommand MovePreviousFileCommand { get; }
    public ICommand MoveNextFileCommand { get; }
    public ICommand MultiSelectionPopupOKCommand { get; }
    public ICommand MultiSelectionPopupCancelCommand { get; }
    public ICommand MultiSelectGridSelectionChangedCommand { get; }
    public ICommand NewConsultationCommand { get; }
    public ICommand RefreshConsultationListCommand  { get; }
    public ICommand ReportPopupCloseCommand { get; }
    public ICommand SaveReportInformationCommand { get; }
    public ICommand ExportReportCommand { get; }

    [ObservableProperty]
    private PatientOneDataOut? patientOneData = null;

    [ObservableProperty]
    private bool activeImageViewerProgressRing = false;

    [ObservableProperty]
    private bool activePageProgressRing = false;

    [ObservableProperty]
    private bool activeConsultationProgressRing = false;

    [ObservableProperty]
    private string activePageProgressRingMessage = string.Empty;

    [ObservableProperty]
    private string teachingTipTitle = string.Empty;

    [ObservableProperty]
    private string teachingTipSubtitle = string.Empty;

    [ObservableProperty]
    private bool teachingTipIsOpen = false;

    [ObservableProperty]
    private bool adminNoteFlyoutIsOpen = false;

    [ObservableProperty]
    private bool isTotalDiagnosingComplete = false;

    [ObservableProperty]
    private bool enableMovePreviousFileBtn = false;

    [ObservableProperty]
    private bool enableMoveNextFileBtn = false;

    [ObservableProperty]
    private bool isOpenMultiSelectPopup = false;

    [ObservableProperty]
    private bool isOpenReportPopup = false;

    [ObservableProperty]
    private bool checkAllMultiSelection = false;

    [ObservableProperty]
    private string multiSelectionStatusText = string.Empty;

    [ObservableProperty]
    private double contentAreaWidth = 0;

    [ObservableProperty]
    private double contentAreaHeight = 0;

    private bool _multiSelectionTaskProcessing = false;
    public bool MultiSelectionTaskProcessing // Multi Selection Task 진행 중 여부
    {
        get => _multiSelectionTaskProcessing;
        set
        {
            if (_multiSelectionTaskProcessing != value)
            {
                SetProperty(ref _multiSelectionTaskProcessing, value);
                // 선택된 항목이 있고 progress ring 이 표시 되지 않는 경우만 OK 버튼 enable
                EnableMultiSelectionTaskOKButton = (!value && UltrasoundFiles.Any(file => file.Check == true));
            }
        }
    }

    [ObservableProperty]
    private bool enableMultiSelectionTaskOKButton = false;

    [ObservableProperty]
    private Visibility emptyConsultationMessageVisible = Visibility.Visible;

    [ObservableProperty]
    private int consultationQuota = 0;

    [ObservableProperty]
    private int consultationUsed = 0;

    [ObservableProperty]
    private bool isActiveLoadingReportProgress = false;

    [ObservableProperty]
    private int resultConsultationTabSelectIndex = 0;

    [ObservableProperty]
    private GridLength analysisResultWidth = new(SettingUtils.GetAnalysisWidth());

    [ObservableProperty]
    private AnalysisReportOptionType reportOptionType = AnalysisReportOptionType.ALL;

    [ObservableProperty]
    private bool isShowFileListHorizontalScrollBar = false;

    [ObservableProperty]
    private bool isEnableReportControl = true;

    private bool _isEnableApplyButton = false;
    public bool IsEnableApplyButton
    {
        get => _isEnableApplyButton;
        set
        {
            if (_isEnableApplyButton != value)
            {
                SetProperty(ref _isEnableApplyButton, value);
            }
        }
    }
    
    private string _orginalReportChartNo = string.Empty;
    private string _reportChartNo = string.Empty;
    public string ReportChartNo
    {
        get => _reportChartNo;
        set
        {
            if (_reportChartNo != value)
            {
                SetProperty(ref _reportChartNo, value);
                IsEnableApplyButton = value != _orginalReportChartNo;
            }
        }
    }


    private string _orginalReportAssessment = string.Empty;
    private string _reportAssessment = string.Empty;
    public string ReportAssessment
    {
        get => _reportAssessment;
        set
        {
            if (_reportAssessment != value)
            {
                SetProperty(ref _reportAssessment, value);
                ReportAssessmentInputCount = _reportAssessment.Length;
                IsEnableApplyButton = value != _orginalReportAssessment;
            }
        }
    }

    [ObservableProperty]
    private int maxReportAssessmentLength = WExpertDefine.MAX_REPORT_ASSESSMENT_INPUT;

    [ObservableProperty]
    private int reportAssessmentInputCount = 0;

    private MultiSelectionType _currentMultiSelectionType = MultiSelectionType.NONE;
    public MultiSelectionType CurrentMultiSelectionType
    {
        get => _currentMultiSelectionType;
        set
        {
            if (_currentMultiSelectionType != value)
            {
                _currentMultiSelectionType = value;
                OnPropertyChanged(nameof(CurrentMultiSelectionType));

                // 모든 UltrasoundFileInfo에 전달
                foreach (var file in UltrasoundFiles)
                {
                    file.MultiSelectionMode = value;
                }
            }
        }
    }

    public float Zoom // Viewer scale
    {
        get => usViewer?.Zoom ?? 0;
        set
        {
            if (usViewer != null && usViewer.Zoom != value)
            {
                usViewer.Zoom = value;
                OnPropertyChanged(nameof(Zoom));
            }
        }
    }

    public int BrightnessSliderValue
    {
        get => usViewer?.Brightness ?? 0;
        set
        {
            if (usViewer != null && usViewer.Brightness != value)
            {
                usViewer.Brightness = value;
                OnPropertyChanged(nameof(BrightnessSliderValue));
            }
        }
    }

    public int ContrastSliderValue
    {
        get => usViewer?.Contrast ?? 0;
        set
        {
            if (usViewer != null && usViewer.Contrast != value)
            {
                usViewer.Contrast = value;
                OnPropertyChanged(nameof(ContrastSliderValue));
            }
        }
    }

    public int SharpnessSliderValue
    {
        get => usViewer?.Sharpness ?? 0;
        set
        {
            if (usViewer != null && usViewer.Sharpness != value)
            {
                usViewer.Sharpness = value;
                OnPropertyChanged(nameof(SharpnessSliderValue));
            }
        }
    }

    public bool FitToScreen // 화면 맞춤 보기
    {
        get => usViewer?.FitToScreen ?? false;
        set
        {
            if (usViewer != null && usViewer.FitToScreen != value)
            {
                usViewer.FitToScreen = value;
                OnPropertyChanged(nameof(FitToScreen));
            }
        }
    }

    public bool RealSize // 실제 이미지 크기 보기
    {
        get => usViewer?.RealSize ?? false;
        set
        {
            if (usViewer != null && usViewer.RealSize != value)
            {
                usViewer.RealSize = value;
                OnPropertyChanged(nameof(RealSize));
            }
        }
    }

    private UltrasoundFileInfo? _selectedUltrasoundFile = null;
    public UltrasoundFileInfo? SelectedUltrasoundFile
    {
        get => _selectedUltrasoundFile;
        set
        {
            if (_selectedUltrasoundFile != value)
            {
                SetProperty(ref _selectedUltrasoundFile, value);

                // 상단 툴바 메뉴 설정 초기화
                BrightnessSliderValue = 0;
                ContrastSliderValue = 0;
                SharpnessSliderValue = 0;
                usViewer?.SetFlip(null);

                if (value == null)
                {
                    EnableMovePreviousFileBtn = UltrasoundFiles.Count > 0;
                    EnableMoveNextFileBtn = UltrasoundFiles.Count > 0;
                }
                else
                {
                    var index = UltrasoundFiles.IndexOf(value);
                    // 이전으로 이동 가능 여부
                    EnableMovePreviousFileBtn = index > 0;
                    // 다음으로 이동 가능 여부
                    EnableMoveNextFileBtn = index >= 0 && index < UltrasoundFiles.Count - 1;
                }

                //ROIOutlineColor = Color.FromArgb(255, 255, 255, 255);
                //ROIOutlineThickness = 0;

                // 분석 메뉴 및 결과 출력 화면 초기화
                ResetResult(true);

                if (CommonUtils.IsValidUrl(value?.ImageUrl))
                {
                    LoadUltrasoundFile(value);
                    GetAnalysisResult(value);
                    
                    // 현재 Consultation Tab 이 선택 되어 있는 상태인 경우
                    if (ResultConsultationTabSelectIndex == 1)
                    {
                        OnRefreshConsultationList();
                    }
                }
                else
                {
                    LoadUltrasoundFile(null);
                }

                UpdateProgressRing();
            }
        }
    }

    private string _totalResultString = string.Empty;
    public string TotalResultString
    {
        get => _totalResultString;
        set
        {
            if (_totalResultString.CompareTo(value) != 0)
            {
                SetProperty(ref _totalResultString, value);
            }
        }
    }

    private string _totalResultFailString = string.Empty;
    public string TotalResultFailString
    {
        get => _totalResultFailString;
        set
        {
            if (_totalResultFailString.CompareTo(value) != 0)
            {
                SetProperty(ref _totalResultFailString, value);
            }
        }
    }    

    /*
    private ROIColorType _roiOutlineColorType = ROIColorType.NONE;
    public ROIColorType ROIOutlineColorType
    {
        get => _roiOutlineColorType;
        set
        {
            if (_roiOutlineColorType != value)
            {
                SetProperty(ref _roiOutlineColorType, value);
                usViewer?.SetROIMarking(ROIOutlineThickness,  DisplayUtils.ROIColorTypeToColor(value));
            }
        }
    }

    private int _roiOutlineThickness = 0;
    public int ROIOutlineThickness
    {
        get => _roiOutlineThickness;
        set
        {
            if (_roiOutlineThickness != value)
            {
                SetProperty(ref _roiOutlineThickness, value);
                usViewer?.SetROIMarking(value, DisplayUtils.ROIColorTypeToColor(ROIOutlineColorType));
            }
        }
    }
    */

    private bool _isFileOpenPickerOpen = false; // 중복 open 방지용 flag

    private int? _orginalReportBirthYear = null;
    private int? _reportBirthYear = null;
    public int? ReportBirthYear
    {
        get => _reportBirthYear;
        set
        {
            if (_reportBirthYear != value)
            {
                SetProperty(ref _reportBirthYear, value);
                UpdateReportDays();
                IsEnableApplyButton = value != _orginalReportBirthYear ;
            }
        }
    }

    private int? _orginalReportBirthMonth = null;
    private int? _reportBirthMonth = null;
    public int? ReportBirthMonth
    {
        get => _reportBirthMonth;
        set
        {
            if (_reportBirthMonth != value)
            {
                SetProperty(ref _reportBirthMonth, value);
                UpdateReportDays();
                IsEnableApplyButton = value != _orginalReportBirthMonth;
            }
        }
    }

    private int? _orginalReportBirthDay = null;
    private int? _reportBirthDay = null;
    public int? ReportBirthDay
    {
        get => _reportBirthDay;
        set
        {
            if (_reportBirthDay != value)
            {
                SetProperty(ref _reportBirthDay, value);
                IsEnableApplyButton = value != _orginalReportBirthDay;
            }
        }
    }

    //public PatientListItem? PatientItem
    //{
    //    get; private set;
    //}

    private int? PatientId = null;

    public AnalysisViewerViewModel(INavigationService navigationService, IRestApiService restApiService, IDialogService dialogService)
    {
        _visualRoot         = (FrameworkElement)App.MainWindow.Content;
        _navigationService  = navigationService;
        _restApiService     = restApiService;
        _dialogService      = dialogService;

        UltrasoundFiles.CollectionChanged += UltrasoundFiles_CollectionChanged;
        ConsultationListItems.CollectionChanged += ConsultationListItem_CollectionChanged;

        GoBackCommand = new RelayCommand(GoBack);
        AnalysisMenuSelectionChangedCommand = new RelayCommand<object>(parameter => OnAnalysisMenuSelectionChanged(parameter));
        PasteKeyDownCommand = new AsyncRelayCommand(OnPasteKeyDown);
        AddNewFileCommand = new AsyncRelayCommand(OnAddNewFile);
        ReAnalysisFileCommand = new RelayCommand(OnReAnalysisFile);
        DeleteFileCommand = new RelayCommand(OnDeleteFile);
        ReportExportCommand = new RelayCommand(OnReportExport);
        AddNewFileFromDropCommand = new AsyncRelayCommand<object>(parameter => OnAddNewFileFromDrop(parameter));
        FlipVerticalCommand = new RelayCommand(OnFlipVertical);
        FlipHorizontalCommand = new RelayCommand(OnFlipHorizontal);
        FitToScreenCommand = new RelayCommand(OnFitToScreen);
        //RealSizeCommand = new RelayCommand(OnRealSize);
        RequestPatientDataCommand = new AsyncRelayCommand<object>(parameter => OnRequestPatientData(parameter));
        ThumbnailOpenedCommand = new AsyncRelayCommand<object>(parameter => OnThumbnailOpened(parameter));
        ThumbnailOpenFailedCommand = new RelayCommand<object>(parameter => OnThumbnailOpenFailed(parameter));
        ExportCommand = new AsyncRelayCommand(OnExport);
        ViewAdminNoteCommand = new RelayCommand(OnViewAdminNote);
        MovePreviousFileCommand = new RelayCommand(OnMovePreviousFile);
        MoveNextFileCommand = new RelayCommand(OnMoveNextFile);
        MultiSelectionPopupOKCommand = new RelayCommand<object>(parameter => OnMultiSelectionPopupOK(parameter));
        ReportPopupCloseCommand = new RelayCommand<object>(parameter => OnReportPopupClose(parameter));
        SaveReportInformationCommand = new RelayCommand<object>(parameter => OnSaveReportInformation(parameter));
        ExportReportCommand = new RelayCommand<object>(parameter => OnExportReport(parameter));
        MultiSelectionPopupCancelCommand = new RelayCommand(OnMultiSelectionPopupCancel);
        MultiSelectGridSelectionChangedCommand = new RelayCommand<object>(parameter => OnMultiSelectGridSelectionChanged(parameter));
        NewConsultationCommand = new RelayCommand(OnNewConsultation);
        RefreshConsultationListCommand = new RelayCommand(OnRefreshConsultationList);
        _dialogService = dialogService;

        // Report 출력 창의 생년 월일을 위한 combo data 설정
        var thisYear = DateTime.Now.Year;
        for (var y = 1925; y <= thisYear; y++)
        {
            BirthYears.Add(y);
        }

        for (var m = 1; m <= 12; m++)
        {
            BirthMonths.Add(m);
        }

        for (var d = 1; d <= 31; d++)
        {
            BirthDays.Add(d);
        }
    }

    // Report 출력용 WebView2(WebMessageReceived) 위임 메서드
    public void HandleWebMessageReceived(string messageJson, WebView2 webview)
    {
        try
        {
            var messageObj = JsonConvert.DeserializeObject<WebMessageIn>(messageJson);
            if (messageObj != null)
            {
                if (messageObj.Type?.ToLower() == "initialized")
                {
                    DataSendToWebView(webview);
                }
                else if (messageObj.Type?.ToLower() == "progress" && messageObj.Payload is bool progress)
                {
                    IsActiveLoadingReportProgress = progress;
                }
                else if (messageObj.Type?.ToLower() == "open_print" && messageObj.Payload is bool isOpen)
                {
                    IsEnableReportControl = !isOpen;
                }
            }
        }
        catch (Exception ex)
        {
            WExpertLogger.Instance.Error("Report WebMessageReceived Error: " + ex.Message);
        }
    }

    private void UpdateReportDays()
    {
        // 년,월 이 정해 져야 day 를 설정 할 수 있으므로
        if (!ReportBirthYear.HasValue || !ReportBirthMonth.HasValue)
        {
            return;
        }

        // 현재 년,월에 해당하는 최대 일수
        var daysInMonth = DateTime.DaysInMonth(ReportBirthYear.Value, ReportBirthMonth.Value);

        // 현재 Combobox 에서 선택되어져 있는 day(ReportBirthDay)가 유효한지 확인, 초과 시 1로 초기화
        if (ReportBirthDay > daysInMonth)
        {
            ReportBirthDay = 1;
        }

        // 현재 리스트의 날짜가 유효한 최대일보다 크면 뒤에서부터 제거 (제거 로직)
        while (BirthDays.Count > daysInMonth)
        {
            BirthDays.RemoveAt(BirthDays.Count - 1);
        }

        // 현재 리스트의 날짜가 유효한 최대일보다 작으면 뒤에 추가 (추가 로직)
        var lastDayInList = BirthDays.Count; // 현재 리스트는 1부터 순차적으로 채워져 있는 가정
        for (var day = lastDayInList + 1; day <= daysInMonth; day++)
        {
            BirthDays.Add(day);
        }
    }

    private void UsViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // UserControl 의  Property 변경 시 현재 모델에서 바인당한 변경 알림 감지
        if (e.PropertyName == nameof(usViewer.Zoom))
        {
            OnPropertyChanged(nameof(Zoom));
        }
        else if (e.PropertyName == nameof(usViewer.RealSize))
        {
            OnPropertyChanged(nameof(RealSize));
        }
        else if (e.PropertyName == nameof(usViewer.FitToScreen))
        {
            OnPropertyChanged(nameof(FitToScreen));
        }
    }

    public void InitAnalysisViewer(USViewerControl ultrasoundViewer, CollectionViewSource collectionViewSource)
    {
        usViewer = ultrasoundViewer;

        // UserControl 의 Zoom Property 변경 시 현재 모델에서 바인당한 변경 알림 감지
        usViewer.PropertyChanged += UsViewPropertyChanged;

        var childMens = new List<DiagnosticMenu>();

        var loginInfo = _restApiService.GetLoginInfo();
        loginInfo?.AnalysisMenus?.ForEach(category =>
        {
            category.Items.ForEach(item =>
            {
                if (item != null)
                {
                    string? roiIcon = null;
                    if (item?.roiStatus?.Enable == true)
                    {
                        roiIcon = item.Id switch
                        {
                            WExpertAlgorithmsType.RUPTURE => $"ms-appx:///Assets/images/ROIMarkOnRupture.png",
                            WExpertAlgorithmsType.THICKENED_CAPSULE => $"ms-appx:///Assets/images/ROIMarkOnTC.png",
                            _ => $"ms-appx:///Assets/images/ROIMarkOn.png"
                        };
                    }

                    DiagnosticMenu menu = new()
                    {
                        Category = category.Category,
                        ROIIcon = roiIcon,
                        ROIIconBg = new SolidColorBrush(Colors.Transparent),
                        Name = item.Name,
                        Result = "-",
                        Id = item.Id,
                        ParentId = item.ParentId,
                        MenuEnable = item.Enable,
                        ROIEnable = item?.roiStatus?.Enable ?? false,
                        ResultTextColor = (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"]
                    };

                    // root 항목
                    if (menu.ParentId == null)
                    {
                        DiagnosticMenuSource.Add(menu);
                    }
                    else
                    {
                        // Child 항목(순서로 인해 앞에서 insert)
                        childMens.Insert(0, menu);
                    }
                }
            });

            // Child Menu들은 부모 뒤에 추가(1 depth 까지만 처리)
            childMens.ForEach(child =>
            {
                var parentIndex = -1;
                for (var i = 0; i < DiagnosticMenuSource.Count; i++)
                {
                    if (DiagnosticMenuSource[i].Id == child.ParentId)
                    {
                        parentIndex = i;
                        break;
                    }
                }

                // 부모 존재 시 부모 뒤에 추가(1 depth 까지만 처리)
                if (parentIndex >= 0)
                {
                    DiagnosticMenuSource.Insert(parentIndex + 1, child);
                }
                // 부모 미존재시 그냥 제일 뒤에 추가
                else
                {
                    DiagnosticMenuSource.Add(child); // 부모 못 찾으면 그냥 뒤에 추가
                }
            });            
        });

        // 그룹화된 데이터 설정(우측 분석 메뉴)
        var groupedContacts = GroupContactsByLastName(DiagnosticMenuSource);
        collectionViewSource.Source = new ObservableCollection<GroupInfoList>(groupedContacts);
    }

    private void UltrasoundFiles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add: // 추가
            case NotifyCollectionChangedAction.Remove: // 삭제
                if (SelectedUltrasoundFile == null)
                {
                    EnableMovePreviousFileBtn = UltrasoundFiles.Count > 0;
                    EnableMoveNextFileBtn = UltrasoundFiles.Count > 0;
                }
                else
                {
                    var index = UltrasoundFiles.IndexOf(SelectedUltrasoundFile);
                    // 이전으로 이동 가능 여부
                    EnableMovePreviousFileBtn = index > 0;
                    // 다음으로 이동 가능 여부
                    EnableMoveNextFileBtn = index >= 0 && index < UltrasoundFiles.Count - 1;
                }
                break;

            case NotifyCollectionChangedAction.Replace: // 교체
                // do nothing (SelectedUltrasoundFile 시 처리)
                break;

            case NotifyCollectionChangedAction.Reset: // 초기화
                // do nothing
                break;
        }
    }

    private void ConsultationListItem_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add: // 추가
                if (EmptyConsultationMessageVisible is Visibility.Visible)
                {
                    EmptyConsultationMessageVisible = Visibility.Collapsed;
                }
                break;
            case NotifyCollectionChangedAction.Remove: // 삭제
                if (EmptyConsultationMessageVisible is Visibility.Collapsed && ConsultationListItems.Count == 0)
                {
                    EmptyConsultationMessageVisible = Visibility.Visible;
                }
                break;
            case NotifyCollectionChangedAction.Replace: // 교체
                // do nothing 
                break;
            case NotifyCollectionChangedAction.Reset: // 초기화
                EmptyConsultationMessageVisible = Visibility.Visible;
                break;
        }
    }

    public bool DataSendToWebView(WebView2 webview)
    {
        try
        {
            var currentUrl = webview.CoreWebView2.Source;
            if (currentUrl is string url && !string.IsNullOrEmpty(url))
            {
                var accessToken = _restApiService.GetLoginInfo().AccessToken;
                accessToken ??= string.Empty;

                var analysisReportIn = new AnalysisReportIn()
                {
                    AccessToken = accessToken,
                    NativeVersion = WExpertDefine.GetVersion(true),
                    Id = PatientId ?? 0,
                    ExportOptionType = ReportOptionType,
                    ChartNo = ReportChartNo,
                    BirthYear = ReportBirthYear.HasValue ? ReportBirthYear.Value.ToString() : string.Empty,
                    BirthMonth = ReportBirthMonth.HasValue ? ReportBirthMonth.Value.ToString() : string.Empty,
                    BirthDay = ReportBirthDay.HasValue ? ReportBirthDay.Value.ToString() : string.Empty,
                    Assessment = ReportAssessment
                };

                var jsonString = JsonConvert.SerializeObject(analysisReportIn);
                webview.CoreWebView2.PostWebMessageAsJson(jsonString); // WebView2에 메시지 전송

                // 마지막 설정값 저장
                _orginalReportChartNo = ReportChartNo;
                _orginalReportAssessment = ReportAssessment;
                _orginalReportBirthYear = ReportBirthYear;
                _orginalReportBirthMonth = ReportBirthMonth;
                _orginalReportBirthDay = ReportBirthDay;

                // Apply Information 버튼 비활성화
                IsEnableApplyButton = false;
                
                return true;
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"Report Web View Data Send Error: {e}");
        }

        return false;
    }

    public void UpdateProgressRing()
    {
        if (SelectedUltrasoundFile != null)
        {
            var dispatcherQueue = App.MainWindow.DispatcherQueue;
            dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() =>
            {
                ActiveImageViewerProgressRing = SelectedUltrasoundFile?.AnalysisStatus == AnalysisStatusType.ANALYZING;
            }));
        }
    }

    public IEnumerable<GroupInfoList> GroupContactsByLastName(IEnumerable<DiagnosticMenu> menus)
    {
        var groupedMenus = new List<GroupInfoList>();
        foreach (var menu in menus)
        {
            var category = menu.Category;
            // 해당 그룹이 이미 존재하는지 확인하고, 없으면 새로운 그룹을 생성합니다.
            var existingGroup = groupedMenus.FirstOrDefault(g => g.Key.Equals(category));
            if (existingGroup == null)
            {
                existingGroup = new GroupInfoList(new List<DiagnosticMenu>()) { Key = category };
                groupedMenus.Add(existingGroup);
            }

            // 연락처를 해당 그룹에 추가합니다.
            existingGroup.Add(menu);
        }

        // 그룹화된 데이터를 반환합니다.
        return groupedMenus;
    }

    #region Analysis Monitoring Task
    private bool _analysisMonitoringTaskRunning = false;
    private ManualResetEventSlim? _analysisMonitoringPauseEvent;
    private CancellationTokenSource? _analysisMonitoringCancellationTokenSource;

    public void CreateAnalysisResultMonitoring()
    {
        if (!_analysisMonitoringTaskRunning)
        {
            _analysisMonitoringTaskRunning = true;
            _analysisMonitoringCancellationTokenSource = new CancellationTokenSource();
            _analysisMonitoringPauseEvent = new ManualResetEventSlim(true);
            Task.Run(() => AnalysisResultMonitoringAsync(_analysisMonitoringCancellationTokenSource.Token));
        }
    }

    public void PauseAnalysisResultMonitoring(bool pause)
    {
        if (pause)
        {
            _analysisMonitoringPauseEvent?.Reset(); // pause
        }
        else
        {
            _analysisMonitoringPauseEvent?.Set();  // resume
        }
    }

    public void StopAnalysisResultMonitoring()
    {
        _analysisMonitoringCancellationTokenSource?.Cancel();
        _analysisMonitoringTaskRunning = false;
    }

    // 분석 요청 안된 파일 존재하는 경우 분석요청 주기적인 체크
    private async Task AnalysisResultMonitoringAsync(CancellationToken token)
    {
        var dispatcherQueue = App.MainWindow.DispatcherQueue;

        while (!token.IsCancellationRequested)
        {
            try
            {                
#if DEBUG
                WExpertLogger.Instance.Debug($"[AnalysisViewer] Monitoring analysis status...{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
#endif
                _analysisMonitoringPauseEvent?.Wait(token); // Wait if paused

                var accessToken = _restApiService.GetLoginInfo().AccessToken;
                var url = string.Format(ApiRoutes.ANALYSIS_STATUS.Path, PatientId);
                var response = await _restApiService.DataRequestAsync<AnalysisStatusOut>(ApiRoutes.ANALYSIS_STATUS.Method, url,
                                                                                        ApiRoutes.ANALYSIS_STATUS.RequiresFormData, accessToken);
                if (response.Result == APIResultType.SUCCESS)
                {
                    var analysisStatusOut = response.Data;
                    if (analysisStatusOut != null)
                    {
                        dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() =>
                        {
                            var totalFileCount = analysisStatusOut.Total;
                            var performedCount = analysisStatusOut.Completed + analysisStatusOut.Incomplete;

                            if (totalFileCount != performedCount)
                            {
                                TotalResultString = string.Format("StringTotalResultProcessing".GetLocalized(), performedCount, totalFileCount);
                                TotalResultFailString = string.Empty;
                                IsTotalDiagnosingComplete = false;
                            }
                            else
                            {
                                TotalResultString = string.Format("StringTotalResultFinished".GetLocalized(), performedCount, totalFileCount);
                                TotalResultFailString = analysisStatusOut.Incomplete > 0 ? 
                                     string.Format("StringTotalResultFailed".GetLocalized(), analysisStatusOut.Incomplete, analysisStatusOut.Incomplete > 1 ? "s" : "")
                                    : string.Empty;
                                IsTotalDiagnosingComplete = true;
                            }

                            analysisStatusOut?.AnalysisSummaryDtoList?.ForEach(f =>
                            {
                                // item 확인
                                var item = UltrasoundFiles.FirstOrDefault(file => file.UltraSoundFileId.Equals(f.ImageId));

                                if (item != null)
                                {
                                    if (f.Progress.Equals("inProgress"))
                                    {
                                        item.AnalysisStatus = AnalysisStatusType.ANALYZING;
                                    }
                                    else if (f.Progress.Equals("success"))
                                    {
                                        item.AnalysisStatus = AnalysisStatusType.COMPLETED;
                                        // 현재 선택 되어져 있는 item 인 경우 분석 결과 화면 update
                                        if (item.UltraSoundFileId == SelectedUltrasoundFile?.UltraSoundFileId)
                                        {
                                            GetAnalysisResult(SelectedUltrasoundFile);
                                        }
                                    }
                                    else if (f.Progress.Equals("failure"))
                                    {
                                        item.AnalysisStatus = AnalysisStatusType.INCOMPLETE;
                                    }
                                    item.VisibilityRuptureTriage = f.RuptureTriage ? Visibility.Visible : Visibility.Collapsed;

                                    if (SelectedUltrasoundFile?.UltraSoundFileId == item.UltraSoundFileId)
                                    {
                                        UpdateProgressRing();
                                    }
                                }
                            });


                            // 수행이 완료 된 경우 monitoring task를 pause 시켜놓음
                            if (totalFileCount == performedCount)
                            {
                                PauseAnalysisResultMonitoring(true);
                            }
                        }));
                    }
                }
                else if (response.Result == APIResultType.UNAUTHORIZED)
                {
                    throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                }
                else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                {
                    throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                }
                else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                {
                    throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                    throw new Exception(message);
                }
            }
            catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
            {
                // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
                await _restApiService.LogoutAsync();
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested) // token 취소 로인한 exception 은 message 처리 안함.
                {
                    dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() =>
                    {
                        // 오류시 오류 메시지 출력
                        WExpertLogger.Instance.Error($"[AnalysisViewer] Monitoring analysis status error : {e}");

                        // Background 로 구동 중이므로 오류시 오류 메시지는 출력 하지않고 오류 로그만 저장
                        //await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
                    }));
                }
            }

            await Task.Delay(2000, token); // 2초에 한번씩
        }
    }
    #endregion

    private async void LoadUltrasoundFile(UltrasoundFileInfo? fileInfo)
    {
        try
        {
            var url = fileInfo?.ImageUrl;
            if (url is null)
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
                throw new Exception(message);
            }

            var response = await _restApiService.HttpGetByteArrayAsync(url);

            if (response.Result == APIResultType.SUCCESS)
            {
                if (usViewer != null && response.Data is not null)
                {
                    using var memoryStream = new MemoryStream(response.Data);
                    // BitmapImage 생성
                    var bi = new BitmapImage();
                    await bi.SetSourceAsync(memoryStream.AsRandomAccessStream());

                    // WriteableBitmap 복사본 생성
                    var copy = new WriteableBitmap(bi.PixelWidth, bi.PixelHeight);
                    memoryStream.Position = 0;
                    await copy.SetSourceAsync(memoryStream.AsRandomAccessStream());

                    usViewer.DisplaySource = copy;
                    usViewer.OriginalSource = copy;

                    // ScrollViewer.ChangeView 수행시 zoomFactor 변경 적용 안되는 이슈로 Delay 처리
                    await Task.Delay(100);
                    OnFitToScreen();
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
                    throw new Exception(message);
                }
            }
            else if (response.Result == APIResultType.UNAUTHORIZED)
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringUnauthorizedMessage".GetLocalized(), response.ResultCode);
                throw new UnauthorizedException(message);
            }
            else
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new Exception(message);
            }
        }
        catch (UnauthorizedException ue)
        {
            // 오류시 오류 메시지 출력 후 로그인 창으로 이동
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringUnauthorizedTitle".GetLocalized(), ue.Message, IconType.ERROR);
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, null, true);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            WExpertLogger.Instance.Error($"{e}");
            usViewer.DisplaySource = null;
        }
    }

    private void GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private void OnAnalysisMenuSelectionChanged(object? parameter)
    {
        if (parameter is not ListView listView)
        {
            return;
        }

        var selectedItems = listView.SelectedItems;
        if (selectedItems.Count == 0)
        {
            // 모두 선택 해제 시 (ROI Marking 제거)
            ResetResult(false);
            return;
        }

        // 선택된 알고리즘 타입 수집
        var algorithmTypes = selectedItems
            .OfType<DiagnosticMenu>()
            .Select(menu => menu.Id)
            .ToList();

        GetAnalysisResult(SelectedUltrasoundFile, algorithmTypes);

        // ROI 아이콘 배경 설정
        var selectedMenus = listView.SelectedItems.OfType<DiagnosticMenu>().ToHashSet();

        foreach (var item in listView.Items)
        {
            if (item is not DiagnosticMenu menu)
                continue;

            if (selectedMenus.Contains(menu) && !string.IsNullOrEmpty(menu.ROIIcon))
            {
                var resourceKey = menu.Id switch
                {
                    WExpertAlgorithmsType.RUPTURE => "BrushComponentsRed",
                    WExpertAlgorithmsType.THICKENED_CAPSULE => "BrushComponentsYellow",
                    _ => "BrushComponentsLtWhite"
                };

                if (Application.Current.Resources.TryGetValue(resourceKey, out var brush) && brush is SolidColorBrush solidBrush)
                {
                    menu.ROIIconBg = solidBrush;
                }
                else
                {
                    menu.ROIIconBg = new SolidColorBrush(Colors.Transparent);
                }
            }
            else
            {
                menu.ROIIconBg = new SolidColorBrush(Colors.Transparent);
            }
        }
    }

    public void OnNavigatedTo(NavigationMode mode, object? parameter)
    {
        if (parameter is Dictionary<string, object> parameters)
        {
            // PatientItem = parameters["PatientListItem"] as PatientListItem;
            var item = parameters["PatientListItem"] as PatientListItem;
            PatientId = item?.Id;
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private async void GetAnalysisResult(UltrasoundFileInfo? requestFileInfo, List<WExpertAlgorithmsType>? algorithmsTypes = null)
    {
        if (_visualRoot == null || requestFileInfo == null)
        {
            return;
        }

        try
        {
            // 분석 결과 확인
            AnalysisResultOut? analysisResultOut = null;

            if (_analysisResults.TryGetValue(requestFileInfo.UltraSoundFileId, out var value))
            {
                // 저장된 분석 결과가 존재 하는 경우(저장된 결과 사용)                
                analysisResultOut = value;
            }
            else
            {
                // 저장된 분석 결과가 존재 하지 않는 경우(서버로 요청)
                var accessToken = _restApiService.GetLoginInfo().AccessToken;
                var url = string.Format(ApiRoutes.ANALYSIS_RESULT.Path, requestFileInfo.UltraSoundFileId);
                var response = await _restApiService.DataRequestAsync<AnalysisResultOut>(ApiRoutes.ANALYSIS_RESULT.Method, url,
                                                                                     ApiRoutes.ANALYSIS_RESULT.RequiresFormData, accessToken);
                if (response.Result == APIResultType.SUCCESS)
                {
                    analysisResultOut = response.Data;
                    if (analysisResultOut == null)
                    {
                        var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_ERROR);
                        throw new Exception(message);
                    }

                    // 리스트에 존재 하는 파일인 경우에만 결과를 저장
                    if (UltrasoundFiles.Contains(requestFileInfo))
                    {
                        _analysisResults.TryAdd(requestFileInfo.UltraSoundFileId, analysisResultOut);
                    }
                }
                else if (response.Result == APIResultType.UNAUTHORIZED)
                {
                    throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                }
                else if (response.Result == APIResultType.FORBIDDEN)
                {
                    throw new ForbiddenException();
                }
                else if (response.Result == APIResultType.NOT_FOUND)
                {
                    throw new NotFundException();
                }
                else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                {
                    throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                }
                else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                {
                    throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                    throw new Exception(message);
                }
            }

            if (algorithmsTypes == null)
            {
                // 분석 메뉴 전체 update
                UpdateAnalysisResultMenu(requestFileInfo);
            }
            else
            {
                Dictionary <WExpertAlgorithmsType, JArray> algorithmPointsList = [];
                algorithmsTypes.ForEach(algorithmsType =>
                {
                    // 특정 분석 항목에 대한 분석 요청(분석 결과 메뉴에서 특정 항목 선택시 ROI Marking 출력)
                    var result = analysisResultOut?.Labels?.FirstOrDefault(r => r.Result_Type.ToEnumByDescription(WExpertAlgorithmsType.NONE) == algorithmsType);

                    // 합치는 방식으로 변경 (JArray에 여러 Points를 추가)
                    if (result?.Points != null && result.Points.Count > 0)
                    {
                        JArray pointArray = [];
                        foreach (var pt in result.Points)
                        {
                            pointArray.Add(pt);
                        }

                        algorithmPointsList.Add(algorithmsType, pointArray);
                    }
                });

                var analysisMenus = _restApiService.GetLoginInfo().AnalysisMenus;
                usViewer?.SetPathData(analysisMenus, algorithmPointsList);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (NotFundException)
        {
            // 분석 결과가 존재 하지 않는 경우 그냥 무시 처리(추후 재 요청 가능 하도록..)
            WExpertLogger.Instance.Debug($"[AnalysisViewer] Analysis result not found for {requestFileInfo.UltraSoundFileId}");
        }
        catch (ForbiddenException)
        {
            WExpertLogger.Instance.Debug($"[AnalysisViewer] Can't access UltraSoundFileId({requestFileInfo.UltraSoundFileId}) image.");
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"[AnalysisViewer]Get analysis result error : {e}");
            //await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
            //requestFileInfo.AnalysisStatus = AnalysisStatusType.ERROR;
        }
    }

    private void ResetResult(bool initMenuResult)
    {
        // 분석 메뉴 선택 및 선택된 분석 결과를 출력한 창 초기화
        usViewer?.ClearCanvas(); // 기존 사용 하던 Path 존재시 삭제 처리        

        foreach (var menu in DiagnosticMenuSource)
        {
            if (initMenuResult)
            {
                menu.ResultReset(); // 결과 초기화
            }
                
            menu.ROIIconBg = new SolidColorBrush(Colors.Transparent);
        }

        // 현재 선택된 항목 선택 초기화
        ResponseHandler?.HandleServerResponse("ClearAlgorithmsMenuListSelection", null);
    }

    private async Task<bool> ReAnalysis(object? parameter)
    {
        try
        {
            if (parameter is IList<object> selectedItems && selectedItems.Count > 0)
            {
                var infos = new List<UltrasoundFileInfo>(selectedItems.OfType<UltrasoundFileInfo>());

                // UltraSoundFileId를 List<string> 으로 추출
                var ultraSoundFileIds = infos.Select(i => i.UltraSoundFileId).ToList();
                var inData = new
                {
                    sonographyIds = ultraSoundFileIds
                };

                var accessToken = _restApiService.GetLoginInfo().AccessToken;
                var url = ApiRoutes.ANALYSIS_REACTION.Path;
                var response = await _restApiService.DataRequestAsync<object>(ApiRoutes.ANALYSIS_REACTION.Method, url,
                                                                           ApiRoutes.ANALYSIS_REACTION.RequiresFormData, accessToken, inData);
                if (response.Result == APIResultType.SUCCESS)
                {
                    infos.ForEach(info =>
                    {
                        // 저장된 분석 결과는 삭제
                        _analysisResults.Remove(info.UltraSoundFileId);
                        // 현재 선택 되어 있는 창과 동일한 경우
                        if (info.UltraSoundFileId == SelectedUltrasoundFile?.UltraSoundFileId)
                        {
                            // image view 및 menu reset
                            ResetResult(true);
                        }

                        info.AnalysisStatus = AnalysisStatusType.NONE;
                        info.VisibilityRuptureTriage = Visibility.Collapsed;

                        // 분석 결과 메뉴 업데이트
                        UpdateAnalysisResultMenu(info);
                    });


                    // monitoring task resume
                    PauseAnalysisResultMonitoring(false);
                }
                else if (response.Result == APIResultType.UNAUTHORIZED)
                {
                    throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                }
                else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                {
                    throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                }
                else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                {
                    throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                    throw new Exception(message);
                }

                return true;
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[AnalysisViewer]Request re-analysis error : {e}");
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }

        return false;
    }

    private async Task<bool> DeleteFiles(object? parameter)
    {
        try
        {
            if (parameter is IList<object> selectedItems && selectedItems.Count > 0)
            {
                var title = string.Empty;
                var message = string.Empty;
                var count = UltrasoundFiles.Count;

                // 현재 목록에 남은 개수가 1개인 경우 삭제 불가 처리
                if (UltrasoundFiles.Count == selectedItems.Count)
                {
                    title = "StringNotDeleteFileTitle".GetLocalized();
                    message = "StringFileNotDeleteMessage".GetLocalized();
                    await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.INFO, false);
                    return false;
                }

                title = "StringConfirmDeleteTitle".GetLocalized();
                message = "StringConfirmDeleteMessage".GetLocalized();
                var result = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.WARN, true);
                if (result)
                {
                    var infos = new List<UltrasoundFileInfo>(selectedItems.OfType<UltrasoundFileInfo>());
                    // UltraSoundFileId를 List<string> 으로 추출
                    var ultraSoundFileIds = infos.Select(i => i.UltraSoundFileId).ToList();

                    var jsonObject = new
                    {
                        sonographiesToDelete = ultraSoundFileIds
                    };
                    var jsonString = JsonConvert.SerializeObject(jsonObject);

                    var multipartContent = new MultipartFormDataContent();
                    var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    multipartContent.Add(jsonContent, "patient");

                    var token = _restApiService.GetLoginInfo().AccessToken;
                    var url = string.Format(ApiRoutes.PATIENT_UPDATE.Path, PatientId);
                    var response = await _restApiService.DataRequestAsync<object>(ApiRoutes.PATIENT_UPDATE.Method, url, ApiRoutes.PATIENT_UPDATE.RequiresFormData, token, multipartContent);
                    if (response.Result == APIResultType.SUCCESS)
                    {
                        infos.ForEach(info =>
                        {
                            // 하단 해당 파일 목록에서 제거
                            UltrasoundFiles.Remove(info);
                            // 분석 결과 존재 시 삭제
                            _analysisResults.Remove(info.UltraSoundFileId);
                        });

                        // 기존 선택되어져 목록이 삭제시 선택 이동(첫번째 item 으로)
                        if (SelectedUltrasoundFile == null || !UltrasoundFiles.Contains(SelectedUltrasoundFile))
                        {
                            // SelectedUltrasoundFile = CommonUtils.GetNextOrPreviousItem(UltrasoundFiles, SelectedUltrasoundFile);
                            SelectedUltrasoundFile = UltrasoundFiles[0];
                        }

                        // monitoring task resume(하단 목록 전체 상태 update를 위해 resume 처리)
                        PauseAnalysisResultMonitoring(false);
                    }
                    else if (response.Result == APIResultType.UNAUTHORIZED)
                    {
                        throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                    }
                    else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                    {
                        throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                    }
                    else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                    {
                        throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                    }
                    else
                    {
                        message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                        throw new Exception(message);
                    }
                }
                return true;
            }
            else
            {
                var title = "StringNotPerformedTitle".GetLocalized();
                var message = "StringFileNotSelectDeleteMessage".GetLocalized();
                await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.INFO, false);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[AnalysisViewer]Delete file error : {e}");
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }

        return false;
    }

    private void UpdateAnalysisResultMenu(UltrasoundFileInfo info)
    {
        if (SelectedUltrasoundFile == null || SelectedUltrasoundFile.UltraSoundFileId != info.UltraSoundFileId
            || info.AnalysisStatus == AnalysisStatusType.INCOMPLETE)
        {
            return;
        }

        // 분석 결과 확인
        var resultExist = _analysisResults.TryGetValue(info.UltraSoundFileId, out var analysisResultOut);
        foreach (var item in DiagnosticMenuSource)
        {
            if (!resultExist || !item.MenuEnable)
            { 
                item.Result = "-";
                item.ResultTextColor = (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"];
                continue;
            }

            var result = analysisResultOut?.Labels?.FirstOrDefault(r => r.Result_Type.ToEnumByDescription(WExpertAlgorithmsType.NONE) == item. Id);
            if (result == null)
            {
                item.Result = "-";
            }
            else
            {
                var convType = result?.Result_Type?.ToEnumByDescription(WExpertAlgorithmsType.NONE);
                item.Result = convType switch
                {
                    // 보형물 정보(Implant Information) ///
                    WExpertAlgorithmsType.POCKET_POSITION or WExpertAlgorithmsType.SHELL_TYPE or
                    WExpertAlgorithmsType.SHAPE_TYPE or WExpertAlgorithmsType.MANUFACTURER or
                    WExpertAlgorithmsType.CONSTITUENT =>
                        string.IsNullOrEmpty(result?.Result_Class) ? "-"
                            // 서버에서 전달 받은 문자를 클라이언트 출력을 위한 문자 열로 변환(현재 서버에서 올바른 문자열을 내려줄수 없는 상태라 클라이언트에서 임시로 처리)
                            : result.Result_Class.Trim().ToLowerInvariant() switch
                            {
                                 "subglandular" => "Subglandular",
                                 "subpectoral" => "Subpectoral",
                                 "textual" => "Texture",
                                 "smooth" => "Smooth",
                                 _ => result.Result_Class!
                            },
                    // 부작용(Complication) ///
                    WExpertAlgorithmsType.FOLDING or WExpertAlgorithmsType.FLUID_COLLECTION or
                    WExpertAlgorithmsType.THICKENED_CAPSULE or WExpertAlgorithmsType.UPSIDE_DOWN_ROTATION or
                    WExpertAlgorithmsType.RUPTURE or WExpertAlgorithmsType.CAPSULAR_MASS or WExpertAlgorithmsType.CAPSULAR_CALCIFICATION or
                    WExpertAlgorithmsType.SILICONE_INVASION_TO_CAPSULE or WExpertAlgorithmsType.SILICONE_INVASION_TO_LN
                    =>
                        string.IsNullOrEmpty(result?.Result_Class) ? "-"
                            : result.Result_Class.Trim().ToLowerInvariant() switch
                            {
                                "exist" => "StringPositive".GetLocalized(),
                                _ => "StringNegative".GetLocalized()
                            },
                    _ => "-"
                };
            }

            // Triage(Rupture, TC)에서 Positive 인경우 각 TC의 대표 색깔과 동일 하게 결과 Text를 출력
            //if (result?.Points?.HasValues == true)
            //{
            //    item.ResultTextColor = item.Id switch
            //    {
            //        WExpertAlgorithmsType.RUPTURE => (SolidColorBrush)Application.Current.Resources["BrushComponentsRed"],
            //        WExpertAlgorithmsType.SILICONE_INVASION_TO_CAPSULE => (SolidColorBrush)Application.Current.Resources["BrushComponentsRed"],
            //        WExpertAlgorithmsType.SILICONE_INVASION_TO_LN => (SolidColorBrush)Application.Current.Resources["BrushComponentsRed"],
            //        WExpertAlgorithmsType.THICKENED_CAPSULE => (SolidColorBrush)Application.Current.Resources["BrushComponentsYellow"],
            //        _ => (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"]
            //    };
            //}
            //else
            //{
            //    item.ResultTextColor = (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"];
            //}

            switch (item.Id)
            {
                case WExpertAlgorithmsType.RUPTURE:
                case WExpertAlgorithmsType.SILICONE_INVASION_TO_CAPSULE:
                case WExpertAlgorithmsType.SILICONE_INVASION_TO_LN:
                    item.ResultTextColor = string.Compare(item.Result, "StringPositive".GetLocalized()) == 0 ? 
                        (SolidColorBrush)Application.Current.Resources["BrushComponentsRed"] : (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"];
                    break;
                case WExpertAlgorithmsType.THICKENED_CAPSULE:
                    item.ResultTextColor = string.Compare(item.Result,"StringPositive".GetLocalized()) == 0 ?
                        (SolidColorBrush)Application.Current.Resources["BrushComponentsYellow"] : (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"];
                    break;
                default:
                    item.ResultTextColor = (SolidColorBrush)Application.Current.Resources["BrushTextSecondary"];
                    break;
            }
        }
    }

    private async Task OnPasteKeyDown()
    {
        try
        {
            var dataPackageView = Clipboard.GetContent();
            // 클립보드에 이미지 데이터가 없는 경우
            if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
            {
                return;
            }

            // 최대 등록 가능 갯수 Check
            if (UltrasoundFiles.Count >= WExpertDefine.MAX_REGISTRATION_COUNT)
            {
                var message = string.Format("StringNewRegistrationFileLimit2".GetLocalized(), WExpertDefine.MAX_REGISTRATION_COUNT);
                throw new FileCountExceedException(message);
            }

            var tmpPath = await FileUtils.MakeTmpFromClipboaard(_visualRoot, dataPackageView);
            if (!string.IsNullOrEmpty(tmpPath))
            {
                // 신규 등록 파일을 서버에 등록
                var multipartContent = new MultipartFormDataContent();
                if (File.Exists(tmpPath))
                {
                    var fileName = Path.GetFileName(tmpPath);
                    var mimeType = FileUtils.GetMimeType(tmpPath);
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(tmpPath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                    multipartContent.Add(fileContent, "file", fileName);
                }
                else
                {
                    // 파일 저장 실패 시
                    throw new RegistrationException("StringSaveFileErrorMessage".GetLocalized());
                }

                var token = _restApiService.GetLoginInfo().AccessToken;
                var url = string.Format(ApiRoutes.PATIENT_UPDATE.Path, PatientId);
                var response = await _restApiService.DataRequestAsync<PatientOneDataOut>(ApiRoutes.PATIENT_UPDATE.Method, url, ApiRoutes.PATIENT_UPDATE.RequiresFormData, token, multipartContent);
                if (response.Result == APIResultType.SUCCESS)
                {
                    var sonographies = response?.Data?.Sonographies;
                    if (sonographies is not null)
                    {
                        foreach (var item in sonographies)
                        {
                            // 동일 path item 존재 확인
                            var itemToExist = UltrasoundFiles.FirstOrDefault(f => f.UltraSoundFileId.Equals(item.Id));
                            // 기존 동일 항목이 존재하지 않는 경우
                            if (itemToExist == null && !string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ImageUrl))
                            {
                                var info = new UltrasoundFileInfo(item.Id, item.ImageUrl, item.OriginalFileName, false);
                                UltrasoundFiles.Add(info);
                            }
                        }
                    }

                    // monitoring task resume
                    PauseAnalysisResultMonitoring(false);
                }
                else if (response.Result == APIResultType.UNAUTHORIZED)
                {
                    throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                }
                else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                {
                    throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                }
                else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                {
                    throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                    throw new Exception(message);
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (FileCountExceedException fce)
        {
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "AppDisplayName".GetLocalized(), fce.Message, IconType.INFO, false);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[AnalysisViewer]Paste error : {e}");
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }
    }

    private void OnFlipVertical()
    {
        usViewer.SetFlip(true);
    }

    private void OnFlipHorizontal()
    {
        usViewer.SetFlip(false);
    }

    private void OnFitToScreen()
    {
        usViewer.SetFitToScreen();
    }

    //private void OnRealSize()
    //{
    //    usViewer.SetRealSize();
    //}

    private async Task OnAddNewFile()
    {
        try
        {
            // 중복 open 방지
            if (_isFileOpenPickerOpen)
            {
                return;
            }

            _isFileOpenPickerOpen = true;

            // 파일 추가 개수 초과시 처리
            if (UltrasoundFiles.Count >= WExpertDefine.MAX_REGISTRATION_COUNT)
            {
                var title = "AppDisplayName".GetLocalized();
                var message = string.Format("StringNewRegistrationFileLimit2".GetLocalized(), WExpertDefine.MAX_REGISTRATION_COUNT);
                await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.INFO);
                return;
            }

            var hwnd = App.MainWindow.GetWindowHandle();
            var filePicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                ViewMode = PickerViewMode.Thumbnail
            };
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            //filePicker.FileTypeFilter.Add(".dcm");
            InitializeWithWindow.Initialize(filePicker, hwnd);

            var files = await filePicker.PickMultipleFilesAsync();

            // 최대 등록 가능 갯수 Check
            if ((UltrasoundFiles.Count + files.Count) > WExpertDefine.MAX_REGISTRATION_COUNT)
            {
                var message = string.Format("StringNewRegistrationFileLimit2".GetLocalized(), WExpertDefine.MAX_REGISTRATION_COUNT);
                throw new FileCountExceedException(message);
            }

            if (files != null && files.Count > 0)
            {
                var multipartContent = new MultipartFormDataContent();
                for (var i = 0; i < files.Count; i++)
                {
                    var path = files[i]?.Path;
                    if (File.Exists(path))
                    {
                        var fileName = Path.GetFileName(path);
                        var mimeType = FileUtils.GetMimeType(path);
                        var fileContent = new ByteArrayContent(File.ReadAllBytes(path));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                        multipartContent.Add(fileContent, "file", fileName);
                    }
                }

                if (multipartContent.Count() == 0)
                {
                    // 파일 저장 실패 시
                    throw new RegistrationException("StringSaveFileErrorMessage".GetLocalized());
                }

                // 신규 등록 파일을 서버에 등록
                var token = _restApiService.GetLoginInfo().AccessToken;
                var url = string.Format(ApiRoutes.PATIENT_UPDATE.Path, PatientId);
                var response = await _restApiService.DataRequestAsync<PatientOneDataOut>(ApiRoutes.PATIENT_UPDATE.Method, url, ApiRoutes.PATIENT_UPDATE.RequiresFormData, token, multipartContent);
                if (response.Result == APIResultType.SUCCESS)
                {
                    var sonographies = response?.Data?.Sonographies;
                    if (sonographies is not null)
                    {
                        foreach (var item in sonographies)
                        {
                            // 동일 path item 존재 확인
                            var itemToExist = UltrasoundFiles.FirstOrDefault(f => f.UltraSoundFileId.Equals(item.Id));
                            // 기존 동일 항목이 존재하지 않는 경우
                            if (itemToExist == null && !string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ImageUrl))
                            {
                                var info = new UltrasoundFileInfo(item.Id, item.ImageUrl, item.OriginalFileName, false);
                                UltrasoundFiles.Add(info);
                            }
                        }
                    }

                    // monitoring task resume
                    PauseAnalysisResultMonitoring(false);
                }
                else if (response.Result == APIResultType.UNAUTHORIZED)
                {
                    throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                }
                else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                {
                    throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                }
                else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                {
                    throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                    throw new RegistrationException(message);
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (FileCountExceedException fce)
        {
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "AppDisplayName".GetLocalized(), fce.Message, IconType.INFO, false);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[AnalysisViewer]Add new file error : {e}");
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }
        finally
        {
            _isFileOpenPickerOpen = false;
        }
    }

    private void OnReAnalysisFile()
    {
        CurrentMultiSelectionType = MultiSelectionType.RE_ANALYSIS;
        IsOpenMultiSelectPopup = true;
    }

    private void OnDeleteFile()
    {
        CurrentMultiSelectionType = MultiSelectionType.DELETE;
        IsOpenMultiSelectPopup = true;
    }

    private void OnReportExport()
    {
        IsOpenReportPopup = true;
    }

    private async void OnMultiSelectionPopupOK(object? parameter)
    {        
        if (CurrentMultiSelectionType == MultiSelectionType.RE_ANALYSIS) // 재분석
        {
            MultiSelectionTaskProcessing = true;
            await Task.Delay(500);
            var result = await ReAnalysis(parameter);
            MultiSelectionTaskProcessing = false;
            IsOpenMultiSelectPopup = !result;
        }
        else if (CurrentMultiSelectionType == MultiSelectionType.DELETE) // 삭제
        {
            MultiSelectionTaskProcessing = true;
            await Task.Delay(500);
            var result = await DeleteFiles(parameter);
            MultiSelectionTaskProcessing = false;
            IsOpenMultiSelectPopup = !result;
        }      
    }

    private void OnReportPopupClose(object? parameter)
    {
        IsOpenReportPopup = false;
    }

    private async void OnSaveReportInformation(object? parameter)
    {
        // Popup 창이 열려 있는 경우에만 데이터 전송 (창이 안열려도 Page 생성시 호출 되는 경우가 있어 체크 필요)
        if (IsOpenReportPopup && parameter is WebView2 webview)
        {
            IsActiveLoadingReportProgress = true;
            DataSendToWebView(webview);
            await Task.Delay(700);
            IsActiveLoadingReportProgress = false;
        }
    }

    private async void OnExportReport(object? parameter)
    {
        if (parameter is WebView2 webview)
        {
            try
            {
                // 적용 되지 않은 환자 정보가 존재 하는 경우 환자 정보 적용후 Export
                if (IsEnableApplyButton)
                {
                    OnSaveReportInformation(webview);
                    await Task.Delay(1000);
                }

                var analysisReportRequestPrintIn = new AnalysisReportRequestPrintIn { RrequestPrint = true };
                var jsonString = JsonConvert.SerializeObject(analysisReportRequestPrintIn);
                webview.CoreWebView2.PostWebMessageAsJson(jsonString);
            }
            catch (Exception e)
            {
                WExpertLogger.Instance.Error("Export Report Error: " + e.Message);
            }
        }
    }

    private void OnMultiSelectionPopupCancel()
    {
        IsOpenMultiSelectPopup = false;
    }

    private async Task OnAddNewFileFromDrop(object? parameter)
    {
        if (parameter is not List<StorageFile> files)
        {
            return;
        }

        try
        {
            var supportedTypes = new[] { ".jpg", ".jpeg", ".png" };
            var addFiles = files.Where(file => supportedTypes.Contains(Path.GetExtension(file.Name).ToLowerInvariant())).ToList();

            // 최대 등록 가능 갯수 Check
            if ((UltrasoundFiles.Count + addFiles.Count) > WExpertDefine.MAX_REGISTRATION_COUNT)
            {
                var message = string.Format("StringNewRegistrationFileLimit2".GetLocalized(), WExpertDefine.MAX_REGISTRATION_COUNT);
                throw new FileCountExceedException(message);
            }

            if (addFiles != null && addFiles.Count > 0)
            {
                var multipartContent = new MultipartFormDataContent();

                for (var i = 0; i < addFiles.Count; i++)
                {
                    var path = addFiles[i]?.Path;
                    if (File.Exists(path))
                    {
                        var fileName = Path.GetFileName(path);
                        var mimeType = FileUtils.GetMimeType(path);
                        var fileContent = new ByteArrayContent(File.ReadAllBytes(path));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                        multipartContent.Add(fileContent, "file", fileName);
                    }
                }

                if (multipartContent.Any())
                {
                    // 신규 등록 파일을 서버에 등록
                    var token = _restApiService.GetLoginInfo().AccessToken;
                    var url = string.Format(ApiRoutes.PATIENT_UPDATE.Path, PatientId);
                    var response = await _restApiService.DataRequestAsync<PatientOneDataOut>(ApiRoutes.PATIENT_UPDATE.Method, url, ApiRoutes.PATIENT_UPDATE.RequiresFormData, token, multipartContent);
                    if (response.Result == APIResultType.SUCCESS)
                    {
                        var sonographies = response?.Data?.Sonographies;
                        if (sonographies is not null)
                        {
                            foreach (var item in sonographies)
                            {
                                // 동일 path item 존재 확인
                                var itemToExist = UltrasoundFiles.FirstOrDefault(f => f.UltraSoundFileId.Equals(item.Id));
                                // 기존 동일 항목이 존재하지 않는 경우
                                if (itemToExist == null && !string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ImageUrl))
                                {
                                    var info = new UltrasoundFileInfo(item.Id, item.ImageUrl, item.OriginalFileName, false);
                                    UltrasoundFiles.Add(info);
                                }
                            }
                        }

                        // monitoring task resume
                        PauseAnalysisResultMonitoring(false);
                    }
                    else if (response.Result == APIResultType.UNAUTHORIZED)
                    {
                        throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                    }
                    else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                    {
                        throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                    }
                    else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                    {
                        throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                    }
                    else
                    {
                        var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                        throw new Exception(message);
                    }
                }
                else
                {
                    // 파일 저장 실패 시
                    throw new RegistrationException("StringSaveFileErrorMessage".GetLocalized());
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (FileCountExceedException fce)
        {
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "AppDisplayName".GetLocalized(), fce.Message, IconType.INFO, false);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[AnalysisViewer]Add new file from drop error : {e}");
            await _dialogService.ShowMessageDialogAsync(
                _visualRoot,
                "StringCommonErrorTitle".GetLocalized(),
                e.Message,
                IconType.ERROR);
        }
    }

    private async Task OnRequestPatientData(object? parameter)
    {
        if (PatientId is not null)
        {
            try
            {
                ActivePageProgressRing = true;
                ActivePageProgressRingMessage = "Requesting data";

                var token = _restApiService.GetLoginInfo().AccessToken;
                var url = string.Format(ApiRoutes.PATIENT_READ_ONE.Path, PatientId);
                var response = await _restApiService.DataRequestAsync<PatientOneDataOut>(ApiRoutes.PATIENT_READ_ONE.Method, url, ApiRoutes.PATIENT_READ_ONE.RequiresFormData, token);
                if (response.Result == APIResultType.SUCCESS)
                {
                    PatientOneData = response?.Data;

                    if (PatientOneData?.SonographyCount == 0)
                    {
                        var title = "AppDisplayName".GetLocalized();
                        var message = "StringNoRegisteredFile".GetLocalized();
                        await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.ERROR);
                        // 리스트 화면으로 화면 이동
                        //_navigationService.NavigateTo(typeof(PatientListViewModel).FullName!, null, true);
                    }
                    else
                    {
                        PatientOneData?.Sonographies?.ForEach(f =>
                        {
                            if (!string.IsNullOrWhiteSpace(f.Id) && !string.IsNullOrWhiteSpace(f.ImageUrl))
                            {
                                var info = new UltrasoundFileInfo(f.Id, f.ImageUrl, f.OriginalFileName, false, f.ConsultationSummary.QuestionCount, f.ConsultationSummary.AnswerCount, f.ConsultationSummary.HasNewAnswers);
                                UltrasoundFiles.Add(info);

                                // 분석 결과가 존재 하는 경우 분석 결과 저장
                                if (f is not null && f.Analysis is not null)
                                {
                                    _analysisResults.TryAdd(f.Analysis.SonographyId, f.Analysis);
                                }
                            }
                        });

                        // 첫번째 항목이 선택 되도록 처리
                        if (UltrasoundFiles.Count > 0)
                        {
                            SelectedUltrasoundFile = UltrasoundFiles[0];
                        }

                        CreateAnalysisResultMonitoring();
                    }
                }
                else if (response.Result == APIResultType.UNAUTHORIZED)
                {
                    throw new UnauthorizedException("StringInvalidLoginInfo".GetLocalized());
                }
                else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
                {
                    throw new MethodNotAllowedException("StringDuplicatelogin".GetLocalized());
                }
                else if (response.Result == APIResultType.NOT_ACCEPTABLE)
                {
                    throw new NotAcceptableException("StringUpdatePlan".GetLocalized());
                }
                else
                {
                    var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                    throw new Exception(message);
                }
            }
            catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
            {
                // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
                await _restApiService.LogoutAsync();
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
            }
            catch (Exception e)
            {
                // WExpertLogger.Instance.Error($"[AnalysisViewer]Request error : {e}");
                // 오류시 오류 메시지 출력 후 환자 목록 창으로 이동
                await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
                _navigationService.NavigateTo(typeof(PatientListViewModel).FullName!, null, false);
            }
            finally
            {
                ActivePageProgressRing = false;
                ActivePageProgressRingMessage = string.Empty;
            }
        }
    }

    private async Task OnThumbnailOpened(object? parameter)
    {
        if (parameter is Image image
            && image.Source is BitmapImage bitmapImage
            && image.DataContext is UltrasoundFileInfo info)
        {
            try
            {
                var height = bitmapImage.PixelHeight;
                var width = bitmapImage.PixelWidth;
                var (mimeType, fileSize) = await _restApiService.GetImageInfoFromUrlAsync(info.ImageUrl);
                info.ImageInfo = new(mimeType, width, height, fileSize);
            }
            catch (Exception e)
            {
                WExpertLogger.Instance.Error(e.ToString());
            }
            finally
            {
                info.ThumbnailLoading = false;
            }
        }
    }

    private void OnThumbnailOpenFailed(object? parameter)
    {
        if (parameter is Image image)
        {
            var info = (UltrasoundFileInfo)image.DataContext;
            info.ThumbnailLoading = false;

            // 대체 이미지 소스 설정 (BitmapImage 사용)
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/NoImageMedium.png"));
        }
    }

    private async Task OnExport()
    {
        var (pixels, width, height) = await usViewer.GetHeatmapImageAsync();
        if (pixels == null || width == 0 || height == 0)
        {
            return;
        }

        var fileName = Path.GetFileNameWithoutExtension(SelectedUltrasoundFile?.FileName);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        // 저장할 파일을 선택
        var savePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SuggestedFileName = fileName
        };
        savePicker.FileTypeChoices.Add("JPEG files", [".jpg"]);
        //savePicker.FileTypeChoices.Add("PNG files", [".png"]);

        var hwnd = App.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(savePicker, hwnd);
        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(file?.FileType.ToLower() == ".png" ?
                BitmapEncoder.PngEncoderId : BitmapEncoder.JpegEncoderId, stream);

            var dpi = 96.0 * usViewer.XamlRoot.RasterizationScale;  // 실제 DPI 계산

            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)width,
                (uint)height,
                dpi, // 96.0  // DpiX
                dpi, // 96.0  // DpiY
                pixels);

            await encoder.FlushAsync();
        }
    }

    private void OnViewAdminNote()
    {
        if (AdminNoteFlyoutIsOpen)
        {
            // 다른 열려 있는 Flyout 닫기
            AdminNoteFlyoutIsOpen = false;
        }

        AdminNoteFlyoutIsOpen = true;
    }

    private void OnMovePreviousFile()
    {
        // 선택된 파일이 없거나 현재 선택된 파일이 목록에 없는 경우 첫 번째 파일 선택
        if (SelectedUltrasoundFile == null || !UltrasoundFiles.Contains(SelectedUltrasoundFile))
        {
            if (UltrasoundFiles.Count > 0)
            {
                SelectedUltrasoundFile = UltrasoundFiles[0];
            }
            return;
        }

        // 현재 선택된 파일의 인덱스 가져오기
        var index = UltrasoundFiles.IndexOf(SelectedUltrasoundFile);

        // 이전 파일 선택
        if (index > 0)
        {
            SelectedUltrasoundFile = UltrasoundFiles[index - 1];
        }
    }

    private void OnMoveNextFile()
    {
        // 선택된 파일이 없거나 현재 선택된 파일이 목록에 없는 경우 첫 번째 파일 선택
        if (SelectedUltrasoundFile == null || !UltrasoundFiles.Contains(SelectedUltrasoundFile))
        {
            if (UltrasoundFiles.Count > 0)
            {
                SelectedUltrasoundFile = UltrasoundFiles[0];
            }
            return;
        }

        // 현재 선택된 파일의 인덱스 가져오기
        var index = UltrasoundFiles.IndexOf(SelectedUltrasoundFile);

        // 다음 파일 선택
        if (index < UltrasoundFiles.Count - 1)
        {
            SelectedUltrasoundFile = UltrasoundFiles[index + 1];
        }
    }

    private void OnMultiSelectGridSelectionChanged(object? parameter)
    {
        if (parameter is IList<object> selectedItems)
        {
            // 선택된 항목을 HashSet으로 변환
            var selectedSet = new HashSet<UltrasoundFileInfo>(selectedItems.OfType<UltrasoundFileInfo>());

            // Check 상태 변경
            foreach (var item in UltrasoundFiles)
            {
                // 상태가 변경된 경우에만 업데이트
                var shouldCheck = selectedSet.Contains(item);
                if (item.Check != shouldCheck)
                {
                    item.Check = shouldCheck;
                }
            }

            MultiSelectionStatusText = selectedItems.Count == 0 ?
                string.Empty : string.Format("StringMultiSelectionStatusFormat".GetLocalized(), selectedItems.Count);

            // Check all 상태 변경 확인
            CheckAllMultiSelection = UltrasoundFiles.Count == selectedItems.Count;

            // OK 버튼 활성화 여부 체크
            EnableMultiSelectionTaskOKButton = selectedItems.Count > 0 && !MultiSelectionTaskProcessing;
        }
    }

    private async void OnNewConsultation()
    {
        try
        {
            var createConsultationOut = await _dialogService.ShowNewConsultationDialogAsync(_visualRoot, SelectedUltrasoundFile?.UltraSoundFileId, ConsultationQuota, ConsultationUsed);
            if (createConsultationOut != null && createConsultationOut is CreateConsultationOut)
            {
                OnRefreshConsultationList();
            }
        }
        catch (UnauthorizedException ue)
        {
            // 오류시 오류 메시지 출력
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringUnauthorizedTitle".GetLocalized(), ue.Message, IconType.ERROR);
            // 로그인 화면으로 화면 이동
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, null, true);
        }
        catch (Exception e)
        {
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }
        finally
        {
            
        }
    }

    private async void OnRefreshConsultationList()
    {
        try
        {
            ActiveConsultationProgressRing = true;
            await Task.Delay(500);

            if (SelectedUltrasoundFile?.UltraSoundFileId == null)
            {
                return;
            }

            var token = _restApiService.GetLoginInfo().AccessToken;
            var url = string.Format(ApiRoutes.CONSULTATION_READ_ALL.Path, SelectedUltrasoundFile.UltraSoundFileId);
            var response = await _restApiService.DataRequestAsync<ConsultationListDataOut>(ApiRoutes.CONSULTATION_READ_ALL.Method, url,
                                                                                  ApiRoutes.CONSULTATION_READ_ALL.RequiresFormData, token);

            if (response.Result == APIResultType.SUCCESS)
            {
                var analysisStatusOut = response.Data;

                // 기존 컨설팅 목록 삭제
                ConsultationListItems.Clear();
                ConsultationQuota = 0;
                ConsultationUsed = 0;

                if (analysisStatusOut != null)
                {
                    ConsultationQuota = analysisStatusOut.ConsultationQuota;
                    ConsultationUsed = analysisStatusOut.ConsultationUsed;

                    foreach (var item in analysisStatusOut.Consultations)
                    {
                        var addItem = new ConsultationListItem()
                        {
                            Id = item.Id,
                            CreatedAt = item.CreatedAt,
                            Question = item.Question
                        };
                        // Check Answer exist
                        if (item.Answer is not null)
                        {
                            addItem.AnswerExist = true;
                            addItem.AnswerId = item.Answer.Id;
                            addItem.AnswerCreatedAt = item.Answer.CreatedAt;
                            addItem.Answer = item.Answer.Answer;
                            addItem.AnswerAttachmentUrl = (item.Answer?.AttachmentUrl) ?? string.Empty;
                        }

                        ConsultationListItems.Add(addItem);
                    }
                }
            }
            else if (response.Result == APIResultType.UNAUTHORIZED)
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringUnauthorizedMessage".GetLocalized(), response.ResultCode);
                throw new UnauthorizedException(message);
            }
            else if (response.Result == APIResultType.METHOD_NOT_ALLOWED)
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringDuplicatelogin".GetLocalized(), response.ResultCode);
                throw new MethodNotAllowedException(message);
            }
            else if (response.Result == APIResultType.NOT_ACCEPTABLE)
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringUpdatePlan".GetLocalized(), response.ResultCode);
                throw new NotAcceptableException(message);
            }
            else
            {
                var message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new Exception(message);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[AnalysisViewer]Get analysis result error : {e}");
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }
        finally
        {
            ActiveConsultationProgressRing = false;
        }
    }
}
