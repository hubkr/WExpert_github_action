using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class UserAgentIn
{
    [JsonProperty(PropertyName = "clientVersion")]
    public required string ClientVersion
    {
        get; set;
    }

    [JsonProperty(PropertyName = "clientIdentifier")]
    public required string ClientIdentifier
    {
        get; set;
    }

    [JsonProperty(PropertyName = "deviceInfo")]
    public required string DeviceInfo
    {
        get; set;
    }
}