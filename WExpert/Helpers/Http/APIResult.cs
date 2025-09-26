
using System.Net;
using WExpert.Code;

namespace WExpert.Helpers.Http;

public static class APIResultUtil
{
    /// <summary>
    /// APIResultType -> result code(int) 변환
    /// </summary>
    /// <param name="result">APIResultType</param>
    /// <returns>result code(int)</returns>
    public static int GetCodeByResult(this APIResultType result)
    {
        return (int)result;
    }

    /// <summary>
    /// result code(int) -> APIResultType enum 변환
    /// </summary>
    /// <param name="resultCode">int 형 result code</param>
    /// <returns>APIResultType</returns>
    public static APIResultType GetResultByCode(int resultCode)
    {
        return Enum.GetValues(typeof(APIResultType))
            .Cast<APIResultType>()
            .FirstOrDefault(o => (int)o == resultCode);
    }

    /// <summary>
    /// APIResultType 코드에 따른 message 변환
    /// </summary>
    /// <param name="result">APIResultType code</param>
    /// <returns>message</returns>
    public static string GetMessage(this APIResultType result)
    {
        return result switch
        {
            // API Common Error Area
            APIResultType.SERVER_ERROR => "Server error.",
            APIResultType.NONE => "NONE",
            // HTTP Request Errors
            APIResultType.UNKNOWN => "Unknown error.",
            APIResultType.NAME_RESOLUTION_ERROR => "Name resolution error.",
            APIResultType.CONNECTION_ERROR => "Connection error.",
            APIResultType.SECURE_CONNECTION_ERROR => "Secure connection error.",
            APIResultType.HTTP_PROTOCOL_ERROR => "HTTP protocol error.",
            APIResultType.EXTENDED_CONNECTION_NOT_SUPPORTED => "Extended connection not supported.",
            APIResultType.VERSION_NEGOTATION_ERROR => "Version negotation error.",
            APIResultType.USER_AUTHENTICATION_ERROR => "User authentication error.",
            APIResultType.PROXY_TUNNEL_ERROR => "Proxy tunnel error.",
            APIResultType.INVALID_RESPONSE => "Invalid response.",
            APIResultType.RESPONSE_ENDED => "Response ended.",
            APIResultType.CONFIGURATION_LIMIT_EXCEEDED => "Configuration limit exceeded.",
            // HTTP Errors
            APIResultType.CONTINUE => "Continue",
            APIResultType.SWITCHING_PROTOCOLS => "Switching protocols",
            APIResultType.PROCESSING => "Processing",
            APIResultType.EARLY_HINTS => "Early hints",
            APIResultType.SUCCESS => "Success.",
            APIResultType.CREATED => "Created",
            APIResultType.ACCEPTED => "Accepted",
            APIResultType.NON_AUTHORITATIVE_INFORMATION => "Non authoritative information",
            APIResultType.NO_CONTENT => "No content",
            APIResultType.RESET_CONTENT => "Reset content",
            APIResultType.PARTIAL_CONTENT => "Partial content",
            APIResultType.MULTI_STATUS => "Multi status",
            APIResultType.ALREADY_REPORTED => "Already reported",
            APIResultType.IM_USED => "Im used",
            APIResultType.AMBIGUOUS => "Ambiguous",
            //APIResultType.MULTIPLE_CHOICES => "Multiple choices",
            APIResultType.MOVED => "Moved",
            //APIResultType.MOVED_PERMANENTLY => "Moved permanently",
            APIResultType.FOUND => "Found",
            //APIResultType.REDIRECT => "Redirect",
            APIResultType.REDIRECT_METHOD => "Redirect method",
            //APIResultType.SEE_OTHER => "See other",
            APIResultType.NOT_MODIFIED => "Not modified",
            APIResultType.USE_PROXY => "Use proxy",
            APIResultType.UNUSED => "Unused",
            APIResultType.REDIRECT_KEEP_VERB => "Redirect keep verb",
            //APIResultType.TEMPORARY_REDIRECT => "Temporary redirect",
            APIResultType.PERMANENT_REDIRECT => "Permanent redirect",
            //APIResultType.BAD_REQUEST => "Bad request",
            //APIResultType.UNAUTHORIZED => "Unauthorized",
            APIResultType.PAYMENT_REQUIRED => "Payment required",
            //APIResultType.FORBIDDEN => "Forbidden",
            //APIResultType.NOT_FOUND => "Not found",
            APIResultType.METHOD_NOT_ALLOWED => "Method not allowed",
            APIResultType.NOT_ACCEPTABLE => "Not acceptable",
            APIResultType.PROXY_AUTHENTICATION_REQUIRED => "Proxy authentication required",
            APIResultType.REQUEST_TIMEOUT => "Request timeout",
            APIResultType.CONFLICT => "Conflict",
            APIResultType.GONE => "Gone",
            APIResultType.LENGTH_REQUIRED => "Length required",
            APIResultType.PRECONDITION_FAILED => "Precondition failed",
            APIResultType.REQUEST_ENTITY_TOO_LARGE => "Request entity too large",
            APIResultType.REQUEST_URI_TOO_LONG => "Request uri too long",
            APIResultType.UNSUPPORTED_MEDIA_TYPE => "Unsupported media type",
            APIResultType.REQUESTED_RANGE_NOT_SATISFIABLE => "Requested range not satisfiable",
            APIResultType.EXPECTATION_FAILED => "Expectation failed",
            APIResultType.MISDIRECTED_REQUEST => "Misdirected request",
            APIResultType.UNPROCESSABLE_ENTITY => "Unprocessable entity",
            //APIResultType.UNPROCESSABLE_CONTENT => "Unprocessable content",
            APIResultType.LOCKED => "Locked",
            APIResultType.FAILED_DEPENDENCY => "Failed dependency",
            APIResultType.UPGRADE_REQUIRED => "Upgrade required",
            APIResultType.PRECONDITION_REQUIRED => "Precondition required",
            APIResultType.TOO_MANY_REQUESTS => "Too many requests",
            APIResultType.REQUEST_HEADER_FIELDS_TOO_LARGE => "Request header fields too large",
            APIResultType.UNAVAILABLE_FOR_LEGAL_REASONS => "Unavailable for legal reasons",
            //APIResultType.INTERNAL_SERVER_ERROR => "Internal server error",
            APIResultType.NOT_IMPLEMENTED => "Not implemented",
            APIResultType.BAD_GATEWAY => "Bad gateway",
            //APIResultType.SERVICE_UNAVAILABLE => "Service unavailable",
            APIResultType.GATEWAY_TIMEOUT => "Gateway timeout",
            APIResultType.HTTP_VERSION_NOT_SUPPORTED => "Http version not supported",
            APIResultType.VARIANT_ALSO_NEGOTIATES => "Variant also negotiates",
            APIResultType.INSUFFICIENT_STORAGE => "Insufficient storage",
            APIResultType.LOOP_DETECTED => "Loop detected",
            APIResultType.NOT_EXTENDED => "Not extended",
            APIResultType.NETWORK_AUTHENTICATION_REQUIRED => "Network authentication required",
            APIResultType.BAD_REQUEST => "Bad request.",
            APIResultType.NOT_FOUND => "Not found.",
            APIResultType.FORBIDDEN => "Forbidden.",
            APIResultType.UNAUTHORIZED => "ApiResponse unauthorized.",
            APIResultType.INTERNAL_SERVER_ERROR => "Internal server error.",
            APIResultType.SERVICE_UNAVAILABLE => "Service unavailable.",
            APIResultType.ALREADY_REQUESTED => "Already requesting.",
            APIResultType.EXCEEDING_MAX_REQUEST => "Exceeding the maximum number of requests.",
            APIResultType.HTTP_ERROR => "HTTP error.",
            APIResultType.HTTP_REQUEST_ERROR => "HTTP request error.",
            APIResultType.NETWORK_ERROR => "Network error.",
            APIResultType.INTERNAL_ERROR => "Internal error.",
            _ => "An unexpected error.",
        };
    }

