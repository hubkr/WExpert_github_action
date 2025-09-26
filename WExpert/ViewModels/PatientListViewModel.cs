using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Contracts.ViewModels;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using Windows.ApplicationModel.DataTransfer;

namespace WExpert.ViewModels;

public partial class PatientListViewModel : ObservableRecipient, INavigationAware
{
    private readonly FrameworkElement _visualRoot;
    private readonly INavigationService _navigationService;
    private readonly IRestApiService _restApiService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<PatientListItem> PatientListSource { get; } = [];

    public ICommand AnalysisViewerCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand AboutWExpertCommand { get; }
    public ICommand AccountCommand { get; }
    public ICommand DeletePatientsCommand { get; }
    public ICommand NewRegistrationCommand { get; }
    public IAsyncRelayCommand SelectedRowPerPageCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ListViewItemDoubleTabCommand { get;}
    public ICommand PasteKeyDownCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand TriageFilterCommand { get; }
    public ICommand PatientTypeFilterCommand { get; }
    public ICommand NewConsultationAnswerFilterCommand { get; }
    public IAsyncRelayCommand SortingCommand { get; }
    public ICommand PatientListSelectionChangedCommand { get; }

    [ObservableProperty]
    public bool checkAll = false;

    [ObservableProperty]
    public int patientListSelectedIndex = -1;

    [ObservableProperty]
    private bool patientListProgressRing = false;

    [ObservableProperty]
    private Visibility noResult = Visibility.Collapsed;

    [ObservableProperty]
    private string? loginId = null;

    [ObservableProperty]
    private string? userName = null;    

    [ObservableProperty]
    private string? hospitalName = null;

    [ObservableProperty]
    private bool enablePreviousPageBtn;

    [ObservableProperty]
    private bool enableNextPageBtn = false;

    [ObservableProperty]
    private bool enableDeleteBtn = false;

    [ObservableProperty]
    private List<TriageFilterType> triageFilter = [];

    [ObservableProperty]
    private List<PatientType> patientTypeFilter = [];

    [ObservableProperty]
    private bool isTriageFilterRuptureCheck = false;

    [ObservableProperty]
    private bool isTriageFilterTCCheck = false;

    [ObservableProperty]
    private bool isTriageFilterNormalCheck = false;

    [ObservableProperty]
    private bool isTypeFilterBothCheck = false;

    [ObservableProperty]
    private bool isTypeFilterReconstructiveCheck = false;

    [ObservableProperty]
    private bool isTypeFilterAestheticCheck = false;

    [ObservableProperty]
    private bool isNewAnswerFilterCheck = false;

    [ObservableProperty]
    private NewConsultationAnswerFilterType newConsultationAnswerFilter = NewConsultationAnswerFilterType.ALL;

    [ObservableProperty]
    private int totalNewAnswerCount = 0;

    [ObservableProperty]
    private int selectedRowPerPage = 0;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private OrderType currentOrderingType = OrderType.CREATED_AT_DESC;

    [ObservableProperty]
    private string currentPageStatus = string.Format("StringCurrentPageStatus".GetLocalized(), 1, 1, 0);

    [ObservableProperty]
    private string searchKeyword = string.Empty;

    [ObservableProperty]
    private string searchTextBox = string.Empty;

    private int _totalCount = -1; // no result 체크를 위해 초기값 -1 로 셋팅
    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (_totalCount != value)
            {
                SetProperty(ref _totalCount, value);
                NoResult = _totalCount > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }

    #region Patient List Monitoring Task
    private bool _patientListMonitoringTaskRunning = false;
    private ManualResetEventSlim? _patientListMonitoringPauseEvent;
    private CancellationTokenSource? _patientListMonitoringCancellationTokenSource;

