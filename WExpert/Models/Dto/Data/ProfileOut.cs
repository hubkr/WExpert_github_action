using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ProfileOut
{
    [JsonProperty(PropertyName = "account")]
    public required ProfileAccountOut Account
    {
        get; set;
    }

    [JsonProperty(PropertyName = "license")]
    public required ProfileLicenseOut License
    {
        get; set;
    }

    [JsonProperty(PropertyName = "hospital")]
    public required ProfileHospitalOut Hospital
    {
        get; set;
    }
}

