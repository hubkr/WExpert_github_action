using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ConsultationSummaryOut
{
    [JsonProperty(PropertyName = "questionCount")]
    public required int QuestionCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "answerCount")]
    public required int AnswerCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "hasNewAnswers")]
    public required bool HasNewAnswers
    {
        get; set;
    }
}