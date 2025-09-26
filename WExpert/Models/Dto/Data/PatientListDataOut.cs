
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class PatientListDataOut
{
    [JsonProperty(PropertyName = "page")]
    public int Page
    {
        get; set;
    }

    [JsonProperty(PropertyName = "limit")]
    public int Limit
    {
        get; set;
    }

    [JsonProperty(PropertyName = "total")]
    public int Total
    {
        get; set;
    }

    [JsonProperty(PropertyName = "newAnswerTotal")]
    public int NewAnswerTotal
    {
        get; set;
    }

    [JsonProperty(PropertyName = "hasNext")]
    public bool HasNext
    {
        get; set;
    }

    [JsonProperty(PropertyName = "data")]
    public List<PatientListItemDataOut>? Data
    {
        get; set;
    }
}