@echo off
chcp 65001 > nul
setlocal

REM 脚本职责：在外层开发目录中编译 BANWlLib C# 项目。
set "PROJECT_FILE=%~dp0bluearchive-newcentury\Source\ClassLibrary1\BANWlLib.csproj"
set "PROJECT_DIR=%~dp0bluearchive-newcentury\Source\ClassLibrary1"
set "CONFIGURATION=%~1"
if "%CONFIGURATION%"=="" set "CONFIGURATION=Debug"

if not exist "%PROJECT_FILE%" (
    echo 未找到项目文件: %PROJECT_FILE%
    exit /b 1
)

pushd "%PROJECT_DIR%"
echo 正在编译 BANWlLib (%CONFIGURATION%)...
msbuild "%PROJECT_FILE%" /p:Configuration=%CONFIGURATION% /p:Platform="AnyCPU"
set "BUILD_EXIT_CODE=%ERRORLEVEL%"
popd

if not "%BUILD_EXIT_CODE%"=="0" (
    echo 编译失败，退出码: %BUILD_EXIT_CODE%
    exit /b %BUILD_EXIT_CODE%
)

echo 编译完成。
exit /b 0