using System.ComponentModel;

namespace WExpert.Code;

public enum PatientType
{
    [Description("")]
    NONE,
    [Description("Aesthetic")]
    AESTHETIC,
    [Description("Reconstructive")]
    RECONSTRUCTIVE,
    [Description("Both")]
    BOTH
}