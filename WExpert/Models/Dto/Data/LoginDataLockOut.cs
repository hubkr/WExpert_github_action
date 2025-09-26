
using System.Text.Json;
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class LoginDataLockOut
{
    [JsonProperty(PropertyName = "loginLockReset")]
    public int? LoginLockReset
    {
        get; set;
    }
}