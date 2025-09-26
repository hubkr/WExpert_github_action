
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class DeletePatientsIn
{
    [JsonProperty(PropertyName = "ids")]
    public List<int>? ids
    {
        get; set;
    }
}
