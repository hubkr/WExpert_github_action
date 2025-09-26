using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WExpert.Models.Dto.Data;

public class AnalysisLabelsOut
{
    [JsonProperty(PropertyName = "result_type")]
    public string? Result_Type
    {
        get; set;
    }

    [JsonProperty(PropertyName = "result_class")]
    public string? Result_Class
    {
        get; set;
    }

    [JsonProperty(PropertyName = "points")]
    public JArray? Points
    {
        get; set;
    }
}