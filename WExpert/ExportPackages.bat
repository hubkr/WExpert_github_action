@echo off

REM #############################################
REM WExpert.csproj ������ �ִ� �������� ������ �ּ���.
REM #############################################

REM ���� ������Ʈ���� ������� ��Ű�� ���(���� ���� ����) ���� ��ɾ�
echo ��Ű�� ��� ����..
dotnet list WExpert.csproj package --include-transitive > WExport_Packages.txt
echo ��Ű�� ��� ���� �Ϸ�!

REM ���� ������Ʈ���� ������� ��Ű�� ���̼��� ���� ���� ��ɾ�
echo ���̼��� ��� ����..
dotnet-project-licenses -i WExpert.csproj --include-transitive --output WExport_Licenses.txt
echo ���̼��� ��� ���� �Ϸ�

echo WExport_Packages.txt, WExport_Licenses.txt ���Ϸ� ����Ǿ����ϴ�.
pause