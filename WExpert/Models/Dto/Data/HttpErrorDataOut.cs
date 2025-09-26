using System.Net;

namespace WExpert.Models.Dto.Data;

public class HttpErrorDataOut
{
    public HttpRequestError HttpRequestError
    {
        get; set;
    }

    public HttpStatusCode? StatusCode
    {
        get; set;
    }

    public string? Message
    {
        get; set;
    }
}
