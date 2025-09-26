using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ReceiveOtpMailIn
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
}