    /// <summary>
    /// HttpStatusCode -> APIResultType 코드로 변환
    /// </summary>
    /// <param name="code">HttpStatusCode code</param>
    /// <returns>APIResultType code</returns>
    public static APIResultType GetResultByHttpErrorCode(HttpStatusCode? code)
    {
        if (code == null)
        {
            return APIResultType.HTTP_ERROR;
        }

        var statusCodeMapping = new Dictionary<HttpStatusCode, APIResultType>
        {
            { HttpStatusCode.Continue, APIResultType.CONTINUE },
            { HttpStatusCode.SwitchingProtocols, APIResultType.SWITCHING_PROTOCOLS },
            { HttpStatusCode.Processing, APIResultType.PROCESSING },
            { HttpStatusCode.EarlyHints, APIResultType.EARLY_HINTS },
            { HttpStatusCode.OK, APIResultType.SUCCESS },
            { HttpStatusCode.Created, APIResultType.CREATED },
            { HttpStatusCode.Accepted, APIResultType.ACCEPTED },
            { HttpStatusCode.NonAuthoritativeInformation, APIResultType.NON_AUTHORITATIVE_INFORMATION },
            { HttpStatusCode.NoContent, APIResultType.NO_CONTENT },
            { HttpStatusCode.ResetContent, APIResultType.RESET_CONTENT },
            { HttpStatusCode.PartialContent, APIResultType.PARTIAL_CONTENT },
            { HttpStatusCode.MultiStatus, APIResultType.MULTI_STATUS },
            { HttpStatusCode.AlreadyReported, APIResultType.ALREADY_REPORTED },
            { HttpStatusCode.IMUsed, APIResultType.IM_USED },
            { HttpStatusCode.Ambiguous, APIResultType.AMBIGUOUS },
            //{ HttpStatusCode.MultipleChoices, APIResultType.MULTIPLE_CHOICES },
            { HttpStatusCode.Moved, APIResultType.MOVED },
            //{ HttpStatusCode.MovedPermanently, APIResultType.MOVED_PERMANENTLY },
            { HttpStatusCode.Found, APIResultType.FOUND },
            //{ HttpStatusCode.Redirect, APIResultType.REDIRECT },
            { HttpStatusCode.RedirectMethod, APIResultType.REDIRECT_METHOD },
            //{ HttpStatusCode.SeeOther, APIResultType.SEE_OTHER },
            { HttpStatusCode.NotModified, APIResultType.NOT_MODIFIED },
            { HttpStatusCode.UseProxy, APIResultType.USE_PROXY },
            { HttpStatusCode.Unused, APIResultType.UNUSED },
            { HttpStatusCode.RedirectKeepVerb, APIResultType.REDIRECT_KEEP_VERB },
            //{ HttpStatusCode.TemporaryRedirect, APIResultType.TEMPORARY_REDIRECT },
            { HttpStatusCode.PermanentRedirect, APIResultType.PERMANENT_REDIRECT },
            { HttpStatusCode.BadRequest, APIResultType.BAD_REQUEST },
            { HttpStatusCode.Unauthorized, APIResultType.UNAUTHORIZED },
            { HttpStatusCode.PaymentRequired, APIResultType.PAYMENT_REQUIRED },
            { HttpStatusCode.Forbidden, APIResultType.FORBIDDEN },
            { HttpStatusCode.NotFound, APIResultType.NOT_FOUND },
            { HttpStatusCode.MethodNotAllowed, APIResultType.METHOD_NOT_ALLOWED },
            { HttpStatusCode.NotAcceptable, APIResultType.NOT_ACCEPTABLE },
            { HttpStatusCode.ProxyAuthenticationRequired, APIResultType.PROXY_AUTHENTICATION_REQUIRED },
            { HttpStatusCode.RequestTimeout, APIResultType.REQUEST_TIMEOUT },
            { HttpStatusCode.Conflict, APIResultType.CONFLICT },
            { HttpStatusCode.Gone, APIResultType.GONE },
            { HttpStatusCode.LengthRequired, APIResultType.LENGTH_REQUIRED },
            { HttpStatusCode.PreconditionFailed, APIResultType.PRECONDITION_FAILED },
            { HttpStatusCode.RequestEntityTooLarge, APIResultType.REQUEST_ENTITY_TOO_LARGE },
            { HttpStatusCode.RequestUriTooLong, APIResultType.REQUEST_URI_TOO_LONG },
            { HttpStatusCode.UnsupportedMediaType, APIResultType.UNSUPPORTED_MEDIA_TYPE },
            { HttpStatusCode.RequestedRangeNotSatisfiable, APIResultType.REQUESTED_RANGE_NOT_SATISFIABLE },
            { HttpStatusCode.ExpectationFailed, APIResultType.EXPECTATION_FAILED },
            { HttpStatusCode.MisdirectedRequest, APIResultType.MISDIRECTED_REQUEST },
            { HttpStatusCode.UnprocessableEntity, APIResultType.UNPROCESSABLE_ENTITY },
            //{ HttpStatusCode.UnprocessableContent, APIResultType.UNPROCESSABLE_CONTENT },
            { HttpStatusCode.Locked, APIResultType.LOCKED },
            { HttpStatusCode.FailedDependency, APIResultType.FAILED_DEPENDENCY },
            { HttpStatusCode.UpgradeRequired, APIResultType.UPGRADE_REQUIRED },
            { HttpStatusCode.PreconditionRequired, APIResultType.PRECONDITION_REQUIRED },
            { HttpStatusCode.TooManyRequests, APIResultType.TOO_MANY_REQUESTS },
            { HttpStatusCode.RequestHeaderFieldsTooLarge, APIResultType.REQUEST_HEADER_FIELDS_TOO_LARGE },
            { HttpStatusCode.UnavailableForLegalReasons, APIResultType.UNAVAILABLE_FOR_LEGAL_REASONS },
            { HttpStatusCode.InternalServerError, APIResultType.INTERNAL_SERVER_ERROR },
            { HttpStatusCode.NotImplemented, APIResultType.NOT_IMPLEMENTED },
            { HttpStatusCode.BadGateway, APIResultType.BAD_GATEWAY },
            { HttpStatusCode.ServiceUnavailable, APIResultType.SERVICE_UNAVAILABLE },
            { HttpStatusCode.GatewayTimeout, APIResultType.GATEWAY_TIMEOUT },
            { HttpStatusCode.HttpVersionNotSupported, APIResultType.HTTP_VERSION_NOT_SUPPORTED },
            { HttpStatusCode.VariantAlsoNegotiates, APIResultType.VARIANT_ALSO_NEGOTIATES },
            { HttpStatusCode.InsufficientStorage, APIResultType.INSUFFICIENT_STORAGE },
            { HttpStatusCode.LoopDetected, APIResultType.LOOP_DETECTED },
            { HttpStatusCode.NotExtended, APIResultType.NOT_EXTENDED },
            { HttpStatusCode.NetworkAuthenticationRequired, APIResultType.NETWORK_AUTHENTICATION_REQUIRED },
        };

        return statusCodeMapping.TryGetValue((HttpStatusCode)code, out var result) ? result : APIResultType.HTTP_ERROR;
    }

