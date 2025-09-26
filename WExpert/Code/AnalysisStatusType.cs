using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WExpert.Code;

[JsonConverter(typeof(StringEnumConverter))]
public enum AnalysisStatusType
{
    NONE = 0,
    [EnumMember(Value = "inProgress")] // json convert 를 위해 설정
    ANALYZING,
    [EnumMember(Value = "success")] // json convert 를 위해 설정
    COMPLETED,
    [EnumMember(Value = "failure")] // json convert 를 위해 설정
    INCOMPLETE
};