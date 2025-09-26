using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;


public class AnalysisReportRequestPrintIn
{
    [JsonProperty(PropertyName = "requestPrint")]
    public required bool RrequestPrint
    {
        get; set;
    }
}