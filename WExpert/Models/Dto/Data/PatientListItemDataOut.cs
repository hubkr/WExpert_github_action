using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;

public class PatientListItemDataOut
{
    [JsonProperty(PropertyName = "id")]
    public required int Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "wexpertId")]
    public string? WExpertId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "registeredAt")]
    public required DateTime RegisteredAt
    {
        get; set;
    }

    [JsonProperty(PropertyName = "type")]
    public required PatientType Type
    {
        get; set;
    }

    [JsonProperty(PropertyName = "name")]
    public required string Name
    {
        get; set;
    }

    [JsonProperty(PropertyName = "adminNote")]
    public required AdminNoteOut AdminNote
    {
        get; set;
    }

    [JsonProperty(PropertyName = "sonographyCount")]
    public required int SonographyCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "ruptureTriage")]
    public required int RuptureTriage
    {
        get; set; 
    }

    [JsonProperty(PropertyName = "tcTriage")]
    public required int TCTriage
    {
        get; set;
    }

    [JsonProperty(PropertyName = "reportCount")]
    public required int ReportCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "analysisStatus")]
    public required AnalysisStatusType AnalysisStatus
    {
        get; set;
    }

    [JsonProperty(PropertyName = "consultationSummary")]
    public required ConsultationSummaryOut ConsultationSummary
    {
        get; set;
    }
}