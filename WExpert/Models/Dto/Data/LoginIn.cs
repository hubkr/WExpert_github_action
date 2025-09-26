
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class LoginIn
{
    [JsonProperty(PropertyName = "loginId")]
    public required string LoginId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "password")]
    public required string Password
    {
        get; set;
    }

    [JsonProperty(PropertyName = "forceLogin")]
    public bool ForceLogin
    {
        get; set;
    }

    [JsonProperty(PropertyName = "userAgent")]
    public UserAgentIn? UserAgent
    {
        get;
        set;
    }
}
