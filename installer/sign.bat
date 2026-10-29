@echo off
setlocal

set SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
set CERT_NAME="Open Source Developer Michele Cozzolino"
set TS_URL=http://time.certum.pl
set BASE=%~dp0..

REM ── FASE 1: firma i singoli eseguibili/DLL PRIMA che Inno li impacchetti ──

echo Signing CLI...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\src\CrocoOfficeFileReserveToggle.Cli\bin\x64\Release\CrocoOfficeFileReserveToggle.Cli.exe"
if errorlevel 1 goto :error

echo Signing Core.dll...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\src\CrocoOfficeFileReserveToggle.Cli\bin\x64\Release\CrocoOfficeFileGuard.Core.dll"
if errorlevel 1 goto :error

echo Signing Shell Extension...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\CrocoIconOverlayShellExtension\bin\x64\Release\CrocoIconOverlayShellExtension.dll"
if errorlevel 1 goto :error

echo Signing OfficeFileGuard.exe...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\OfficeFileGuard.App\bin\Release\OfficeFileGuard.exe"
if errorlevel 1 goto :error

echo Signing OfficeFileGuard.Whitelist.dll...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\OfficeFileGuard.Whitelist\bin\Release\OfficeFileGuard.Whitelist.dll"
if errorlevel 1 goto :error


REM ── FASE 2: build dell'installer con Inno Setup (nessuna firma automatica, lo .iss non ha SignTool=) ──

echo Building installer with Inno Setup...
%ISCC% "%~dp0setup_whitelist.iss"
if errorlevel 1 goto :error


REM ── FASE 3: firma il setup.exe risultante─

echo Signing final installer...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\installer\output\OfficeFileGuard_Setup.exe"
if errorlevel 1 goto :error


REM ── FASE 4: crea lo zip di distribuzione con la versione nel nome ──

for /f "tokens=3" %%V in ('findstr /b /c:"#define MyAppVersion" "%~dp0setup_whitelist.iss"') do set APP_VERSION=%%~V
if not defined APP_VERSION (
    echo ERRORE: versione non trovata in setup_whitelist.iss
    goto :error
)

echo Creating zip v%APP_VERSION%...
powershell -NoProfile -Command "Compress-Archive -Path '%BASE%\installer\output\OfficeFileGuard_Setup.exe' -DestinationPath '%BASE%\installer\output\OfficeFileGuard_Setup_v%APP_VERSION%.zip' -Force"
if errorlevel 1 goto :error


echo.
echo Signing complete!
goto :end

:error
echo.
echo ERRORE during signing or installe build
exit /b 1

:end
endlocal
pause