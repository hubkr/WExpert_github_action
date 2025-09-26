using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;


public class AnalysisReportIn
{
    [JsonProperty(PropertyName = "accessToken")]
    public required string AccessToken
    {
        get; set;
    }

    [JsonProperty(PropertyName = "nativeVersion")]
    public required string NativeVersion
    {
        get; set;
    }

    [JsonProperty(PropertyName = "id")]
    public required int Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "exportOptionType")]
    public AnalysisReportOptionType ExportOptionType
    {
        get; set;
    } = AnalysisReportOptionType.ALL;

    [JsonProperty(PropertyName = "chartNo")]
    public string? ChartNo
    {
        get; set;
    }

    [JsonProperty(PropertyName = "birthYear")]
    public string? BirthYear
    {
        get; set;
    }

    [JsonProperty(PropertyName = "birthMonth")]
    public string? BirthMonth
    {
        get; set;
    }

    [JsonProperty(PropertyName = "birthDay")]
    public string? BirthDay
    {
        get; set;
    }

    [JsonProperty(PropertyName = "assessment")]
    public string? Assessment
    {
        get; set;
    }
}