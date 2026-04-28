@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

REM 脚本职责：将内层实际 Mod 目录部署到 RimWorld Mods 目录。
set "MOD_FOLDER_NAME=bluearchive-newcentury"
set "TARGET_PATH=E:\huanshijie\1.6\RimWorld\Mods"
set "CURRENT_DIR=%~dp0"
set "SOURCE_MOD_PATH=%CURRENT_DIR%%MOD_FOLDER_NAME%"
set "TARGET_MOD_PATH=%TARGET_PATH%\%MOD_FOLDER_NAME%"

echo ==========================================
echo Mod Deployment Script
echo ==========================================
echo Mod Name: %MOD_FOLDER_NAME%
echo Source Path: %SOURCE_MOD_PATH%
echo Target Path: %TARGET_MOD_PATH%
echo ==========================================
echo.

if not exist "%SOURCE_MOD_PATH%" (
    echo [ERROR] Source mod folder does not exist: %SOURCE_MOD_PATH%
    pause
    exit /b 1
)

if not exist "%SOURCE_MOD_PATH%\About\About.xml" (
    echo [ERROR] About.xml not found in source mod folder.
    pause
    exit /b 1
)

if not exist "%TARGET_PATH%" (
    echo [ERROR] Target path does not exist: %TARGET_PATH%
    pause
    exit /b 1
)

echo [INFO] Starting mod deployment...
if exist "%TARGET_MOD_PATH%" (
    echo [INFO] Removing existing mod: %TARGET_MOD_PATH%
    rd /s /q "%TARGET_MOD_PATH%" >nul 2>&1
    timeout /t 1 /nobreak >nul 2>&1
)

echo [INFO] Copying mod files...
robocopy "%SOURCE_MOD_PATH%" "%TARGET_MOD_PATH%" /e /is /it /r:1 /w:1
set "ROBOCOPY_EXIT=%ERRORLEVEL%"

if %ROBOCOPY_EXIT% GEQ 8 (
    echo [ERROR] Robocopy failed with exit code %ROBOCOPY_EXIT%
    pause
    exit /b %ROBOCOPY_EXIT%
)

if exist "%TARGET_MOD_PATH%\About\About.xml" (
    echo [SUCCESS] Mod deployed to: %TARGET_MOD_PATH%
    exit /b 0
)

echo [ERROR] Deployment finished but About.xml was not found in target.
pause
exit /b 1