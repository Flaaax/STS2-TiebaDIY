@echo off
setlocal

set "LATEST_VERSION=0.110.0"
pushd "%~dp0"

if "%~1"=="" goto build_latest
if /I "%~1"=="all" goto build_all
if /I "%~1"=="latest" goto build_latest
if /I "%~1"=="help" goto help
if /I "%~1"=="-h" goto help
if /I "%~1"=="/?" goto help
goto build_specific

:build_latest
dotnet build -t:CurrentVersion -c "Release %LATEST_VERSION%"
set "RESULT=%ERRORLEVEL%"
goto end

:build_all
dotnet build -t:AllVersion -c "Release %LATEST_VERSION%"
set "RESULT=%ERRORLEVEL%"
goto end

:build_specific
set "TARGET_VERSION=%~1"
dotnet build -t:CurrentVersion -c "Release %TARGET_VERSION%"
set "RESULT=%ERRORLEVEL%"
goto end

:help
echo Usage: build [latest^|all^|0.107.1^|0.110.0]
set "RESULT=0"
goto end

:end
popd
exit /b %RESULT%
