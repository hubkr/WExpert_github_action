using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class CheckVersionOut
{
    [JsonProperty(PropertyName = "hasNewVersion")]
    public required bool HasNewVersion
    {
        get; set;
    } = false;

    [JsonProperty(PropertyName = "latestVersion")]
    public required string LatestVersion
    {
        get; set;
    }

    [JsonProperty(PropertyName = "isForceUpdate")]
    public required bool IsForceUpdate
    {
        get; set;
    } = false;

    [JsonProperty(PropertyName = "downloadUrl")]
    public required string DownloadUrl
    {
        get; set;
    }
}

