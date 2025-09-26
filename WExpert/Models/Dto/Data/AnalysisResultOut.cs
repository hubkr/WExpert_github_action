using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class AnalysisResultOut
{
    [JsonProperty(PropertyName = "sonographyId")]
    public string SonographyId
    {
        get; set;
    } = string.Empty;

    [JsonProperty(PropertyName = "filename")]
    public string Filename
    {
        get; set;
    } = string.Empty;

    [JsonProperty(PropertyName = "width")]
    public int Width
    {
        get; set;
    }

    [JsonProperty(PropertyName = "height")]
    public int Height
    {
        get; set;
    }

    [JsonProperty(PropertyName = "labels")]
    public List<AnalysisLabelsOut>? Labels
    {
        get; set;
    }
}