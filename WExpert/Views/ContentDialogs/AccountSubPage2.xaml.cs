using Microsoft.UI.Xaml.Navigation;
using WExpert.Models.Dto.Data;
using WExpert.ViewModels.ContentDialogs;
using WExpert.Views.Base;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WExpert.Views.ContentDialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AccountSubPage2 : WEXBasePage
{
    public AccountSubPage2ViewModel ViewModel
    {
        get;
    }

    public AccountSubPage2()
    {
        InitializeComponent();

        ViewModel = App.GetService<AccountSubPage2ViewModel>();
        DataContext = ViewModel;
    }

    public void UpdateHospitalInfo(ProfileHospitalOut? hospital)
    {
        if (hospital != null)
        {
            ViewModel.Hospital = hospital.Name;
            ViewModel.Country  = hospital.Country;
            ViewModel.Address  = hospital.Address;
            ViewModel.Contact  = hospital.PhoneNumber;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        UpdateHospitalInfo(e.Parameter as ProfileHospitalOut);
    }
}
