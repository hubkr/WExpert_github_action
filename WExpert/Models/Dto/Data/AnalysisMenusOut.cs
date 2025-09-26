using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class AnalysisMenusOut
{
    [JsonProperty(PropertyName = "category")]
    public required string Category
    {
        get; set;
    }

    [JsonProperty(PropertyName = "algorithms")]
    public required List<AnalysisMenusItemOut> Items
    {
        get; set;
    }
}
