
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class WebMessageIn
{
    [JsonProperty(PropertyName = "type")]
    public required string Type
    {
        get; set;
    }

    [JsonProperty(PropertyName = "payload")]
    public object? Payload
    {
        get; set;
    }
}