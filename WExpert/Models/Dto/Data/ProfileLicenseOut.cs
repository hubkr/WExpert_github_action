
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ProfileLicenseOut
{
    [JsonProperty(PropertyName = "licenseKey")]
    public string? LicenseKey
    {
        get; set;
    }

    [JsonProperty(PropertyName = "validFrom")]
    public DateTime? ValidFrom
    {
        get; set;
    }

    [JsonProperty(PropertyName = "expiresAt")]
    public DateTime? ExpiresAt
    {
        get; set;
    }

    [JsonProperty(PropertyName = "serverNow")]
    public DateTime? ServerNow
    {
        get; set;
    }    

    [JsonProperty(PropertyName = "algorithmPlanName")]
    public AlgorithmPlanType? AlgorithmPlanName
    {
        get; set;
    }

    [JsonProperty(PropertyName = "consultationPlanName")]
    public ConsultationPlanType? ConsultationPlanName
    {
        get; set;
    }
}
