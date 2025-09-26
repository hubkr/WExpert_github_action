using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;


public class AnalysisStatusListOut
{
    [JsonProperty(PropertyName = "patientId")]
    public int PatientId
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

    [JsonProperty(PropertyName = "analysisStatus")]
    public AnalysisStatusType AnalysisStatus
    {
        get; set;
    }
}