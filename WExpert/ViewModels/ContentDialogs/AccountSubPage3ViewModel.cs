using CommunityToolkit.Mvvm.ComponentModel;
using WExpert.Models.Dto.Data;

namespace WExpert.ViewModels.ContentDialogs;

public partial class AccountSubPage3ViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ProfileLicenseOut? profileLicense;

    [ObservableProperty]
    private string? algorithmPlan;

    [ObservableProperty]
    private string? consultationPlan;    

    [ObservableProperty]
    private string? licenseKey;

    [ObservableProperty]
    private string? validityPeriod;

    public AccountSubPage3ViewModel()
    {
    }
}