    public void CreatePatientListMonitoring()
    {
        if (!_patientListMonitoringTaskRunning)
        {
            _patientListMonitoringTaskRunning = true;
            _patientListMonitoringCancellationTokenSource = new CancellationTokenSource();
            _patientListMonitoringPauseEvent = new ManualResetEventSlim(true);
            Task.Run(() => PatientListStatusMonitoringAsync(_patientListMonitoringCancellationTokenSource.Token));
        }
    }

    public void PausePatientListMonitoring(bool pause)
    {
        if (pause)
        {
            WExpertLogger.Instance.Debug("[PatientList] Monitoring analysis status(pause)");
            _patientListMonitoringPauseEvent?.Reset(); // pause
        }
        else
        {
            WExpertLogger.Instance.Debug("[PatientList] Monitoring analysis status(resume)");
            _patientListMonitoringPauseEvent?.Set();  // resume
        }
    }

    public void StopPatientListMonitoring()
    {
        _patientListMonitoringCancellationTokenSource?.Cancel();
        _patientListMonitoringTaskRunning = false;
    }

    // 분석 요청 안된 파일 존재하는 경우 분석요청 주기적인 체크
    private async Task PatientListStatusMonitoringAsync(CancellationToken token)
    {
        var dispatcherQueue = App.MainWindow.DispatcherQueue;
        var url = ApiRoutes.ANALYSIS_STATUS_ARRAY.Path;
        var accessToken = _restApiService.GetLoginInfo().AccessToken;

        while (!token.IsCancellationRequested)
        {
            try
            {
                WExpertLogger.Instance.Debug($"[PatientList] Monitoring analysis status(run)({DateTime.Now:HH:mm:ss})");
                _patientListMonitoringPauseEvent?.Wait(token); // Wait if paused

                var patientIds = PatientListSource.Where(p => p.AnalysisStatus == AnalysisStatusType.ANALYZING).Select(p => p.Id).ToList();
                var content = new AnalysisStatusListIn() { PatientIds = patientIds };

                // 분석 진행 중인 환자 목록이 존재 하는 경우 상태 체크
                if (patientIds.Count > 0)
                {
                    var response = await _restApiService.DataRequestAsync<List<AnalysisStatusListOut>>(ApiRoutes.ANALYSIS_STATUS_ARRAY.Method, url,
                                                                        ApiRoutes.ANALYSIS_STATUS_ARRAY.RequiresFormData, accessToken, content);
                    if (response.Result == APIResultType.SUCCESS)
                    {
                        var analysisStatusList = response.Data;
                        if (analysisStatusList != null)
                        {
                            dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() =>
                            {
                                analysisStatusList?.ForEach(item =>
                                {
                                    var patientListSource = PatientListSource.FirstOrDefault(p => p.Id == item.PatientId);
                                    if (patientListSource != null)
                                    {
                                        patientListSource.RuptureTriage  = item.RuptureTriage;
                                        patientListSource.TCTriage       = item.TCTriage;
                                        patientListSource.AnalysisStatus = item.AnalysisStatus;
                                    }
                                });
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
            }
            catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
            {
                dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(async () =>
                {
                    // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
                    await _restApiService.LogoutAsync();
                    _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
                }));
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested) // token 취소로 인한 exception은 message 처리 안함
                {
                    dispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() =>
                    {
                        // 오류시 오류 메시지 출력
                        WExpertLogger.Instance.Error($"[PatientList] Monitoring analysis status error : {e}");

                        // Background 로 구동 중이므로 오류시 오류 메시지는 출력 하지않고 오류 로그만 저장
                        //await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
                    }));
                }
            }

            await Task.Delay(3000, token); // 3초에 한번씩 Check
        }
    }
    #endregion Patient List Monitoring Task

