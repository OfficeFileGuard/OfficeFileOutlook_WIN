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


REM ── FASE 2: build dell'installer con Inno Setup (nessuna firma automatica, lo .iss non ha SignTool=) ──

echo Building installer with Inno Setup...
%ISCC% "%~dp0setup.iss"
if errorlevel 1 goto :error


REM ── FASE 3: firma il setup.exe risultante─

echo Signing final installer...
%SIGNTOOL% sign /n %CERT_NAME% /fd SHA256 /tr %TS_URL% /td SHA256 "%BASE%\installer\output\OfficeFileGuard_Setup_v2.0.0.exe"
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