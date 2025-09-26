using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ProfileAccountOut
{
    [JsonProperty(PropertyName = "loginId")]
    public string? LoginId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "name")]
    public string? Name
    {
        get; set;
    }

    [JsonProperty(PropertyName = "email")]
    public string? Email
    {
        get; set;
    }
}
