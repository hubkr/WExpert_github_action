using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace WExpert.Utils;

/// <summary>
/// 일반 Util 함수들
/// </summary>
public class CommonUtils
{
    /// <summary>
    /// byte -> KB/MB/GB/TB 로 단위 변환
    /// </summary>
    /// <param name="byte">byte 단위의 크기</param>
    /// <returns>KB/MB/GB/TB 로 변환된 크기의 데이트(단위 포함)</returns>
    public static string ConvertByteToKMGT(float bytes)
    {
        string[] Group = { "Bytes", "KB", "MB", "GB", "TB" };
        var B = bytes; var G = 0;
        while (B >= 1024 && G < 5)
        {
            B /= 1024;
            G += 1;
        }
        var truncated = (float)(Math.Truncate((double)B * 100.0) / 100.0);
        var load = (truncated + " " + Group[G]);

        return load;
    }

    /*
    /// <summary>
    /// 문자열을 DiagnosticType enum 으로 변환
    /// </summary>
    /// <param name="value">DiagnosticType 의 string 형태</param>
    /// <returns>Diagnostic enum</returns>
    public static Diagnostic.Type StringToDiagnosticType(string value)
    {
        return (Diagnostic.Type)Enum.Parse(typeof(DiagnosticType), value, true);
    }
    */

