using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using WExpert.Code;
using WExpert.Models;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.Views.Base;

namespace WExpert.Views;

/// <summary>
/// 환자 목록 페이지
/// </summary>
public sealed partial class PatientListPage : WEXBasePage
{
    private readonly bool IsInitialized = false;

    /*
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadCursorFromFile(string lpFileName);

    [DllImport("user32.dll")]
    static extern bool SetCursor(IntPtr hCursor);

    [DllImport("user32.dll")]
    static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private const int GCLP_HCURSOR = -12;

    private IntPtr customCursorHandle; // 커서 핸들을 저장할 변수

    private void LoadCustomCursor()
    {
        try
        {
            // 앱 패키지 내의 커서 파일 경로를 가져옵니다.
            //var cursorPath = Path.Combine("Assets", "Cursor", "Grap.cur");
            var basePath = AppContext.BaseDirectory;
            var cursorPath = Path.Combine(basePath, "Assets", "Cursor", "Grap.cur");


            // 파일에서 커서를 로드합니다.
            customCursorHandle = LoadCursorFromFile(cursorPath);

            if (customCursorHandle == IntPtr.Zero)
            {
                // 오류 처리
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // 창의 기본 커서를 변경합니다.
            //IntPtr hwnd = WindowNative.GetWindowHandle(this);
            //var hwnd = App.MainWindow.GetWindowHandle();
            //SetClassLongPtr(hwnd, GCLP_HCURSOR, customCursorHandle);
            SetCursor(customCursorHandle);

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"커서 로드/설정 실패: {ex.Message}");
        }
    }
    */

    public PatientListViewModel ViewModel
    {
        get;
    }

    public PatientListPage()
    {
        InitializeComponent();

        IsInitialized = false;
        ViewModel = App.GetService<PatientListViewModel>();
        DataContext = ViewModel;

        // Row per Page
        List<string> RowPerPageUnit = ["10", "15", "20", "30", "40", "50", "100"];
        RowPerPageDropDownList.ItemsSource = RowPerPageUnit;
        RowPerPageDropDownList.SelectedIndex = 2;

        IsInitialized = true;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {  
        // 페이지 시작시 특정 컨트롤에 포커스가 가있는
        // 현상을 방지하기 위해 페이지를 disable 후 enable 처리
        IsEnabled = false;
        IsEnabled = true;

        // New Consultation Answer Animation 작동
        var storyboard = (Storyboard) NewConsultationAnswerDot.Resources["BlinkingAnimation"];
        storyboard.Begin();

        // ViewModel 속성 변경 감지
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        //LoadCustomCursor();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        /*
        if (e.PropertyName == nameof(ViewModel.IsNotificationPopupOpen))
        {
        }
        */
    }

    private bool IsAscSortingMode()
    {
        return (ViewModel.CurrentOrderingType == OrderType.CREATED_AT_ASC ||
            ViewModel.CurrentOrderingType == OrderType.NAME_ASC ||
            ViewModel.CurrentOrderingType == OrderType.STATUS_ASC ||
            ViewModel.CurrentOrderingType == OrderType.TRIAGE_ASC);
    }

    private void RowPerPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var list = sender as ListView;
        if (list == null)
        {
            return;
        }

        if (list.SelectedItem != null && list.SelectedItem is string page)
        {
            // 초기화가 안된 경우 화면만 Update
            if (!IsInitialized)
            {
                ViewModel.SelectedRowPerPage = int.Parse(page);
                return;
            }

            // 새로 고침 전 첫 번째 항목의 인덱스를 계산
            var oFirstIndex = ((Math.Max(ViewModel.CurrentPage, 1) - 1) * ViewModel.SelectedRowPerPage) + 1;

            // 새로 고침 후 oFirstIndex가 포함된 페이지를 계산
            var newPageSize = int.Parse(page);
            var targetPage = (oFirstIndex - 1) / newPageSize + 1;

            ViewModel.SelectedRowPerPage = newPageSize;

            // 선택 후 드롭다운 닫기
            RowPerPageDropDown.Flyout.Hide();

            // 타겟 페이지로 새로 고침 실행
            ViewModel.SelectedRowPerPageCommand.Execute(targetPage);
        }
    }

