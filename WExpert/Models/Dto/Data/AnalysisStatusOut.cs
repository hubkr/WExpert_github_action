using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class AnalysisStatusOut
{
    [JsonProperty(PropertyName = "total")]
    public int Total
    {
        get; set;
    }

    [JsonProperty(PropertyName = "inProgress")]
    public int Analyzing
    {
        get; set;
    }

    [JsonProperty(PropertyName = "success")]
    public int Completed
    {
        get; set;
    }

    [JsonProperty(PropertyName = "failure")]
    public int Incomplete
    {
        get; set;
    }

    [JsonProperty(PropertyName = "analysisSummaryDtoList")]
    public List<AnalysisSummaryOut>? AnalysisSummaryDtoList
    {
        get; set;
    }
}
