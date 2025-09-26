using System.ComponentModel;

namespace WExpert.Helpers;

// Enum→Description 설정 문자열로 변환
public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field != null)
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }
        
        return value.ToString();
    }
}

// Description 설정 문자열→Enum 값으로 변환
public static class StringEnumExtensions
{
    /// <summary>
    /// description(Attribute.Description) 으로 enum을 찾아 리턴하고,
    /// 없으면 defaultValue 반환
    /// </summary>
    public static T ToEnumByDescription<T>(this string? description, T defaultValue)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(description))
            return defaultValue;

        var lowerDesc = description!.ToLowerInvariant();
        foreach (var val in Enum.GetValues(typeof(T)).Cast<T>())
        {
            var desc = val.GetDescription();
            if (desc != null && desc.ToLowerInvariant() == lowerDesc)
            {
                return val;
            }
        }

        return defaultValue;
    }

    // helper: 중간 연산용 Let 패턴
    private static TResult Let<TSource, TResult>(this TSource self, Func<TSource, TResult> fn)
        => fn(self);
}