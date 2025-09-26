
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ConsultationListItemOut
{
    [JsonProperty(PropertyName = "id")]
    public required int Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "createdAt")]
    public required DateTime CreatedAt
    {
        get; set;
    }

    [JsonProperty(PropertyName = "question")]
    public required string Question
    {
        get; set;
    }

    [JsonProperty(PropertyName = "answer")]
    public ConsultationAnswerDataOut? Answer
    {
        get; set;
    }
}