using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class CreateConsultationOut
{
    [JsonProperty(PropertyName = "id")]
    public int Id
    {
        get; set;
    }
}