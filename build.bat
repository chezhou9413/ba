@echo off
chcp 65001 > nul
setlocal

REM 编译 RimWorld 1.7 主库与瞄准显示组件。
set "CONFIGURATION=%~1"
if "%CONFIGURATION%"=="" set "CONFIGURATION=Release"
set "BUILD_SCRIPT=%~dp0bluearchive-newcentury\Tools\Build\Build-1.7.ps1"
if not exist "%BUILD_SCRIPT%" (
    echo 未找到构建脚本: %BUILD_SCRIPT%
    exit /b 1
)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%BUILD_SCRIPT%" -Configuration "%CONFIGURATION%"
exit /b %ERRORLEVEL%
