# Innosetup 빌드 환경
### 디렉 토리 구성
* /Source/license.txt - 빌드시 Setup 파일 화면에 보여지게 되는 라이선스 정보
* /Source/WindowIcon.ico - Setup exe 파일 생성시 생성 파일 아이콘
* /Source/WExpert - 빌드된 WExpert 바이너리 파일들(Setup 파일 생성시 빌드된 파일들을 복사하여 추가)
* /Output - Setup 파일 생성후 만들어진 Setup 파일이 저장 되는 폴더
* /signtool.exe - 코드 사이닝을 위한 도구 프로그램 
* /Step1 Code Sign_Source Directory.bat - 빌드 파일에 포함되는 모든 파일들에 대한 EV 디지털 인증 수항
* /Step2 Make Setup File for x64.iss - Setup 파일 생성 + 생성된 파일에 대한 EV 디지털 인증 수행 
* /WExpert_code_sign_테스트인증서.bat - 테스트용 인승서 생성 스크립트
* /토큰.txt - Setigo 인증서 사용을 위힌 비밀번호

### 빌드 환경 구성
* 사용 InnoSetup 버전 - 6.4.3 
* InnoSetup 설치 후 Tool > Configure Sign Tools 창 > Add > Name of sign Tool 에 WExpertSignTool 입력 후 > Configure Sign Tools 창 에 > E:\Source\Setup 생성\Build\signtool sign /fd SHA256 /a /tr http://timestamp.sectigo.com /td SHA256 $f 입력 (signtool 경로는 PC 환경에 따라 변경 필요)

### Setup 파일 생성 방법
* WExpert 를 빌드 완료 후 바이너리 파일을 /Source/WExpert 폴더 밑에 복사(WExpert 디렉 토리가 없는 경우 디렉토리 생성)
* 빌드된 Setup 파일이 저장되는 /Output 폴더 위치 확인(없는 경우 Output 추가 필요)
* /Step1 Code Sign_Source Directory.bat 를 실행 하여 Source 디렉토리에 있는 모든 파일을 디지털 서명 수행
* /Step2 Make Setup File for x64.iss 를 실행하여 Setup 파일 생성