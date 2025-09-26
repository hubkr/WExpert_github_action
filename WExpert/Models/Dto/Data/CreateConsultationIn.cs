
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class CreateConsultationIn
{
    [JsonProperty(PropertyName = "sonographyId")]
    public string? SonographyId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "question")]
    public string? Question
    {
        get; set;
    }

    //[JsonProperty(PropertyName = "algorithmCodes")]
    //public List<string>? AlgorithmCodes
    //{
    //    get; set;
    //}
}
