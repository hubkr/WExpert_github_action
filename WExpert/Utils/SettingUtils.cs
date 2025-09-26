using Newtonsoft.Json;
using WExpert.Code;
using WExpert.Helpers;
using WExpert.Models;
using Windows.Graphics;

namespace WExpert.Utils;

public class SettingUtils
{
    private const string file = "settings.json";

    public static SettingAccount GetAccountInfo()
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);

            if (File.Exists(settingPath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath));
                if (settings != null && settings.Account != null)
                {
                    return settings.Account;
                }
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }

        return new SettingAccount { Rememberid = false, Id = null };
    }

    public static void SetAccountInfo(bool remember, string id)
    {
        var account = GetAccountInfo();
        // 기 설정된 값과 동일한 값인 경우 처리 하지 않음
        if (remember)
        {
            if (remember == account.Rememberid && id == account.Id)
            {
                return;
            }
        }
        else
        {
            if (remember == account.Rememberid && string.IsNullOrEmpty(account.Id))
            {
                return;
            }
        }

        var accountNew = new SettingAccount { Rememberid = remember, Id = remember ? id : string.Empty };
        try
        {
            //var infoString = JsonConvert.SerializeObject(accountNew, Formatting.Indented);
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);

            var settings = File.Exists(settingPath) ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath)) : null;
            settings ??= new Settings();
            settings.SetAccount(accountNew);
            File.WriteAllText(settingPath, settings.ToString());
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }
    }

    public static void SetWindowSettings(WExpertWindowState windowSettings)
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}", FileUtils.GetDirectory(DirectoryType.SETTINGS), Path.DirectorySeparatorChar, file);
            var settings = File.Exists(settingPath) ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath)) : null;
            settings ??= new Settings();
            settings.SetWindowSettings(windowSettings);
            File.WriteAllText(settingPath, settings.ToString());
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }
    }

    public static WExpertWindowState GetWindowSettings()
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);

            if (File.Exists(settingPath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath));
                if (settings != null && settings.WindowSettings != null)
                {
                    return settings.WindowSettings;
                }
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }

        return new WExpertWindowState() { Position = new RectInt32(0, 0, 0, 0) , State = WindowState.Maximized };
    }

    public static void SetAnalysisWidth(double width)
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);
            var settings = File.Exists(settingPath) ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath)) : null;
            settings ??= new Settings();
            settings.SetResultWidth(width);
            File.WriteAllText(settingPath, settings.ToString());
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }
    }

    public static double GetAnalysisWidth()
    {
        var defaultWidth = 450;

        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);
            if (File.Exists(settingPath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath));
                return settings?.ResultWidth ?? defaultWidth;
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }

        return defaultWidth;
    }

    public static void SetLocale(SettingLocale locale)
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);
            var settings = File.Exists(settingPath) ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath)) : null;
            settings ??= new Settings();
            settings.SetLocale(locale);
            File.WriteAllText(settingPath, settings.ToString());
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }
    }

    public static SettingLocale GetLocale()
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);
            if (File.Exists(settingPath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath));
                if (settings != null && settings.Locale != null)
                {
                    return settings.Locale;
                }
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }
        return new SettingLocale() { Language = "en-US" };
    }

#if !PROD
    public static ServerModeType GetMode()
    {
        try
        {
            var settingPath = string.Format("{0}{1}{2}",
                FileUtils.GetDirectory(DirectoryType.SETTINGS),
                Path.DirectorySeparatorChar, file);
            if (File.Exists(settingPath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath));
                if (Enum.TryParse<ServerModeType>(settings?.Mode, true, out var result))
                {
                    return result;
                }
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Fatal(e.ToString());
        }
        
        return ServerModeType.STG; // 기본값은 STG 로 설정
    }
#endif
}