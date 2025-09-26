@echo off
setlocal enabledelayedexpansion

REM 1. �ʼ� ���� ��
set TARGET_DIR=Source
set TIMESTAMP_SERVER=http://timestamp.sectigo.com
set CERT_DESC=W.AI, Inc

REM 2. ������ Ȯ���� ���
set EXTENSIONS=.exe .dll .ocx .sys .msi .msp .cpl .scr

REM 3. ��ü ���� ���� ����
set FILE_COUNT=0
for %%e in (%EXTENSIONS%) do (
    for /R "%TARGET_DIR%" %%f in (*%%e) do (
        set /a FILE_COUNT+=1
    )
)

if %FILE_COUNT%==0 (
    echo [�˸�] ������ ������ �����ϴ�.
    goto :END
)

REM 4. ���� �� ���� �۾� + ����� ǥ��
set CURRENT=0
for %%e in (%EXTENSIONS%) do (
    echo.
    echo [Ȯ����: %%e] ó�� ����...
    for /R "%TARGET_DIR%" %%f in (*%%e) do (
        set /a CURRENT+=1
        echo.
        echo [�����] !CURRENT! / %FILE_COUNT% : [����] %%f
        signtool sign /fd SHA256 /a /tr %TIMESTAMP_SERVER% /td SHA256 /d "%CERT_DESC%" "%%f"
        
        if !errorlevel! equ 0 (
            echo [����] %%f
            signtool verify /pa "%%f"
            if !errorlevel! neq 0 (
                echo [���] ���� ����: %%f
            )
        ) else (
            echo [����] ���� ����: %%f
        )
    )
)

:END
echo.
echo ===== ��� �۾� �Ϸ� =====
pause