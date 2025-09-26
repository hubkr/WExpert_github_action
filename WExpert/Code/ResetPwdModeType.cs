namespace WExpert.Code;

public enum ResetPwdModeType
{
    INPUT_EMAIL, // 이메일 입력 단계
    INPUT_OTP,   // 이메일로 전달받은 인증코드 입력 단계
    INPUT_PWD    // 비밀번호 입력 단계
}