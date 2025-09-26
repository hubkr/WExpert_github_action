using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ResetPasswordOut
{
    [JsonProperty(PropertyName = "email")]
    public string? Email
    {
        get; set;
    }
}
