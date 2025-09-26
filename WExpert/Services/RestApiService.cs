using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using WExpert.Code;
using WExpert.Contracts.Services;
using WExpert.Helpers;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using Windows.Networking.Connectivity;
using Windows.Storage.Provider;

namespace WExpert.Services;

public partial class RestApiService : IRestApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly LoginDataOut _loginData;
    private readonly INotificationService _notificationService;

    public RestApiService(INotificationService notificationService)
    {
        _httpClient = new HttpClient();
        _loginData = new();
        _notificationService = notificationService;
    }

    /// <summary>
    /// Service 종료 시 처리
    /// </summary>
    public async void Dispose() // App 종료 시 처리
    {
        await LogoutAsync();

        // 기존 인스턴스 정리
        _httpClient.Dispose();
        GC.SuppressFinalize(this); // CA1816: Dispose 메서드에서 GC.SuppressFinalize 호출 추가  
    }

    /// <summary>
    /// 현재 로그인 정보 반환
    /// </summary>
    /// <returns>로그인 정보 object</returns>
    public LoginDataOut GetLoginInfo()
    {
        return _loginData;
    }

    /// <summary>
    /// 서버로 부터 받은 JWT 토큰에 라이선스 정보 축출
    /// </summary>
    public XLicenseOut GetLicenseInfo()
    {
        XLicenseOut? license = null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(_loginData.AccessToken);

            if (jwtToken.Payload.TryGetValue("x-license", out var jsonObj) && jsonObj is JsonElement jsonElement)
            {
                // JsonElement를 JSON 문자열로 변환
                var jsonStr = jsonElement.GetRawText();
                // JSON을 C# 객체로 역직렬화
                license = JsonConvert.DeserializeObject<XLicenseOut>(jsonStr);
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"License info read error: {e}");
        }

        return license ?? new XLicenseOut();
    }

    /// <summary>
    /// url의 파일 정보 확인
    /// </summary>
    /// <param name="url">이미지 URL</param>
    /// <returns>이미지의 mime type, file size 정보</returns>
    public async Task<(string mimeType, long fileSize)> GetImageInfoFromUrlAsync(string url)
    {
        HttpResponseMessage? response = null;

        try
        {
            // GET 요청을 보내지만 실제 콘텐츠는 다운로드하지 않습니다
            response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "Unknown";
            var fileSize = response.Content.Headers.ContentLength ?? -1;

            return (mimeType, fileSize);
        }
        finally
        {
            // 요청 후 HttpClient의 DefaultRequestHeaders를 초기화
            _httpClient.DefaultRequestHeaders.Clear();

            // 응답 객체가 null이 아닐 경우 Dispose 호출
            response?.Dispose();
        }
    }

    /// <summary>
    /// 이미지 data byte 를 읽어 옴.
    /// </summary>
    /// <param name="url">요청 서버 URL</param>
    /// <returns>APIResponse(이미지 binary data)</returns>
    public async Task<APIResponse<byte[]>> HttpGetByteArrayAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetByteArrayAsync(url);
            // 여기서 T가 byte[]와 호환되는지 확인해야 합니다.
            if (response is not null && response.Length > 0)
            {
                var resultCode = APIResultUtil.GetCodeByResult(APIResultType.SUCCESS);
                var message = APIResultUtil.GetMessage(APIResultType.SUCCESS);
                return new APIResponse<byte[]>(APIResultType.SUCCESS, resultCode, response, message);
            }
            else
            {
                return new APIResponse<byte[]>(APIResultType.SERVER_ERROR, "Response is empty.");
            }
        }
        catch (HttpRequestException e)
        {
            var result = APIResultUtil.GetResultByHttpRequestErrorCode(e.HttpRequestError);
            if (result == APIResultType.UNKNOWN && e.StatusCode.HasValue)
            {
                var apiResult = APIResultUtil.GetResultByHttpErrorCode(e.StatusCode);
                return new APIResponse<byte[]>(apiResult, $"HTTP error: {e.StatusCode}");
            }
            return new APIResponse<byte[]>(result, e.Message);
        }
        catch (TaskCanceledException)
        {
            return new APIResponse<byte[]>(APIResultType.SERVER_ERROR, "Request timed out.");
        }
        catch (Exception e)
        {
            return new APIResponse<byte[]>(APIResultType.SERVER_ERROR, $"Unexpected error: {e}");
        }
        finally
        {
            // 요청 후 HttpClient의 DefaultRequestHeaders를 초기화
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }

    /// <summary>
    /// Login 요청
    /// </summary>
    /// <param name="id">로그인 id</param>
    /// <param name="password">로그인 password</param>
    /// <param name="forceLogin">중복 접속 상태인 경우 강제 로그인 여부</param>
    /// <returns>APIResponse(LoginDataOut)</returns>
    public async Task<APIResponse<object>> LoginAsync(string id, string password, bool forceLogin)
    {
        try
        {
            var clientVersion = WExpertDefine.GetVersion();
            var deviceInfo = $"{RuntimeInformation.OSDescription}; {RuntimeInformation.OSArchitecture}";
            var clientIdentifier = string.Empty;
            using (var sha256 = SHA256.Create())
            {
                // 현재 Windows 사용자의 보안 식별자(SID) 를 이용한 indentity 값 생성
                clientIdentifier = $"WExpert-{id}-{WindowsIdentity.GetCurrent()?.User?.Value}";
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(clientIdentifier));
                clientIdentifier = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            var userAgent = new UserAgentIn { ClientVersion = clientVersion, ClientIdentifier = clientIdentifier, DeviceInfo = deviceInfo };
            var login = new LoginIn() { LoginId = id ?? string.Empty, Password = password ?? string.Empty, ForceLogin = forceLogin, UserAgent = userAgent };
            var response = await DataRequestAsync<object>(ApiRoutes.LOGIN.Method, ApiRoutes.LOGIN.Path, ApiRoutes.LOGIN.RequiresFormData, null, login);
            if (response.Result is APIResultType.SUCCESS)
            {
                var jsonString = JsonConvert.SerializeObject(response.Data); // response.Data를 JSON으로 직렬화된 문자열로 변환
                var loginDataOut = JsonConvert.DeserializeObject<LoginDataOut>(jsonString); // JSON 문자열을 객체로 변환
                _loginData.CopyData(id, loginDataOut);
            }

            return response;
        }
        catch (Exception)
        {
            var message = CommonUtils.MakeHTTPErrorMessage("StringInternalServerErrorMessage".GetLocalized(), (int)APIResultType.INTERNAL_SERVER_ERROR);
            throw new Exception(message);
        }
        finally
        {
            // 요청 후 HttpClient의 DefaultRequestHeaders를 초기화
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }

    /// <summary>
    /// Logout 요청
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            // 서버에 로그아웃 요청
            var token = _loginData.AccessToken;
            var response = await DataRequestAsync<object>(ApiRoutes.LOGOUT.Method, ApiRoutes.LOGOUT.Path, ApiRoutes.LOGOUT.RequiresFormData, token);
            if (response.Result is APIResultType.SUCCESS)
            {
                _loginData.CopyData(null, null); // 로그인 정보 초기화
            }
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            WExpertLogger.Instance.Error($"Logout error : {e}");
        }
        finally
        {
            // 요청 후 HttpClient의 DefaultRequestHeaders를 초기화
            _httpClient.DefaultRequestHeaders.Clear();
        }
    }

    /// <summary>
    /// 서버로 부터 받은 JWT 토큰에 대해 만료 여부 체크
    /// </summary>
    public async Task<APIResultType> CheckTokenValid()
    {
        try
        {
            var token = _loginData.AccessToken;
            var response = await DataRequestAsync<object>(ApiRoutes.CHECK_TOKEN.Method, ApiRoutes.CHECK_TOKEN.Path,
                                                          ApiRoutes.CHECK_TOKEN.RequiresFormData, token);
            return response.Result;
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"Check token valid error : {e}");
            return APIResultType.UNKNOWN;
        }
    }

    /// <summary>
    /// 일반 Restful API 요청
    /// </summary>
    /// <param name="url">요청 서버 URL</param>
    /// <param name="requestFileInfo">request로 요청할 정보</param>
    /// <param name="model">분석 요청 화면 data model</param>
    /// <returns>WaiApiResponse(분석 요청 결과)</returns>
    public async Task<APIResponse<T>> DataRequestAsync<T>(RequestMethodType mothod, string path, bool requiresFormData, string? token = null, object? content = null)
    {
        /*
        // 네트워크 연결 상태 확인
        var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
        WExpertLogger.Instance.Debug($"############ [Network]Check network connectivity level: {connectionProfile?.GetNetworkConnectivityLevel()} [{DateTime.Now}]");
        if (connectionProfile == null ||
            connectionProfile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
        {
            _notificationService.ShowNotification("StringNotice".GetLocalized(), "네트워크 연결을 확인해주세요.");
            //ShowMessage("네트워크 연결을 확인해주세요.");
            return new APIResponse<T>(APIResultType.NETWORK_STATUS_ERROR, "Time out-the request was canceled.");            
        }
        */

        HttpResponseMessage? response = null;

        try
        {
            var uri = $"{WExpertDefine.GetAPIServerUrl()}/{path}";

            // Add the Bearer Token to the Authorization header
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (mothod == RequestMethodType.POST)
            {
                HttpContent? httpContent = null;
                if (requiresFormData && content != null)
                {
                    httpContent = content as HttpContent;
                }
                else if (content != null)
                {
                    var jsonContent = JsonConvert.SerializeObject(content);
                    httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }
                else
                {
                    httpContent = new StringContent(string.Empty);
                }
#if DEBUG
                WExpertLogger.Instance.Debug($"[Network]Post request uri {uri} [{DateTime.Now}]");
#endif
                response = await _httpClient.PostAsync(uri, httpContent);
            }
            else if (mothod == RequestMethodType.GET)
            {
#if DEBUG
                WExpertLogger.Instance.Debug($"[Network]Get request uri {uri} [{DateTime.Now}]");
#endif
                response = await _httpClient.GetAsync(uri);
            }
            else
            {
                // PUT, DELETE
                return new APIResponse<T>(APIResultType.INTERNAL_ERROR, "Not support yet.");
            }

            response.EnsureSuccessStatusCode();
            // 응답 상태 코드별 처리
            if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                // 요청이 성공적인 경우 응답 본문 읽기
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<APIResponse<T>>(responseBody);
                return result ?? new APIResponse<T>(APIResultType.SERVER_ERROR, "Response is null.");
            }
            else
            {
                return new APIResponse<T>(APIResultType.SERVER_ERROR, "Response state is not success.");
            }
        }
        catch (HttpRequestException re)
        {
            var result = APIResultUtil.GetResultByHttpRequestErrorCode(re.HttpRequestError);

            // Http request error 이 Unknown 인 경우 http error code 가 내려옴
            if (result == APIResultType.UNKNOWN)
            {
                // 요청이 실패한 경우 상태 코드별 처리
                var apiResult = APIResultUtil.GetResultByHttpErrorCode(re?.StatusCode);
                if (response is not null)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    APIResponse<T>? body = null;
                    try
                    {
                        // HTTP Error 상태에서도 body 가 json 형태로 내려 오는 경우 처리
                        body = JsonConvert.DeserializeObject<APIResponse<T>>(responseBody);
                    }
                    catch (JsonReaderException je)
                    {
                        // JSON 파싱 오류 처리(로그만 남기고 body 는 null 처리)
                        WExpertLogger.Instance.Error($"JSON read error: {je.Message}");
                    }
                    catch (Exception e)
                    {
                        // 일반 예외 처리(로그만 남기고 body 는 null 처리)
                        WExpertLogger.Instance.Error($"JSON deserialize error: {e}");
                    }
                    return body ?? new APIResponse<T>(apiResult);
                }

                return new APIResponse<T>(apiResult);
            }
            else
            {
                return new APIResponse<T>(result);
            }
        }
        catch (TaskCanceledException)
        {
            // 요청 타임아웃 예외 처리
            return new APIResponse<T>(APIResultType.REQUEST_TIMEOUT, "Time out-the request was canceled.");
        }
        catch (JsonReaderException)
        {
            return new APIResponse<T>(APIResultType.INVALID_RESPONSE, "Invalid response was received.");
        }
        catch (Exception e)
        {
            // 일반 예외 처리
            return new APIResponse<T>(APIResultType.SERVER_ERROR, $"Unexpected errors : {e}");
        }
        finally
        {
            // 요청 후 HttpClient의 DefaultRequestHeaders를 초기화
            _httpClient.DefaultRequestHeaders.Clear();

            // 응답 객체가 null이 아닐 경우 Dispose 호출
            response?.Dispose();
        }
    }
}
