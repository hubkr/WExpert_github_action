using Newtonsoft.Json;

namespace WExpert.Models;

// access token 의 x-license 데이터를 담을 클래스
public class XLicenseOut
{
    [JsonProperty(PropertyName = "valid_from")]
    public DateTime? ValidFrom
    {
        get; set;
    }

    [JsonProperty(PropertyName = "expires_at")]
    public DateTime? ExpiresAt
    {
        get; set;
    }

    [JsonProperty(PropertyName = "model_plan")]
    public AlgorithmPlanType? ModelPlan
    {
        get; set;
    }

    [JsonProperty(PropertyName = "consultation_plan")]
    public ConsultationPlanType? ConsultationPlan
    {
        get; set;
    }
}
