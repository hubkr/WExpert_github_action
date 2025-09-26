using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;
public class AnalysisSummaryOut
{
    [JsonProperty(PropertyName = "imageId")]
    public string ImageId
    {
        get; set;
    } = string.Empty;

    [JsonProperty(PropertyName = "progress")]
    public string Progress
    {
        get; set;
    } = string.Empty;

    [JsonProperty(PropertyName = "ruptureTriage")]
    public bool RuptureTriage
    {
        get; set;
    } = false;

    [JsonProperty(PropertyName = "tcTriage")]
    public bool TcTriage
    {
        get; set;
    } = false;
}