    public PatientListViewModel(INavigationService navigationService, IRestApiService restApiService, INotificationService notificationService, IDialogService dialogService)
    {
        _visualRoot             = (FrameworkElement)App.MainWindow.Content;
        _navigationService      = navigationService;
        _restApiService         = restApiService;
        _notificationService    = notificationService;
        _dialogService          = dialogService;

        AnalysisViewerCommand               = new RelayCommand<object>(parameter => _navigationService.NavigateTo(typeof(AnalysisViewerViewModel).FullName!, parameter));
        LogoutCommand                       = new RelayCommand(OnLogout);
        AboutWExpertCommand                 = new RelayCommand(OnAboutWExpert);
        AccountCommand                      = new RelayCommand(OnAccountAsync);
        DeletePatientsCommand               = new RelayCommand(OnDeletePatientsAsync);
        ListViewItemDoubleTabCommand        = new RelayCommand<object>(parameter => OnListViewItemDoubleTab(parameter));
        NewRegistrationCommand              = new RelayCommand<Tuple<object, object>>(parameter => OnNewRegistrationAsync(parameter));
        SelectedRowPerPageCommand           = new AsyncRelayCommand<object>(parameter => OnSelectedRowPerPage(parameter));
        RefreshCommand                      = new AsyncRelayCommand<object>(parameter => OnRefreshPatientList(parameter));
        PasteKeyDownCommand                 = new RelayCommand(OnPasteKeyDown);
        PreviousPageCommand                 = new AsyncRelayCommand(OnPreviousPage);
        NextPageCommand                     = new AsyncRelayCommand(OnNextPage);
        TriageFilterCommand                 = new AsyncRelayCommand(OnTriageFilter);
        PatientTypeFilterCommand            = new AsyncRelayCommand(OnPatientTypeFilter);
        NewConsultationAnswerFilterCommand  = new AsyncRelayCommand(OnNewConsultationAnswerFilter);
        SortingCommand                      = new AsyncRelayCommand<object>(parameter => OnSorting(parameter));
        PatientListSelectionChangedCommand  = new RelayCommand<object>(parameter => OnPatientListSelectionChanged(parameter));

        var info     = _restApiService.GetLoginInfo();
        LoginId      = info.LoginId;
        UserName     = info.UserName;
        hospitalName = info.HospitalName;
        CurrentOrderingType = OrderType.CREATED_AT_DESC;

        #region Patient List Monitoring Task
        CreatePatientListMonitoring();
        PausePatientListMonitoring(true); // pause
        #endregion Patient List Monitoring Task
    }

    public async void OnNavigatedTo(NavigationMode mode, object? parameter)
    {                
        if (mode == NavigationMode.New) // 로그인창 > 로그인 진입인 경우
        {
            await OnRefreshPatientList(null);

            // 사이버 보안 요구 사항(로그인 성공시 시스템 사용 알림 호출)
            CybersecurityRequirementsCheckSystemNoti();

            // 사이버 보안 요구 사항(비밀번호 초기화(신규 가입 or 설정 초기화) 후 로그인)
            CybersecurityRequirementsCheckInitPassword();

            // 사이버 보안 요구 사항(주기적 비밀번호 변경 안내 체크)
            CybersecurityRequirementsCheckPasswordChangeInterval();
        }
        else if (mode == NavigationMode.Back) // 분석결과 화면 > 환자 목록 화면으로 돌아온 경우
        {
            await OnRefreshPatientList(false);
        }

        #region Patient List Monitoring Task
        PausePatientListMonitoring(false); // resume
        #endregion Patient List Monitoring Task
    }

    public void OnNavigatedFrom()
    {
        #region Patient List Monitoring Task
        PausePatientListMonitoring(true); // pause
        #endregion Patient List Monitoring Task
    }

    // 사이버 보안 요구 사항(로그인 성공시 시스템 사용 알림 호출)
    private async void CybersecurityRequirementsCheckSystemNoti()
    {
        var info = _restApiService.GetLoginInfo();
        if (!string.IsNullOrEmpty(info?.SystemUsageNotificationMessage))
        {
            await Task.Delay(2000); // 페이지가 로드된 후 2초 대기후 NotificationPopup 열기
            _notificationService.ShowNotification("StringNotice".GetLocalized(), info.SystemUsageNotificationMessage);
        }
    }

