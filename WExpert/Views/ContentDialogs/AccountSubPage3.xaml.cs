using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WExpert.Helpers;
using WExpert.Models.Dto.Data;
using WExpert.ViewModels.ContentDialogs;
using WExpert.Views.Base;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WExpert.Views.ContentDialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AccountSubPage3 : WEXBasePage
{

    public AccountSubPage3ViewModel ViewModel
    {
        get;
    }

    public AccountSubPage3()
    {
        InitializeComponent();

        ViewModel = App.GetService<AccountSubPage3ViewModel>();
        DataContext = ViewModel;
    }

    public void UpdateLicenseInfo(ProfileLicenseOut? license)
    {
        if (license != null)
        {
            ViewModel.ProfileLicense = license;
            ViewModel.AlgorithmPlan = license?.AlgorithmPlanName?.GetDescription();
            ViewModel.ConsultationPlan = license?.ConsultationPlanName?.GetDescription();
            ViewModel.LicenseKey = license?.LicenseKey;
            ViewModel.ValidityPeriod = $"{license?.ValidFrom:yyyy.MM.dd} ~ {license?.ExpiresAt:yyyy.MM.dd}";
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        UpdateLicenseInfo(e.Parameter as ProfileLicenseOut);
    }
}
