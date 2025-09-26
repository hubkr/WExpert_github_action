
using System.ComponentModel;

public enum ConsultationPlanType
{
    [Description("Basic")]
    BASIC = 0,
    [Description("Advanced")]
    ADVANCED,
    [Description("Ultra")]
    ULTRA,
    [Description("Premium")]
    PREMIUM,
    [Description("Platinum")]
    PLATINUM
};