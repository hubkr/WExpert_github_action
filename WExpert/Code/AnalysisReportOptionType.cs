
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WExpert.Code;

[JsonConverter(typeof(StringEnumConverter))]
public enum AnalysisReportOptionType
{
    [EnumMember(Value = "all")]
    ALL,
    [EnumMember(Value = "only_positive_case")]
    ONLY_POSITIVE_CASE
}