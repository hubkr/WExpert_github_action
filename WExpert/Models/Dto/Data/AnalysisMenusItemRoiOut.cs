using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class AnalysisMenusItemRoiOut
{
    [JsonProperty(PropertyName = "enabled")]
    public bool? Enable
    {
        get; set;
    }

    [JsonProperty(PropertyName = "borderThickness")]
    public int BorderThickness
    {
        get; set;
    }

    [JsonProperty(PropertyName = "borderColor")]
    public string? BorderColor
    {
        get; set;
    }

    [JsonProperty(PropertyName = "fillColor")]
    public string? FillColor
    {
        get; set;
    }
}
