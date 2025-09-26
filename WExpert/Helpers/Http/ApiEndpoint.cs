
using WExpert.Code;

namespace WExpert.Helpers.Http;

public class ApiEndpoint
{
    // url path
    public string Path { get; set; }
    // request method(get/post/put/delete/patch)
    public RequestMethodType Method
    { get; set; }
    // post 인 경우 form data 요청 유무
    public bool RequiresFormData { get; set; }

    public ApiEndpoint(RequestMethodType method, string path, bool requiresFormData = false)
    {
        Path             = path;
        Method           = method;
        RequiresFormData = requiresFormData;
    }
}
