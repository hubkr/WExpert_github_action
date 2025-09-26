using Newtonsoft.Json;

namespace WExpert.Models.Dto.Data;

public class ProfileHospitalOut
{
    [JsonProperty(PropertyName = "name")]
    public string? Name
    {
        get; set;
    }

    [JsonProperty(PropertyName = "country")]
    public string? Country
    {
        get; set;
    }

    [JsonProperty(PropertyName = "address")]
    public string? Address
    {
        get; set;
    }

    [JsonProperty(PropertyName = "contactPerson")]
    public string? ContactPerson
    {
        get; set;
    }

    [JsonProperty(PropertyName = "phoneNumber")]
    public string? PhoneNumber
    {
        get; set;
    }
}
