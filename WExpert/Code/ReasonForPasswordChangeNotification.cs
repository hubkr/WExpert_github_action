using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WExpert.Code;

[JsonConverter(typeof(StringEnumConverter))]
public enum ReasonForPasswordChangeNotification
{
    [EnumMember(Value = "temporary_password")] // json convert 를 위해 설정(관리자에 의한 비밀번호 초기화 상태)
    TEMPORARY_PASSWORD,
    [EnumMember(Value = "password_expired")] // json convert 를 위해 설정(비밀번호 만료 상태)
    PASSWORD_EXPIRED,
    [EnumMember(Value = "initial_password")] // json convert 를 위해 설정(비밀번호 초기 설정 상태)
    INITIAL_PASSWORD
};
