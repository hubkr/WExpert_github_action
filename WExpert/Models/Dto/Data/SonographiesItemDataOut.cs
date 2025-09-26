using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class SonographiesItemDataOut
{
    [JsonProperty(PropertyName = "id")]
    public string? Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "imageUrl")]
    public string? ImageUrl
    {
        get; set;
    }

    [JsonProperty(PropertyName = "originalFileName")]
    public string? OriginalFileName
    {
        get; set;
    }

    [JsonProperty(PropertyName = "analysis")]
    public AnalysisResultOut? Analysis
    {
        get; set;
    }

    [JsonProperty(PropertyName = "consultationSummary")]
    public required ConsultationSummaryOut ConsultationSummary
    {
        get; set;
    }
}