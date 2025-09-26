using Newtonsoft.Json;
using WExpert.Helpers;

namespace WExpert.Models;

public class Settings
{
#if !PROD
    [JsonProperty(PropertyName = "mode")]
    public string? Mode
    {
        get; set;
    }
#endif

    [JsonProperty(PropertyName = "account")]
    public SettingAccount? Account
    {
        get; set;
    }

    [JsonProperty(PropertyName = "window-state")]
    public WExpertWindowState? WindowSettings
    {
        get; set;
    }

    [JsonProperty(PropertyName = "locale")]
    public SettingLocale? Locale
    {
        get; set;
    }

    [JsonProperty(PropertyName = "result-width")]
    public double? ResultWidth
    {
        get; set;
    }

    public void SetAccount(SettingAccount account)
    {
        Account = account;        
    }

    public void SetWindowSettings(WExpertWindowState windowSettings)
    {
        WindowSettings = windowSettings;
    }

    public void SetResultWidth(double width)
    {
        ResultWidth = width;
    }

    public void SetLocale(SettingLocale locale)
    {
        Locale = locale;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public class SettingAccount
{
    [JsonProperty(PropertyName = "rememberid")]
    public bool Rememberid
    {
        get; set;
    }

    [JsonProperty(PropertyName = "id")]
    public string? Id
    {
        get; set;
    }
}

public class SettingLocale
{
    [JsonProperty(PropertyName = "language")]
    public string? Language
    {
        get; set;
    }

    [JsonProperty(PropertyName = "time-zone")]
    public string? TimeZone
    {
        get; set;
    }
}

