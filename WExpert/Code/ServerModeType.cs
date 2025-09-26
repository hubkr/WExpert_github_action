using System.ComponentModel;

namespace WExpert.Code;

public enum ServerModeType
{
    [Description("dev")]
    DEV,    // 개발
    [Description("stg")]
    STG,    // Staging
    [Description("prod")]
    PROD    // 상용
}