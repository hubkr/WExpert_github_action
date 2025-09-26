using WExpert.Code;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;

namespace WExpert.Contracts.Services;

public interface IRestApiService
{
    LoginDataOut GetLoginInfo();
    XLicenseOut GetLicenseInfo();
    Task<(string mimeType, long fileSize)> GetImageInfoFromUrlAsync(string url);
    Task<APIResponse<byte[]>> HttpGetByteArrayAsync(string url);
    Task<APIResponse<object>> LoginAsync(string id, string password, bool forceLogin);
    Task LogoutAsync();
    Task<APIResultType> CheckTokenValid();
    Task<APIResponse<T>> DataRequestAsync<T>(RequestMethodType mothod, string path, bool requiresFormData, string? token = null, object? content = null);
}
