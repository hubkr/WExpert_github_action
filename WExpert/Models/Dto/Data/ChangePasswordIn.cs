using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ChangePasswordIn
{
    [JsonProperty(PropertyName = "oldPassword")]
    public string? OldPassword
    {
        get; set;
    }

    [JsonProperty(PropertyName = "newPassword")]
    public string? NewPassword
    {
        get; set;
    }
}
