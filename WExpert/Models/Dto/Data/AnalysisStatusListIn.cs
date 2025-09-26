using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;


public class AnalysisStatusListIn
{
    [JsonProperty(PropertyName = "patientIds")]
    public required List<int> PatientIds
    {
        get; set;
    }
}