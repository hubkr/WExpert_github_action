using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class VerifyOtpIn
{
    [JsonProperty(PropertyName = "loginId")]
    public string? LoginId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "email")]
    public string? Email
    {
        get; set;
    }

    [JsonProperty(PropertyName = "otp")]
    public string? Otp
    {
        get; set;
    }
}
