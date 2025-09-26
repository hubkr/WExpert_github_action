using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class VerifyOtpOut
{
    [JsonProperty(PropertyName = "result")]
    public bool Result
    {
        get; set;
    }

    [JsonProperty(PropertyName = "csrfToken")]
    public string? CsrfToken
    {
        get; set;
    }
}
