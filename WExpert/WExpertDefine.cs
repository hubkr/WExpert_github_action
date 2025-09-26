using System.Reflection;
using WExpert.Code;
using WExpert.Helpers;
using WExpert.Utils;
using Windows.ApplicationModel;

namespace WExpert;

public static class WExpertDefine
{
    // 프로그램 중복 실행 방지를 위한 MutexName
    public const string ExecutionMutexName = "wai.WExpert";

    // 환자 등록시 등록 가능 최대 파일 개수
    public const int MAX_REGISTRATION_COUNT = 30;

    // report assessment 최대 입력 글자수
    public const int MAX_REPORT_ASSESSMENT_INPUT = 1000;

    // Consultation 질문 최대 입력 글자 수
    public const int MAX_QUESTION_INPUT = 500;

    public static string GetVersion(bool showBuildNumber = false)
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        if (showBuildNumber)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}.{GetBuildNumber()}"; //.{version.Revision}";
        }
        else
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    public static string GetBuildDate()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var infoVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (infoVersionAttribute != null)
            {
                var versionInfo = infoVersionAttribute.InformationalVersion;
                var parts = versionInfo.Split('-', '+');

                if (parts.Length >= 2)
                {
                    if (DateTime.TryParseExact(parts[1], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out var buildDate))
                    {
                        return buildDate.ToString("yyyy-MM-dd");
                    }
                }

                WExpertLogger.Instance.Error($"(Get build date) Unexpected version format: {versionInfo}");
            }
        }
        catch (Exception ex)
        {
            WExpertLogger.Instance.Error($"(Get build date) Error getting build date: {ex.Message}");
        }

        return "Unknown";
    }

    public static string GetBuildNumber()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var infoVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (infoVersionAttribute != null)
            {
                var versionInfo = infoVersionAttribute.InformationalVersion;
                var parts = versionInfo.Split('-', '+');

                if (parts.Length >= 2)
                {
                    var buildPart = parts[1]; // 이제 yyDDD 형식
                    if (buildPart.Length == 5)
                    {
                        /*
                        // yyDDD 형식에서 빌드 날짜 계산, yyDDD → 연도, Julian Day로 변환
                        var yy = int.Parse(buildPart.Substring(0, 2));
                        var ddd = int.Parse(buildPart.Substring(2, 3));

                        // 2000년대 기준으로 변환 (필요 시 1900~2099 범위 조정 가능)
                        var year = 2000 + yy;

                        var buildDate = new DateTime(year, 1, 1).AddDays(ddd - 1);

                        // 빌드 날짜 반환 (원하면 문자열 형식 변경 가능)
                        return buildDate.ToString("yyyy-MM-dd");
                        */
                        return buildPart; // yyDDD 형식 그대로 반환
                    }
                }

                WExpertLogger.Instance.Error($"(Get build number) Unexpected version format: {versionInfo}");
            }
        }
        catch (Exception ex)
        {
            WExpertLogger.Instance.Error($"(Get build number) Error getting build date: {ex.Message}");
        }

        return string.Empty;
    }


    public static string GetAPIServerUrl()
    {
#if PROD
        var address = "https://wexpert-api.w-ai.ai";
#else
        var url = "https://{0}wexpert-api.w-ai.ai";
        var address = SettingUtils.GetMode() switch
        {
            ServerModeType.DEV => string.Format(url, "dev-"),
            ServerModeType.STG => string.Format(url, "stage-"),
            ServerModeType.PROD => string.Format(url, ""),
            _ => string.Format(url, ""),
        };
#endif
        return address;
    }

    public static string GetReportServerUrl()
    {
#if PROD
        var address = "https://wexpert-report-preview.w-ai.ai";
#else
        var url = "https://{0}wexpert-report-preview.w-ai.ai";
        var address = SettingUtils.GetMode() switch
        {
            ServerModeType.DEV => string.Format(url, "dev-"),
            ServerModeType.STG => string.Format(url, "stage-"),
            ServerModeType.PROD => string.Format(url, ""),
            _ => string.Format(url, ""),
        };
#endif
        return address;
    }

    public struct ImageInfos(string mimeType, int width, int height, long size)
    {
        public string MimeType  = mimeType;
        public int Width        = width;
        public int Height       = height;
        //public int Depth        = depth;
        public long size         = size;
    };
}
