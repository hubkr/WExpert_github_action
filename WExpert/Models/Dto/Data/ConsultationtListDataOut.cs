
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ConsultationListDataOut
{
    [JsonProperty(PropertyName = "consultationUsed")]
    public required int ConsultationUsed
    {
        get; set;
    }

    [JsonProperty(PropertyName = "consultationQuota")]
    public required int ConsultationQuota
    {
        get; set;
    }

    [JsonProperty(PropertyName = "consultations")]
    public required List<ConsultationListItemOut> Consultations
    {
        get; set;
    }
}