    private void PatientListHeader_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag.Equals("Triage"))
            {
                ViewModel.SortingCommand.Execute(ViewModel.CurrentOrderingType == OrderType.TRIAGE_DESC ?
                    OrderType.TRIAGE_ASC : OrderType.TRIAGE_DESC);
            }
            else if (button.Tag.Equals("Name"))
            {
                ViewModel.SortingCommand.Execute(ViewModel.CurrentOrderingType == OrderType.NAME_DESC ?
                    OrderType.NAME_ASC : OrderType.NAME_DESC);
            }
            else if (button.Tag.Equals("DateCreated"))
            {
                ViewModel.SortingCommand.Execute(ViewModel.CurrentOrderingType == OrderType.CREATED_AT_DESC ?
                    OrderType.CREATED_AT_ASC : OrderType.CREATED_AT_DESC);
            }
            else if (button.Tag.Equals("Analysis"))
            {
                ViewModel.SortingCommand.Execute(ViewModel.CurrentOrderingType == OrderType.STATUS_DESC ?
                    OrderType.STATUS_ASC : OrderType.STATUS_DESC);
            }
        }
    }

    private void SearchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.RefreshCommand.Execute(null);           
        }
    }

    private void TriageFilter_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        var toggleButton = sender as ToggleButton;
        var triageMapping = new Dictionary<string, TriageFilterType>
        {
            { "TriageFilterNormal", TriageFilterType.NORMAL },
            { "TriageFilterRupture", TriageFilterType.RUPTURE },
            { "TriageFilterTC", TriageFilterType.THICKENED_CAPSULE }
        };

        if (toggleButton?.Name != null && triageMapping.TryGetValue(toggleButton.Name, out var type))
        {
            var isChecked = toggleButton.IsChecked == true;
            var containsType = ViewModel.TriageFilter.Contains(type);

            if (isChecked && !containsType)
            {
                ViewModel.TriageFilter.Add(type);
            }
            else if (!isChecked && containsType)
            {
                ViewModel.TriageFilter.Remove(type);
            }
            else
            {
                return; // 상태 변경이 없으면 조기 종료
            }

            // 상태가 변경된 경우만 호출
            ViewModel.TriageFilterCommand.Execute(null);
        }
    }

    private void TypeFilter_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        var toggleButton = sender as ToggleButton;

        var patientTypeMapping = new Dictionary<string, PatientType>
        {
            { "TypeFilterAesthetic", PatientType.AESTHETIC },
            { "TypeFilterReconstructive", PatientType.RECONSTRUCTIVE },
            { "TypeFilterBoth", PatientType.BOTH }
        };

        if (toggleButton?.Name != null && patientTypeMapping.TryGetValue(toggleButton.Name, out var type))
        {
            var isChecked = toggleButton.IsChecked == true;
            var containsType = ViewModel.PatientTypeFilter.Contains(type);

            if (isChecked && !containsType)
            {
                ViewModel.PatientTypeFilter.Add(type);
            }
            else if (!isChecked && containsType)
            {
                ViewModel.PatientTypeFilter.Remove(type);
            }
            else
            {
                return; // 상태 변경이 없으면 조기 종료
            }

            // 상태가 변경된 경우만 호출
            ViewModel.PatientTypeFilterCommand.Execute(null);
        }
    }

    private void NewAnswerFilter_CheckedUnchecked(object sender, RoutedEventArgs e)
    {
        var toggleButton = sender as ToggleButton;
        if (toggleButton?.IsChecked == true)
        {
            NewConsultationAnswerDot.Opacity = 0;
        }
        else
        {
            NewConsultationAnswerDot.Opacity = ViewModel.TotalNewAnswerCount > 0 ? 1 : 0;
        }

        ViewModel.NewConsultationAnswerFilter = toggleButton?.IsChecked == true ? NewConsultationAnswerFilterType.HAS_NEW_ANSWER : NewConsultationAnswerFilterType.ALL;
        ViewModel.NewConsultationAnswerFilterCommand.Execute(null);
    }

    private void ListItemCheckBox_Tapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;

        if (sender is CheckBox checkBox)
        {
            if (checkBox.DataContext is PatientListItem item)
            {
                if (checkBox.IsChecked == true)
                {
                    PatientListView.SelectedItems.Add(item);
                }
                else
                {
                    PatientListView.SelectedItems.Remove(item);
                }                
            }
        }
    }

    private void AccountMenu_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AccountCommand.Execute(null);
        // 선택 후 드롭다운 닫기
        AccountDropDown.Flyout.Hide();
    }

    private void AboutMenu_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AboutWExpertCommand.Execute(null);
        // 선택 후 드롭다운 닫기
        AccountDropDown.Flyout.Hide();
    }

    private void LogoutMenu_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.LogoutCommand.Execute(null);
        // 선택 후 드롭다운 닫기
        AccountDropDown.Flyout.Hide();
    }

    private void PatientListCheckBox_Click(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        if (checkBox == null)
        {
            return;
        }

        if (checkBox.IsChecked == true)
        {
            PatientListView.SelectAll();
        }
        else
        {
            PatientListView.SelectedItems.Clear();
        }
    }

    private void SortingMenuField_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is ToggleButton dirction)
            {
                if (dirction.IsChecked == false)
                {
                    // 기존에 체크된 상태로 되돌림 (선택 해제를 방지)
                    dirction.IsChecked = true;
                    return;
                }

                if (dirction.Tag.Equals("DateCreated"))
                {
                    ViewModel.SortingCommand.Execute(IsAscSortingMode() ? OrderType.CREATED_AT_ASC : OrderType.CREATED_AT_DESC );
                }
                else if (dirction.Tag.Equals("Name"))
                {
                    ViewModel.SortingCommand.Execute(IsAscSortingMode() ? OrderType.NAME_ASC : OrderType.NAME_DESC);
                }
                else if (dirction.Tag.Equals("Analysis"))
                {
                    ViewModel.SortingCommand.Execute(IsAscSortingMode() ? OrderType.STATUS_ASC : OrderType.STATUS_DESC);
                }
                else if (dirction.Tag.Equals("Triage"))
                {
                    ViewModel.SortingCommand.Execute(IsAscSortingMode() ? OrderType.TRIAGE_ASC : OrderType.TRIAGE_DESC);
                }
            }
        }
        finally
        {
            SortingDropDown?.Flyout?.Hide(); //드롭다운 닫기
        }
    }

    private void SortingMenuDirection_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is ToggleButton dirction)
            {
                if (dirction.IsChecked == false)
                {
                    // 기존에 체크된 상태로 되돌림 (선택 해제를 방지)
                    dirction.IsChecked = true;
                    return;
                }

                if (dirction.Tag.Equals("Asc"))
                {
                    if (ViewModel.CurrentOrderingType == OrderType.CREATED_AT_DESC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.CREATED_AT_ASC);
                    }
                    else if (ViewModel.CurrentOrderingType == OrderType.NAME_DESC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.NAME_ASC);
                    }
                    else if (ViewModel.CurrentOrderingType == OrderType.TRIAGE_DESC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.TRIAGE_ASC);
                    }
                    else if (ViewModel.CurrentOrderingType == OrderType.STATUS_DESC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.STATUS_ASC);
                    }
                }
                else if (dirction.Tag.Equals("Desc"))
                {
                    if (ViewModel.CurrentOrderingType == OrderType.CREATED_AT_ASC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.CREATED_AT_DESC);
                    }
                    else if (ViewModel.CurrentOrderingType == OrderType.NAME_ASC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.NAME_DESC);
                    }
                    else if (ViewModel.CurrentOrderingType == OrderType.TRIAGE_ASC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.TRIAGE_DESC);
                    }
                    else if (ViewModel.CurrentOrderingType == OrderType.STATUS_ASC)
                    {
                        ViewModel.SortingCommand.Execute(OrderType.STATUS_DESC);
                    }
                }
            }
        }
        finally
        {
            SortingDropDown?.Flyout?.Hide(); //드롭다운 닫기
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

    private void AdminNoteFlyout_Opended(object sender, object e)
    {
        var flyout = sender as Flyout;
        if (flyout != null)
        {
            if (flyout.Target is Button parentButton)
            {
                // 오픈시 버튼 컬러 변경
                if (Application.Current.Resources.TryGetValue("BrushComponentsLtBrand", out var resource) && resource is SolidColorBrush brush)
                {
                    parentButton.Background = brush;
                }
            }
        }
    }

    private void AdminNoteFlyout_Closed(object sender, object e)
    {
        var flyout = sender as Flyout;
        if (flyout != null)
        {
            if (flyout.Target is Button parentButton)
            {
                // 투명 처리
                parentButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }
    }
}