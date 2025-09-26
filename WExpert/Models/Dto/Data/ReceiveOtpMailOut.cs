
using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ReceiveOtpMailOut
{
    [JsonProperty(PropertyName = "otpExpireAt")]
    public required DateTime OtpExpireAt
    {
        get; set;
    }
}
