using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;

public class AnalysisMenusItemOut
{
    [JsonProperty(PropertyName = "name")]
    public required string Name
    {
        get; set;
    }

    [JsonProperty(PropertyName = "id")]
    public required WExpertAlgorithmsType Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "parentId")]
    public required WExpertAlgorithmsType? ParentId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "enabled")]
    public required bool Enable
    {
        get; set;
    }

    [JsonProperty(PropertyName = "roi")]
    public bool? roi
    {
        get; set;
    }

    [JsonProperty(PropertyName = "roiStatus")]
    public AnalysisMenusItemRoiOut? roiStatus
    {
        get; set;
    }
}
