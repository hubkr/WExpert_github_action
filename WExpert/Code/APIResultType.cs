namespace WExpert.Code;

public enum APIResultType
{
    /* API Common Error Area */
    NONE = -2,
    SERVER_ERROR = -1,

    /* HTTP Request Errors */
    // 제네릭 또는 알 수 없는 오류가 발생했습니다.
    UNKNOWN = 0,
    // DNS 이름 확인에 실패했습니다.
    NAME_RESOLUTION_ERROR = 1,
    // 원격 엔드포인트에 연결하는 동안 전송 수준 오류가 발생했습니다.
    CONNECTION_ERROR = 2,
    // TLS 핸드셰이크 중에 오류가 발생했습니다.
    SECURE_CONNECTION_ERROR = 3,
    // HTTP/2 또는 HTTP/3 프로토콜 오류가 발생했습니다.
    HTTP_PROTOCOL_ERROR = 4,
    // HTTP/2를 통한 WebSockets용 확장 CONNECT는 피어에서 지원되지 않습니다.
    EXTENDED_CONNECTION_NOT_SUPPORTED = 5,
    // 요청된 HTTP 버전을 협상할 수 없습니다.
    VERSION_NEGOTATION_ERROR = 6,
    // 인증이 실패한 경우.
    USER_AUTHENTICATION_ERROR = 7,
    // 프록시 터널에 대한 연결을 설정하는 동안 오류가 발생했습니다.
    PROXY_TUNNEL_ERROR = 8,
    // 잘못되었거나 잘못된 응답이 수신되었습니다.
    INVALID_RESPONSE = 9,
    // 응답은 조기에 종료되었습니다.
    RESPONSE_ENDED = 10,
    // 응답이 또는 MaxResponseHeadersLength와 같이 MaxResponseContentBufferSize 미리 구성된 제한을 초과했습니다.
    CONFIGURATION_LIMIT_EXCEEDED = 11,

    /* HTTP Errors - HTTP Request error 이 Unknown(0) 인 경우 발생 */
    CONTINUE = 100,
    SWITCHING_PROTOCOLS = 101,
    PROCESSING = 102,
    EARLY_HINTS = 103,
    SUCCESS = 200, // OK = 200
    CREATED = 201,
    ACCEPTED = 202,
    NON_AUTHORITATIVE_INFORMATION = 203,
    NO_CONTENT = 204,
    RESET_CONTENT = 205,
    PARTIAL_CONTENT = 206,
    MULTI_STATUS = 207,
    ALREADY_REPORTED = 208,
    IM_USED = 226,
    AMBIGUOUS = 300,
    //MULTIPLE_CHOICES = 300,
    MOVED = 301,
    //MOVED_PERMANENTLY = 301,
    FOUND = 302,
    //REDIRECT = 302,
    REDIRECT_METHOD = 303,
    //SEE_OTHER = 303,
    NOT_MODIFIED = 304,
    USE_PROXY = 305,
    UNUSED = 306,
    REDIRECT_KEEP_VERB = 307,
    //TEMPORARY_REDIRECT = 307,
    PERMANENT_REDIRECT = 308,
    BAD_REQUEST = 400,
    UNAUTHORIZED = 401,
    PAYMENT_REQUIRED = 402,
    FORBIDDEN = 403,
    NOT_FOUND = 404,
    METHOD_NOT_ALLOWED = 405,
    NOT_ACCEPTABLE = 406,
    PROXY_AUTHENTICATION_REQUIRED = 407,
    REQUEST_TIMEOUT = 408,
    CONFLICT = 409,
    GONE = 410,
    LENGTH_REQUIRED = 411,
    PRECONDITION_FAILED = 412,
    REQUEST_ENTITY_TOO_LARGE = 413,
    REQUEST_URI_TOO_LONG = 414,
    UNSUPPORTED_MEDIA_TYPE = 415,
    REQUESTED_RANGE_NOT_SATISFIABLE = 416,
    EXPECTATION_FAILED = 417,
    MISDIRECTED_REQUEST = 421,
    UNPROCESSABLE_ENTITY = 422,
    //UNPROCESSABLE_CONTENT = 422,
    LOCKED = 423,
    FAILED_DEPENDENCY = 424,
    UPGRADE_REQUIRED = 426,
    PRECONDITION_REQUIRED = 428,
    TOO_MANY_REQUESTS = 429,
    REQUEST_HEADER_FIELDS_TOO_LARGE = 431,
    UNAVAILABLE_FOR_LEGAL_REASONS = 451,
    INTERNAL_SERVER_ERROR = 500,
    NOT_IMPLEMENTED = 501,
    BAD_GATEWAY = 502,
    SERVICE_UNAVAILABLE = 503,
    GATEWAY_TIMEOUT = 504,
    HTTP_VERSION_NOT_SUPPORTED = 505,
    VARIANT_ALSO_NEGOTIATES = 506,
    INSUFFICIENT_STORAGE = 507,
    LOOP_DETECTED = 508,
    NOT_EXTENDED = 510,
    NETWORK_AUTHENTICATION_REQUIRED = 511,

    /* WExpert Server Error - 서버 자체 정의 오류 */
    // ACCOUNT_EXPIRED = 1001,

    /* Client 에서 정의 사용 */
    // 이미 요청 중
    ALREADY_REQUESTED = 10000,
    // 최대 요청 개수 초과(Client 에서 정의 사용)
    EXCEEDING_MAX_REQUEST = 10001,
    // HTTP 오류..HTTP Status Code (100~500 번대 코드 사용)
    HTTP_ERROR = 10002,
    // HTTP Request 오류
    HTTP_REQUEST_ERROR = 10004,
    //network 오류
    NETWORK_ERROR = 10005,
    //내부 오류
    INTERNAL_ERROR = 10006,
    // 네트워크 상태가 원할하지 않을 때 발생
    NETWORK_STATUS_ERROR = 10007
}
