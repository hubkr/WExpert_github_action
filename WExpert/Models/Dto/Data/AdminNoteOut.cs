using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class AdminNoteOut
{
    [JsonProperty(PropertyName = "note")]
    public required string Note
    {
        get; set;
    }

    [JsonProperty(PropertyName = "updatedAt")]
    public required DateTime UpdatedAt
    {
        get; set;
    }
}