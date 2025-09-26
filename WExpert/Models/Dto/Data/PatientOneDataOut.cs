
using Newtonsoft.Json;
using WExpert.Code;

namespace WExpert.Models.Dto.Data;

public class PatientOneDataOut
{
    [JsonProperty(PropertyName = "id")]
    public int Id
    {
        get; set;
    }

    [JsonProperty(PropertyName = "wexpertId")]
    public string? WexpertId
    {
        get; set;
    }

    [JsonProperty(PropertyName = "registeredAt")]
    public DateTime RegisteredAt
    {
        get; set;
    }

    [JsonProperty(PropertyName = "type")]
    public PatientType Type
    {
        get; set;
    }

    [JsonProperty(PropertyName = "name")]
    public string? Name
    {
        get; set;
    }

    [JsonProperty(PropertyName = "adminNote")]
    public AdminNoteOut? AdminNote
    {
        get; set;
    }

    [JsonProperty(PropertyName = "sonographyCount")]
    public int SonographyCount
    {
        get; set;
    }

    [JsonProperty(PropertyName = "sonographies")]
    public List<SonographiesItemDataOut>? Sonographies
    {
        get; set;
    }
}