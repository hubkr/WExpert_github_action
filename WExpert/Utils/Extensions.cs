namespace WExpert.Utils;

public interface IServerResponseHandler
{
    void HandleServerResponse(string type, object? responseData);
}

public static class Extensions
{
    public static void Let<T>(this T self, Action<T> action)
    {
        if (self != null)
        {
            action(self);
        }
    }
}
