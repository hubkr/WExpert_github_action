
using System.ComponentModel;

namespace WExpert.Code;

public enum TriageFilterType
{
    [Description("rupture")]
    RUPTURE,
    [Description("thickened_capsule")]
    THICKENED_CAPSULE,
    [Description("normal")]
    NORMAL
}