using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;

public class CreatePatientOut
{
    [JsonProperty(PropertyName = "id")]
    public int Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "wexpertId")]
    public required string WExpertId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "ruptureTriage")]
    public int RuptureTriage
    {
        get; set;
    }

    [JsonProperty(PropertyName = "tcTriage")]
    public int TCTriage
    {
        get; set;
    }

    [JsonProperty(PropertyName = "sonographyCount")]
    public int SonographyCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "name")]
    public string? Name
    {
        get; set;
    }

    [JsonProperty(PropertyName = "reportCount")]
    public int ReportCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "registeredAt")]
    public DateTime RegisteredAt
    {
        get; set;
    }

    [JsonProperty(PropertyName = "analysisStatus")]
    public required AnalysisStatusType AnalysisStatus
    {
        get; set;
    }

    [JsonProperty(PropertyName = "type")]
    public required PatientType Type
    {
        get; set;
    }
}