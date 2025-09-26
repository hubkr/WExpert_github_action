
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ReceiveOtpMailOut2
{
    [JsonProperty(PropertyName = "requestQuota")]
    public required int RequestQuota
    {
        get; set;
    }

    [JsonProperty(PropertyName = "rateLimitReset")]
    public required int RateLimitReset
    {
        get; set;
    }

    [JsonProperty(PropertyName = "remaining")]
    public required int Remaining
    {
        get; set;
    }
}
