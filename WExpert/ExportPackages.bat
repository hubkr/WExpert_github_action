@echo off

REM #############################################
REM WExpert.csproj 파일이 있는 폴더에서 실행해 주세요.
REM #############################################

REM 현재 프로젝트에서 사용중인 패키지 목록(의존 관계 포함) 축출 명령어
echo 패키지 목록 축출..
dotnet list WExpert.csproj package --include-transitive > WExport_Packages.txt
echo 패키지 목록 축출 완료!

REM 현재 프로젝트에서 사용중인 패키지 라이선스 정보 축출 명령어
echo 라이선스 목록 축출..
dotnet-project-licenses -i WExpert.csproj --include-transitive --output WExport_Licenses.txt
echo 라이선스 목록 축출 완료

echo WExport_Packages.txt, WExport_Licenses.txt 파일로 저장되었습니다.
pause