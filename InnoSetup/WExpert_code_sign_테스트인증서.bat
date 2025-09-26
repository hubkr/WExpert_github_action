REM signtool sign /f W.AI.cer /p test /fd sha256 /tr http://timestamp.sectigo.com /td sha256 Output/WExpertSetup.exe


@echo off
setlocal

REM 인증서 파일(.pfx) 경로와 비밀번호
set PFX_FILE=W.AI_TestCert.pfx
set PFX_PASSWORD=1234
REM set PFX_FILE=W.AI.cer
REM set PFX_PASSWORD=0ihh54HL8t15gs&M

REM 타임스탬프 서버
set TIMESTAMP_SERVER=http://timestamp.sectigo.com

REM 서명할 파일 경로
set TARGET_FILE=Output\WExpert*.exe

REM signtool.exe가 PATH에 없다면 전체 경로로 지정 (예시)
REM set SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe"

REM 서명 실행
signtool sign /f %PFX_FILE% /p %PFX_PASSWORD% /fd sha256 /tr %TIMESTAMP_SERVER% /td sha256 %TARGET_FILE%

REM 결과 확인
if %ERRORLEVEL%==0 (
    echo.
    echo 서명이 완료되었습니다.
) else (
    echo.
    echo 오류 발생! 서명에 실패했습니다.
)

pause
endlocal