    /// <summary>
    /// 현재 객체의 상위 parent 객체 항목 검색
    /// </summary>
    /// <param name="child">자식 객체</param>
    /// <returns>검색된 부모 객체</returns>
    public static T? FindParentControl<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);

        while (parent != null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent as T;
    }


    /// <summary>
    /// xaml 에서 하위 child 항목 검색
    /// </summary>
    /// <param name="parent">해당 부모 객체</param>
    /// <returns>검색된 하위 항목</returns>
    public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var foundChild = FindVisualChild<T>(child);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        return null;
    }

    /// <summary>
    /// xaml 에서 하위 child 항목 검색 후 객체들을 반환
    /// </summary>
    /// <param name="parent">해당 부모 객체</param>
    /// <returns>검색된 하위 항목 객체 리스트</returns>
    public static T[] FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        // 결과를 누적할 리스트
        var result = new List<T>();

        // 내부적으로 탐색 로직 처리
        void FindChildren(DependencyObject currentParent)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(currentParent);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(currentParent, i);

                // 자식이 T 타입이면 리스트에 추가
                if (child is T typedChild)
                {
                    result.Add(typedChild);
                }

                // 현재 자식을 기준으로 재귀 탐색
                FindChildren(child);
            }
        }

        // 초기 부모에서 탐색 시작
        FindChildren(parent);

        // 결과를 배열로 반환
        return result.ToArray();
    }

    /// <summary>
    /// random 문자열을 생성후 반환
    /// </summary>
    /// <param name="length">만들고 싶은 문자열의 길이</param>
    /// <returns>요청한 길이의 random 문자열</returns>
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// ObservableCollection 에서 현재 선택 항목의 다음(이전) 항목을 가져옴
    /// </summary>
    /// <param name="collection"> 찾을 ObservableCollection </param>
    /// <param name="currentItem"> 현재 선택된 ObservableCollection 내의 항목 </param>
    /// <returns>다음 항목이 있으면 다음 항목 없으면 이전 항목 이전 항목도 없으면 null 반환 </returns>
    public static T? GetNextOrPreviousItem<T>(ObservableCollection<T> collection, T currentItem)
    {
        var currentIndex = collection.IndexOf(currentItem);
        if (currentIndex != -1)
        {
            if (currentIndex < collection.Count - 1)
            {
                // 다음 항목이 있으면 다음 항목 반환
                return collection[currentIndex + 1];
            }
            else if (currentIndex > 0)
            {
                // 다음 항목이 없고 이전 항목이 있으면 이전 항목 반환
                return collection[currentIndex - 1];
            }
        }

        // 다음 항목과 이전 항목 모두 없을 경우 null 반환
        return default;
    }

    /// <summary>
    /// 소수점 반내림 후 결과 반환
    /// </summary>
    /// <param name="value"> 반내림 하고자 하는 값 </param>
    /// <param name="decimalPlaces"> 반내림 하고자 하는소수점 자리수 </param>
    /// <returns>요청한 소수점 자리 까지 반내림 후 결과 반환</returns>
    public static double FloorToDecimalPlaces(double value, int decimalPlaces)
    {
        var factor = Math.Pow(10, decimalPlaces);
        return Math.Floor(value * factor) / factor;
    }

    /// <summary>
    /// 10 단위의 올림수 계산(ex 10->20, 15->20, 19->20, 21->30, 29->30, 31->40)
    /// </summary>
    /// <param name="number"> 반올림 하고자 하는 숫자 </param>
    /// <returns>처리 결과 반환</returns>
    public static int IncrementByTen(int number, int frequence )
    {
        // 10으로 나눈 몫에 1을 더하고, 다시 10을 곱합니다.
        return ((number - 1) / frequence + 1) * frequence;
    }

    /// <summary>
    /// 10 단위의 내림수 계산(ex 20->10, 21->20, 25->20, 29->20, 30->20)
    /// </summary>
    /// <param name="number"> 반올림 하고자 하는 숫자 </param>
    /// <returns>처리 결과 반환</returns>
    public static int DecrementByTen(int number, int frequency)
    {
        // 10으로 나눈 몫에서 1을 빼고 다시 10을 곱합니다.
        return ((number - 1) / frequency - 1) * frequency;
    }

    /// <summary>
    /// 일정 길이를 넘어가는 문자열 줄임
    /// </summary>
    /// <param name="text"> 원본 문자열 </param>
    /// <param name="maxLength"> 줄이게될 문자열 길이 </param>
    /// <returns>처리 결과 반환</returns>
    public static string TruncateText(string text, int maxLength)
    {
        return text.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength - 3), "...") : text;
    }

    public static DependencyObject? FindChildElementByName(DependencyObject parent, string name)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement element && element.Name == name)
            {
                return child;
            }
            var result = FindChildElementByName(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        // 이메일 형식을 검사하는 정규식 패턴
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    public static bool IsValidPassword(string? password)
    {
        // step1. 비밀번호 길이 체크 (8자~16자)
        if (string.IsNullOrEmpty(password) || password.Length < 8 || password.Length > 16)
        {
            return false;
        }

        // step2. 포함 불가능한 문자 체크
        var patternNotAllowedCharacter = "[^a-zA-Z0-9!\"#$%&'()*+,\\-./:;<=>?@\\[₩\\]^_`{|}~]";
        if (Regex.IsMatch(password, patternNotAllowedCharacter))
        {
            return false;
        }

        // step3. 반드시 포함되어야 할 문자 조건 검사(영어 대.소문자, 숫자, 특수문자 최소 1자 이상씩)
        var hasUpperCase = Regex.IsMatch(password, "[A-Z]"); // 영어 대문자
        var hasLowerCase = Regex.IsMatch(password, "[a-z]"); // 영어 소문자
        var hasDigit = Regex.IsMatch(password, "[0-9]");     // 숫자
        var hasSpecialChar = Regex.IsMatch(password, "[!\"#$%&'()*+,\\-./:;<=>?@\\[₩\\]^_`{|}~]"); // 특수문자

        // step4. 모든 조건을 만족하면 true 반환
        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// HTTP Error Message 를 포맷에 맞게 생성
    /// </summary>
    /// <param name="message">오류 메시지</param>
    /// <param name="code">오류 코드</param>
    /// <param name="isOneLine">한줄 짜리 메시지 여부</param>
    /// <returns>오류 메시지</returns>
    public static string MakeHTTPErrorMessage(string message, int code, bool isOneLine = false)
    {
        return isOneLine ? $"{message} (Error code: {code})" : $"{message} \r\n(Error code: {code})";
    }

    /// <summary>
    /// url 경로의 validation 검증
    /// </summary>
    /// <param name="url">검증 하고자 하는 url</param>
    /// <returns>검증 결과 - true/false</returns>
    public static bool IsValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