    /// <summary>
    /// HttpRequestError -> APIResultType 코드로 변환
    /// </summary>
    /// <param name="code">HttpRequestError code</param>
    /// <returns>APIResultType code</returns>
    public static APIResultType GetResultByHttpRequestErrorCode(HttpRequestError? code)
    {
        return code switch
        {
            HttpRequestError.Unknown => APIResultType.UNKNOWN,
            HttpRequestError.NameResolutionError => APIResultType.NAME_RESOLUTION_ERROR,
            HttpRequestError.ConnectionError => APIResultType.CONNECTION_ERROR,
            HttpRequestError.SecureConnectionError => APIResultType.SECURE_CONNECTION_ERROR,
            HttpRequestError.HttpProtocolError => APIResultType.HTTP_PROTOCOL_ERROR,
            HttpRequestError.ExtendedConnectNotSupported => APIResultType.EXTENDED_CONNECTION_NOT_SUPPORTED,
            HttpRequestError.VersionNegotiationError => APIResultType.VERSION_NEGOTATION_ERROR,
            HttpRequestError.UserAuthenticationError => APIResultType.USER_AUTHENTICATION_ERROR,
            HttpRequestError.ProxyTunnelError => APIResultType.PROXY_TUNNEL_ERROR,
            HttpRequestError.InvalidResponse => APIResultType.INVALID_RESPONSE,
            HttpRequestError.ResponseEnded => APIResultType.RESPONSE_ENDED,
            HttpRequestError.ConfigurationLimitExceeded => APIResultType.CONFIGURATION_LIMIT_EXCEEDED,
            // 기타 오류: 상태 코드
            _ => APIResultType.HTTP_REQUEST_ERROR
        };
    }
}