    // 사이버 보안 요구 사항(비밀번호 초기화(신규 가입 or 설정 초기화) 후 로그인)
    private async void CybersecurityRequirementsCheckInitPassword()
    {
        var info = _restApiService.GetLoginInfo();
        // 신규 계정 생성 후 최초 로그인 or 비밀번호 초기화 후 로그인
        if (info.ReasonForPasswordChangeNotification == ReasonForPasswordChangeNotification.INITIAL_PASSWORD
            || info.ReasonForPasswordChangeNotification == ReasonForPasswordChangeNotification.TEMPORARY_PASSWORD)
        {
            var title = "StringPasswordChangeRequired".GetLocalized();
            var message = info.ReasonForPasswordChangeNotification == ReasonForPasswordChangeNotification.INITIAL_PASSWORD ?
                "StringInitPassword".GetLocalized() : "StringTemporaryPassword".GetLocalized();

            // 1초 추가 대기후 Open 열기
            await Task.Delay(1000);

            var result = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.WARN, true,
                                                                    "StringChangeNow".GetLocalized(), "StringChangeLater".GetLocalized());
            if (result)
            {
                await _dialogService.ShowAccountDialogAsync(_visualRoot); // Show Account Dialog  
            }
        }
    }

    // 사이버 보안 요구 사항(주기적 비밀번호 변경 안내 체크)
    private async void CybersecurityRequirementsCheckPasswordChangeInterval()
    {
        var info = _restApiService.GetLoginInfo();
        // 비밀번호 변경 주기 체크
        if (info.ReasonForPasswordChangeNotification == ReasonForPasswordChangeNotification.PASSWORD_EXPIRED)
        {
            var title = "StringPasswordChangeRequired".GetLocalized();
            var message = "StringPasswordExpired".GetLocalized();

            // 1초 추가 대기후 Open 열기
            await Task.Delay(1000);

            var result = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.WARN, true,
                                                                    "StringChangeNow".GetLocalized(), "StringChangeLater".GetLocalized());
            if (result)
            {
                await _dialogService.ShowAccountDialogAsync(_visualRoot); // Show Account Dialog  
            }
        }
    }

    private (string? filterTriage, string? filterType, string? filterNewConsultationAnswer, string sort, string order) GetCurrentFilterSortOrderParam()
    {
        var filterTriage = TriageFilter.Count != 0 ? string.Join(",", TriageFilter.Select(t => t.GetDescription())) : null;
        var filterType = PatientTypeFilter.Count != 0 ? string.Join(",", PatientTypeFilter.Select(t => t.GetDescription())) : null;
        var filterNewConsultationAnswer = NewConsultationAnswerFilter.GetDescription();

        var orderMapping = new Dictionary<OrderType, (string sort, string order)>
        {
            { OrderType.TRIAGE_ASC, ("triage", "asc") },
            { OrderType.TRIAGE_DESC, ("triage", "desc") },
            { OrderType.NAME_ASC, ("name", "asc") },
            { OrderType.NAME_DESC, ("name", "desc") },
            { OrderType.CREATED_AT_ASC, ("created_at", "asc") },
            { OrderType.CREATED_AT_DESC, ("created_at", "desc") },
            { OrderType.STATUS_ASC, ("status", "asc") },
            { OrderType.STATUS_DESC, ("status", "desc") }
        };

        var (sort, order) = orderMapping.TryGetValue(CurrentOrderingType, out var value) ? value : (string.Empty, string.Empty);
        return (filterTriage, filterType, filterNewConsultationAnswer, sort, order);
    }

    private async Task<bool> GetPatientsListAsync(int page, int limit, string? query = null,
                                                  string? filterTriage = null, string? filterPatientType = null,
                                                  string? filterNewConsultationAnswer = null, string? sort = null, string? order = null, bool refreshAll = true)
    {
        try
        {
            List<PatientListItem> patientItems = [];
            PatientListProgressRing = true;

            var token = _restApiService.GetLoginInfo().AccessToken;

            // 쿼리 문자열을 동적으로 구성
            var queryString = $"{ApiRoutes.PATIENT_READ_ALL.Path}?page={page}&limit={limit}";

            if (!string.IsNullOrEmpty(query))
            {
                queryString += $"&query={Uri.EscapeDataString(query)}";
            }

            if (!string.IsNullOrEmpty(filterTriage))
            {
                queryString += $"&filter_triage={filterTriage}";
            }

            if (!string.IsNullOrEmpty(filterPatientType))
            {
                queryString += $"&filter_type={filterPatientType}";
            }

            if (!string.IsNullOrEmpty(filterNewConsultationAnswer))
            {
                queryString += $"&filter_consultation={filterNewConsultationAnswer}";
            }

            if (!string.IsNullOrEmpty(sort))
            {
                queryString += $"&sort_by={sort}";
            }

            if (!string.IsNullOrEmpty(order))
            {
                queryString += $"&order={order}";
            }

            var response = await _restApiService.DataRequestAsync<PatientListDataOut>(ApiRoutes.PATIENT_READ_ALL.Method, queryString,
                                                                                 ApiRoutes.PATIENT_READ_ALL.RequiresFormData, token);
            if (response.Result == APIResultType.SUCCESS)
            {
                var data = response?.Data;
                var items = data?.Data?.ToList();
                if (items != null)
                {
                    // Total count & page update
                    if (data?.Total is int total && data?.Limit is int pageLimit && data?.Page is int currentPage && data?.NewAnswerTotal is int newAnswerTotal)
                    {
                        TotalCount = total;
                        CurrentPage = currentPage == 0 ? 1 : currentPage;

                        var totalPage = (int)Math.Ceiling((double)total / pageLimit);
                        var startRange = ((CurrentPage - 1) * pageLimit) + 1;
                        var endRange = (startRange - 1) + items.Count;

                        CurrentPageStatus = TotalCount == 0 ? string.Empty : string.Format("StringCurrentPageStatus".GetLocalized(), startRange, endRange, TotalCount);

                        TotalNewAnswerCount = newAnswerTotal;    // 신규 답변 개수
                        EnablePreviousPageBtn = currentPage > 1; // 이전 페이지
                        EnableNextPageBtn = data.HasNext;        // 다음 페이지
                    }

                    foreach (var item in items)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        var addItem = new PatientListItem()
                        {
                            Id = item.Id,
                            WExpertId = item?.WExpertId,
                            Name = item?.Name ?? string.Empty,
                            PatientType = item?.Type ?? PatientType.NONE,
                            Files = item == null ? 0 : item.SonographyCount,
                            AdminNote = item?.AdminNote,
                            RuptureTriage = item == null ? 0 : item.RuptureTriage,
                            TCTriage = item == null ? 0 : item.TCTriage,
                            DateCreated = item?.RegisteredAt,
                            AnalysisStatus = item?.AnalysisStatus ?? AnalysisStatusType.NONE,
                            HasNewConsultAnswer = item != null && item.ConsultationSummary.HasNewAnswers,
                            ConsultQuestCount = (item?.ConsultationSummary?.QuestionCount > 0) ? item.ConsultationSummary.QuestionCount : 0,
                            ConsultAnswerCount = (item?.ConsultationSummary?.AnswerCount > 0) ? item.ConsultationSummary.AnswerCount : 0
                        };

                        patientItems.Add(addItem);
                    }

                    if (refreshAll) // 전체 갱신
                    {
                        // 기존 list 항목 clear
                        PatientListSource.Clear();
                        CheckAll = false; // check all control init

                        // list 항목 update
                        foreach (var item in patientItems)
                        {
                            PatientListSource.Add(item);
                        }
                    }
                    else // 기존 목록 update(차이점 만 찾아서 업데이트)
                    {
                        var currentItems = PatientListSource.ToList();
                        var newItems = patientItems.ToList();

                        // 기존 항목들 업데이트
                        for (var i = 0; i < Math.Min(currentItems.Count, newItems.Count); i++)
                        {
                            if (currentItems[i].Id == newItems[i].Id)
                            {
                                // 기존 항목 업데이트
                                currentItems[i].Name = newItems[i].Name;
                                currentItems[i].PatientType = newItems[i].PatientType;
                                currentItems[i].Files = newItems[i].Files;
                                currentItems[i].AdminNote = newItems[i].AdminNote;
                                currentItems[i].RuptureTriage = newItems[i].RuptureTriage;
                                currentItems[i].TCTriage = newItems[i].TCTriage;
                                currentItems[i].DateCreated = newItems[i].DateCreated;
                                currentItems[i].AnalysisStatus = newItems[i].AnalysisStatus;
                                currentItems[i].HasNewConsultAnswer = newItems[i].HasNewConsultAnswer;
                                currentItems[i].ConsultQuestCount = newItems[i].ConsultQuestCount;
                                currentItems[i].ConsultAnswerCount = newItems[i].ConsultAnswerCount;
                                // NotifyPropertyChanged(nameof(PatientListSource));
                            }
                        }

                        // 새로 추가할 항목 Add
                        for (var i = currentItems.Count; i < newItems.Count; i++)
                        {
                            PatientListSource.Add(newItems[i]);
                            CheckAll = false;
                        }

                        // 제거할 항목 Remove (뒤에서부터 제거)
                        for (var i = currentItems.Count - 1; i >= newItems.Count; i--)
                        {
                            PatientListSource.RemoveAt(i);
                        }

                        //CheckAll = false;
                    }

                }

                return true;
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
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[PatientList]Refresh error : {e}");
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }
        finally
        {
            // progress ring stop
            PatientListProgressRing = false;
        }

        return false;
    }

    private async void OnLogout()
    {
        var title = ResourceExtensions.GetLocalized("StringConfirmSignOutTitle");
        var message = ResourceExtensions.GetLocalized("StringConfirmSignOutMessage");
        // 로그 아웃 수행 여부 확인
        var confirm = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.CHECK, true);
        if (confirm)
        {
            try
            {
                await _restApiService.LogoutAsync();
            }
            finally
            {
                _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, null, true);
            }
        }
    }

    private async void OnAboutWExpert()
    {
        await _dialogService.ShowAboutDialogAsync(_visualRoot);

    }

    private async void OnAccountAsync()
    {
        await _dialogService.ShowAccountDialogAsync(_visualRoot);

        /*
        try
        {
            await _dialogService.ShowAccountDialogAsync(_visualRoot);
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
        */
    }

    private async void OnDeletePatientsAsync()
    {
        try
        {
            var title = string.Empty;
            var message = string.Empty;
            var removeList = PatientListSource.Where(p => p.Check).ToList();
            var removeIds = removeList.Select(p => p.Id).ToList();

            if (removeIds.Count == 0)
            {
                title = "StringNotPerformedTitle".GetLocalized();
                message = "StringFileNotSelectDeleteMessage".GetLocalized();
                await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.INFO, false);
                return;
            }

            title = "StringConfirmDeleteTitle".GetLocalized();
            message = "StringConfirmDeletePatientItemsMessage".GetLocalized();
            var confirm = await _dialogService.ShowMessageDialogAsync(_visualRoot, title, message, IconType.WARN, true);
            if (!confirm)
            {
                return;
            }

            PatientListProgressRing = true;

            var accessToken = _restApiService.GetLoginInfo().AccessToken;
            var postData = new DeletePatientsIn() { ids = removeIds };
            var response = await _restApiService.DataRequestAsync<object>(ApiRoutes.PATIENTS_DELET.Method, ApiRoutes.PATIENTS_DELET.Path,
                                                                           ApiRoutes.PATIENTS_DELET.RequiresFormData, accessToken, postData);
            if (response.Result == APIResultType.SUCCESS)
            {
                // 현재 리스트 목록에서 해당 item 들 삭제
                foreach (var item in removeList)
                {
                    PatientListSource.Remove(item);
                }
                CheckAll = false;

                // 다음 페이지가 없고 현재 페이지의 모든 항목을 삭제한 경우 이전 페이지로 이동
                var page = (CurrentPage > 1 && !EnableNextPageBtn && CheckAll) ? CurrentPage - 1 : CurrentPage;

                // 삭제 완료 후 페이지 새로고침
                (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
                await GetPatientsListAsync(page, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order, false);

                return;
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
            else if (response.Result == APIResultType.FORBIDDEN)
            {
                message = CommonUtils.MakeHTTPErrorMessage("StringForbiddenMessage".GetLocalized(), response.ResultCode);
                throw new ForbiddenException(message);
            }
            else
            {
                // 기타 오류 시 메시지창 출력
                message = CommonUtils.MakeHTTPErrorMessage("StringCommonErrorMessage".GetLocalized(), response.ResultCode);
                throw new Exception(message);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedException || ex is MethodNotAllowedException || ex is NotAcceptableException)
        {
            // 공통 처리 - 401(유효 하지 않은 토큰),405(중복 로그인),406(라이선스 정보 업데이트) 오류인 경우 > 로그아웃 후 로그인 창으로 이동
            await _restApiService.LogoutAsync();
            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!, ex.Message, true);
        }
        catch (ForbiddenException fe)
        {
            // 오류 메시지 출력
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringForbiddenTitle".GetLocalized(), fe.Message, IconType.ERROR);
        }
        catch (Exception e)
        {
            // WExpertLogger.Instance.Error($"[Login]Login error : {e}");
            // 오류시 오류 메시지 출력
            await _dialogService.ShowMessageDialogAsync(_visualRoot, "StringCommonErrorTitle".GetLocalized(), e.Message, IconType.ERROR);
        }
        finally
        {
            PatientListProgressRing = false;
        }
    }

    // 신규 등록(+클립보드 이미지 추가 기능 포함)
    private async void OnNewRegistrationAsync(object? parameter)
    {
        try
        {
            var filePath = parameter as string;
            var createPatientOut = await _dialogService.ShowRegistrationDialogAsync(_visualRoot, filePath);
            if (createPatientOut != null && createPatientOut is CreatePatientOut)
            {
                // 검색 조건 초기화
                CurrentOrderingType = OrderType.CREATED_AT_DESC;
                SearchTextBox = string.Empty;
                CurrentPage = 1;
                TriageFilter.Clear();
                PatientTypeFilter.Clear();
                NewConsultationAnswerFilter = NewConsultationAnswerFilterType.ALL;
                IsTriageFilterRuptureCheck = false;
                IsTriageFilterTCCheck = false;
                IsTriageFilterNormalCheck = false;
                IsTypeFilterBothCheck = false;
                IsTypeFilterReconstructiveCheck = false;
                IsTypeFilterAestheticCheck = false;
                IsNewAnswerFilterCheck = false;

                // 목록 갱신
                await OnRefreshPatientList(null);

                // 새로 추가된 항목 확인
                var newPatient = PatientListSource.FirstOrDefault(p => p.Id == createPatientOut.Id);
                if (newPatient != null)
                {
                    PatientListSelectedIndex = 0;
                    var parameters = new Dictionary<string, object>
                    {
                        { "PatientListItem", newPatient },
                        { "page", CurrentPage },
                        { "limit", SelectedRowPerPage },
                    };

                    // Analysis Viewer 화면 이동
                    AnalysisViewerCommand.Execute(parameters);
                }
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
    }

    // 페이지당 출력 item 개수 변경
    private async Task OnSelectedRowPerPage(object? parameter)
    {
        if (parameter is int page)
        {
            (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
            await GetPatientsListAsync(page, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
        }
    }  

    private async Task OnRefreshPatientList(object? parameter) // 리스트 Refresh(현재 화면 상의 설정된 상태(필터, Sort, 검색어) 조건 그대로 갱신)
    {
        await Task.Delay(500);

        var refreshAll = true;
        if (parameter is bool all)
        {
            refreshAll = all;
        }

        // 검색어 조건 확인
        SearchKeyword = SearchTextBox.Trim();
        // 필터, 정렬 조건 확인
        (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
        // 해당 조건을 가진 목록 요청
        var result = await GetPatientsListAsync(CurrentPage, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order, refreshAll);
        if (!result)
        {
            SearchKeyword = string.Empty;
        }
    }

    private async Task OnPreviousPage() // 이전 페이지
    {
        var page = CurrentPage - 1;
        (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
        await GetPatientsListAsync(page, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
    }

    private async Task OnNextPage() // 다음 페이지
    {
        var page = CurrentPage + 1;
        (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
        await GetPatientsListAsync(page, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
    }

    private async void OnPasteKeyDown()
    {
        var dataPackageView = Clipboard.GetContent();
        // 클립 보드에 이미지 데이터가 없는 경우
        if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
        {
            return;
        }

        var tmpPath = await FileUtils.MakeTmpFromClipboaard(_visualRoot, dataPackageView);
        if (!string.IsNullOrEmpty(tmpPath))
        {
            // 신규 등록 창 출력
            OnNewRegistrationAsync(tmpPath);
        }
    }

    private async void OnListViewItemDoubleTab(object? parameter)
    {
        try
        {
            if (parameter is ListView listView && listView.SelectedItem is PatientListItem patientListItem)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "PatientListItem", patientListItem },
                    //{ "page", CurrentPage },
                    //{ "limit", SelectedRowPerPage },
                };

                _navigationService.NavigateTo(typeof(AnalysisViewerViewModel).FullName!, parameters);
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
    }

    private async Task OnTriageFilter()
    {
        (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
        await GetPatientsListAsync(1, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
    }

    private async Task OnPatientTypeFilter()
    {
        (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
        await GetPatientsListAsync(1, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
    }

    private async Task OnNewConsultationAnswerFilter()
    {
        (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
        await GetPatientsListAsync(1, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
    }

    private async Task OnSorting(object? parameter)
    {
        if (parameter is OrderType orderType)
        {
            CurrentOrderingType = orderType;

            (var filterTriage, var filterPatientType, var filterNewConsultationAnswer, var sort, var order) = GetCurrentFilterSortOrderParam();
            await GetPatientsListAsync(1, SelectedRowPerPage, SearchKeyword, filterTriage, filterPatientType, filterNewConsultationAnswer, sort, order);
        }
        else
        {
            return;
        }
    }

    private void OnPatientListSelectionChanged(object? parameter)
    {
        if (parameter is IList<object> selectedItems)
        {
            // 선택된 항목을 HashSet으로 변환
            var selectedSet = new HashSet<PatientListItem>(selectedItems.OfType<PatientListItem>());

            var existCheck = false;
            // Check 상태 변경
            foreach (var item in PatientListSource)
            {
                // 상태가 변경된 경우에만 업데이트
                var shouldCheck = selectedSet.Contains(item);
                if (item.Check != shouldCheck)
                {
                    item.Check = shouldCheck;                    
                }

                if (item.Check)
                {
                    existCheck = true;
                }
            }

            EnableDeleteBtn = existCheck;

            // Check all 상태 변경 확인
            CheckAll = PatientListSource.Count == selectedItems.Count;
        }
    }
}
