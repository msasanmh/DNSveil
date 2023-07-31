mode con cols=100 lines=20
@echo OFF & color 1F
:START
title Check .Net 6 Runtime Installation - MSasanMH
REM SETLOCAL: ensure the lifetime of the environment will end with the termination of the batch.
SETLOCAL EnableExtensions EnableDelayedExpansion

REM =============== Get Current Path:
pushd %~dp0
set CurrentPath=%CD%
popd

REM =============== Get System Architecture:
REG Query "HKLM\Hardware\Description\System\CentralProcessor\0" >NUL 2>&1
IF %ERRORLEVEL% EQU 0 (
REG Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && SET sArchitecture=x86 || SET sArchitecture=x64
) ELSE (
REM Default Architecture if reading registry failed:
SET sArchitecture=x64
)

:Check
IF !sArchitecture! == x64 (
	IF NOT EXIST "%HomeDrive%\Program Files\dotnet\dotnet.exe" GOTO :NotInstalledX64
	IF NOT EXIST "%HomeDrive%\Program Files (x86)\dotnet\dotnet.exe" GOTO :NotInstalledX86
	
	start /b /wait "" "%HomeDrive%\Program Files\dotnet\dotnet.exe" --list-runtimes >"%CurrentPath%\tmp.txt"
	SET CheckNet64=false
	for /f "tokens=*" %%a in ('find /I "Microsoft.WindowsDesktop.App 6" ^< "%CurrentPath%\tmp.txt"') do (
		SET CheckNet64=true
	)
	IF !CheckNet64! == false GOTO :NotInstalledX64
	
	start /b /wait "" "%HomeDrive%\Program Files (x86)\dotnet\dotnet.exe" --list-runtimes >"%CurrentPath%\tmp.txt"
	SET CheckNet86=false
	for /f "tokens=*" %%a in ('find /I "Microsoft.WindowsDesktop.App 6" ^< "%CurrentPath%\tmp.txt"') do (
		SET CheckNet86=true
	)
	IF !CheckNet86! == false GOTO :NotInstalledX86
	
) ELSE IF !sArchitecture! == x86 (
	IF NOT EXIST "%HomeDrive%\Program Files\dotnet\dotnet.exe" GOTO :NotInstalledX86
	
	start /b /wait "" "%HomeDrive%\Program Files\dotnet\dotnet.exe" --list-runtimes >"%CurrentPath%\tmp.txt"
	SET CheckNet=false
	for /f "tokens=*" %%a in ('find /I "Microsoft.WindowsDesktop.App 6" ^< "%CurrentPath%\tmp.txt"') do (
		SET CheckNet=true
	)
	IF !CheckNet! == false GOTO :NotInstalledX86
	
)

:Installed
ECHO.
ECHO All requirements are installed.
ECHO.
GOTO :Exit

:NotInstalledX64
ECHO.
ECHO .NET Desktop Runtime v6 x64 is Not Installed.
ECHO.
GOTO :DownloadNet

:NotInstalledX86
ECHO.
ECHO .NET Desktop Runtime v6 x86 is Not Installed.
ECHO.
GOTO :DownloadNet

:DownloadNet
ECHO Download:
ECHO https://dotnet.microsoft.com/en-us/download/dotnet/6.0
ECHO.
GOTO :Exit

:Exit
IF EXIST "%CurrentPath%\tmp.txt" DEL /F /Q "%CurrentPath%\tmp.txt" >nul 2>&1
EndLocal
Pause
