using WExpert.Code;

namespace WExpert.Helpers.Http;

public static class ApiRoutes
{
    /* Auth */
    // 로그인
    public static readonly ApiEndpoint LOGIN             = new(RequestMethodType.POST, "v1/auth/login");
    // 토큰 확인
    public static readonly ApiEndpoint CHECK_TOKEN       = new(RequestMethodType.GET,  "v1/auth/token");
    // 비밀번호 분실시 인증번호 전송
    public static readonly ApiEndpoint RECEIVE_OTP_MAIL  = new(RequestMethodType.POST, "v1/auth/verification-mail-forgot-password");
    // 인증번호 검증
    public static readonly ApiEndpoint VERIFICATION_OTP  = new(RequestMethodType.POST, "v1/auth/verify-otp-forgot-password");
    // 비밀 번호 변경(로그인 전)
    public static readonly ApiEndpoint RESET_PASSWORD    = new(RequestMethodType.POST, "v1/auth/reset-password");

    /* Patients information */
    // 환자 등록
    public static readonly ApiEndpoint PATIENT_CREATE = new(RequestMethodType.POST, "v1/patients", true);
    // 환자 목록 리스트
    public static readonly ApiEndpoint PATIENT_READ_ALL = new(RequestMethodType.GET, "v1/patients");
    // 환자 목록 상세
    public static readonly ApiEndpoint PATIENT_READ_ONE = new(RequestMethodType.GET, "v1/patients/{0}");
    // 환자 정보 update
    public static readonly ApiEndpoint PATIENT_UPDATE = new(RequestMethodType.POST, "v1/patients/{0}/update", true);
    // 환자 삭제(한명)
    public static readonly ApiEndpoint PATIENT_DELET = new(RequestMethodType.POST, "v1/patients/{0}/delete");
    // 환자 삭제(복수명)
    public static readonly ApiEndpoint PATIENTS_DELET = new(RequestMethodType.POST, "v1/patients/delete-multiple");

    /* User */
    // 비밀 번호 변경(로그인 후)
    public static readonly ApiEndpoint CHANGE_PASSWORD   = new(RequestMethodType.POST, "v1/user/change-password");
    // 프로 파일
    public static readonly ApiEndpoint PROFILE           = new(RequestMethodType.GET,  "v1/user/profile");
    // 로그 아웃
    public static readonly ApiEndpoint LOGOUT            = new(RequestMethodType.POST, "v1/user/logout");

    /* Consultation */
    // 질문 등록
    public static readonly ApiEndpoint CONSULTATION_QUESTION_CREATE = new(RequestMethodType.POST, "v1/consultation/question");
    // 목록 리스트 가져오기
    public static readonly ApiEndpoint CONSULTATION_READ_ALL = new(RequestMethodType.GET, "v1/consultation?sonography_id={0}");

    /* Analysis */
    // 분석 요청(old api)
    //public static readonly ApiEndpoint ANALYSIS          = new(RequestMethodType.POST, "v1/diagnostics/picture", true);
    // 분석 진행 상태 상태 조회(환자 1건)
    public static readonly ApiEndpoint ANALYSIS_STATUS   = new(RequestMethodType.GET,  "v1/analysis/progress/{0}");
    // 환자 코드 목록으로 환자별 분석 진행 상태 조회(환자 n건)
    public static readonly ApiEndpoint ANALYSIS_STATUS_ARRAY = new(RequestMethodType.POST, "v1/analysis/progress");
    // 분석 결과 확인
    public static readonly ApiEndpoint ANALYSIS_RESULT   = new(RequestMethodType.GET,  "v1/analysis/sonography/{0}");
    // 재 분석 요청
    public static readonly ApiEndpoint ANALYSIS_REACTION = new(RequestMethodType.POST, "v1/analysis/sonography");

    /* 기타 */
    // 최신 버전 존재 확인
    public static readonly ApiEndpoint CHECK_NEW_VERSION = new(RequestMethodType.GET, "v1/client-app/latest-version?currentVersion={0}");
}