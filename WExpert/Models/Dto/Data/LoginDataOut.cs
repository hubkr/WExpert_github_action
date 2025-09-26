
using System.Text.Json;
using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;

public class LoginDataOut
{
    [JsonProperty(PropertyName = "accessToken")]
    public string? AccessToken
    {
        get; set;
    }

    [JsonProperty(PropertyName = "hospitalName")]
    public string? HospitalName
    {
        get; set;
    }

    [JsonProperty(PropertyName = "userName")]
    public string? UserName
    {
        get; set;
    }

    [JsonProperty(PropertyName = "analysisMenus")]
    public List<AnalysisMenusOut>? AnalysisMenus
    {
        get; set;
    }

    [JsonProperty(PropertyName = "reasonForPasswordChangeNotification")]
    public ReasonForPasswordChangeNotification? ReasonForPasswordChangeNotification
    {
        get; set;
    }

    [JsonProperty(PropertyName = "systemUsageNotificationMessage")]
    public string? SystemUsageNotificationMessage
    {
        get; set;
    }    

    public string? LoginId
    {
        get; set;
    }

    public void CopyData(string? loginId, LoginDataOut? original)
    {
        LoginId                                 = loginId;
        AccessToken                             = original?.AccessToken;
        HospitalName                            = original?.HospitalName;
        UserName                                = original?.UserName;
        AnalysisMenus                           = original?.AnalysisMenus;
        ReasonForPasswordChangeNotification     = original?.ReasonForPasswordChangeNotification;
        SystemUsageNotificationMessage          = original?.SystemUsageNotificationMessage;
    }
}