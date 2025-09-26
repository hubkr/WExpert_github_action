
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ConsultationAnswerDataOut
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

    [JsonProperty(PropertyName = "answer")]
    public required string Answer
    {
        get; set;
    }

    [JsonProperty(PropertyName = "attachmentUrl")]
    public string? AttachmentUrl
    {
        get; set;
    }
}