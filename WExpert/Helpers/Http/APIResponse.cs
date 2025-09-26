using WExpert.Code;

namespace WExpert.Helpers.Http;

public class APIResponse<T>
{
    public APIResultType Result
    {
        get; set;
    }

    public int ResultCode
    {
        get; set;
    }

    public T? Data
    {
        get; set;
    }

    public string Message
    {
        get; set;
    } = string.Empty;

    protected APIResponse()
    {
    }

    public APIResponse(APIResultType result, string? message = null)
    {
        Result = result;
        ResultCode = result.GetCodeByResult();
        Data = default;
        Message = string.IsNullOrEmpty(message) ? result.GetMessage() : message;
    }

    public APIResponse(APIResultType result, int resultCode, T? data, string? message)
    {
        Result = result;
        ResultCode = resultCode;
        Data = data;
        Message = message ?? string.Empty;
    }

    public override string ToString()
    {
        return $"{{\"ResultCode\": {ResultCode},\"Result\":{Result},\"Message\":\"{Message}\",\"Data\":{Data?.ToString()}}}";
    }
}