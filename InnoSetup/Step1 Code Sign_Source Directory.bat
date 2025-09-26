@echo off
setlocal enabledelayedexpansion

REM 1. 필수 설정 값
set TARGET_DIR=Source
set TIMESTAMP_SERVER=http://timestamp.sectigo.com
set CERT_DESC=W.AI, Inc

REM 2. 서명할 확장자 목록
set EXTENSIONS=.exe .dll .ocx .sys .msi .msp .cpl .scr

REM 3. 전체 파일 개수 세기
set FILE_COUNT=0
for %%e in (%EXTENSIONS%) do (
    for /R "%TARGET_DIR%" %%f in (*%%e) do (
        set /a FILE_COUNT+=1
    )
)

if %FILE_COUNT%==0 (
    echo [알림] 서명할 파일이 없습니다.
    goto :END
)

REM 4. 서명 및 검증 작업 + 진행률 표시
set CURRENT=0
for %%e in (%EXTENSIONS%) do (
    echo.
    echo [확장자: %%e] 처리 시작...
    for /R "%TARGET_DIR%" %%f in (*%%e) do (
        set /a CURRENT+=1
        echo.
        echo [진행률] !CURRENT! / %FILE_COUNT% : [서명] %%f
        signtool sign /fd SHA256 /a /tr %TIMESTAMP_SERVER% /td SHA256 /d "%CERT_DESC%" "%%f"
        
        if !errorlevel! equ 0 (
            echo [검증] %%f
            signtool verify /pa "%%f"
            if !errorlevel! neq 0 (
                echo [경고] 검증 실패: %%f
            )
        ) else (
            echo [오류] 서명 실패: %%f
        )
    )
)

:END
echo.
echo ===== 모든 작업 완료 =====
pause