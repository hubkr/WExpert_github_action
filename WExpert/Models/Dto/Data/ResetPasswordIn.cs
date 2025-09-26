using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ResetPasswordIn
{
    [JsonProperty(PropertyName = "email")]
    public string? Email
    {
        get; set;
    }

    [JsonProperty(PropertyName = "csrfToken")]
    public string? CsrfToken
    {
        get; set;
    }

    [JsonProperty(PropertyName = "password")]
    public string? Password
    {
        get; set;
    }
}
