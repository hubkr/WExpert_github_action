REM signtool sign /f W.AI.cer /p test /fd sha256 /tr http://timestamp.sectigo.com /td sha256 Output/WExpertSetup.exe


@echo off
setlocal

REM ������ ����(.pfx) ��ο� ��й�ȣ
set PFX_FILE=W.AI_TestCert.pfx
set PFX_PASSWORD=1234
REM set PFX_FILE=W.AI.cer
REM set PFX_PASSWORD=0ihh54HL8t15gs&M

REM Ÿ�ӽ����� ����
set TIMESTAMP_SERVER=http://timestamp.sectigo.com

REM ������ ���� ���
set TARGET_FILE=Output\WExpert*.exe

REM signtool.exe�� PATH�� ���ٸ� ��ü ��η� ���� (����)
REM set SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe"

REM ���� ����
signtool sign /f %PFX_FILE% /p %PFX_PASSWORD% /fd sha256 /tr %TIMESTAMP_SERVER% /td sha256 %TARGET_FILE%

REM ��� Ȯ��
if %ERRORLEVEL%==0 (
    echo.
    echo ������ �Ϸ�Ǿ����ϴ�.
) else (
    echo.
    echo ���� �߻�! ���� �����߽��ϴ�.
)

pause
endlocal