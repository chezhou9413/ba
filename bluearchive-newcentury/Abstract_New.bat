@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

:: 脚本职责：按目录模块选择性汇总 bluearchive-newcentury 的 C# 和 XML 文件。
set "output_file=combined_output_selected.txt"
set "defs_features_path=1.6\Defs"
set "source_features_path=Source\ClassLibrary1"
set "extra_module_path=Common"

:: 忽略职责：跳过构建缓存、IDE 缓存、二进制资源和大体积输出文件。
set "global_ignore_list=.vs .idea bin obj Assemblies AssetBundles Textures Sounds combined_output_all.txt combined_output_selected.txt"

if exist "%output_file%" del "%output_file%"

cls
echo.
echo =======================================================
echo  选择要打包的功能模块
echo =======================================================
echo.
set "feature_count=0"
set "all_features="

:: 扫描职责：从 Defs、Source 和兼容目录中构建可选模块列表。
for /d %%D in ("%defs_features_path%\*", "%source_features_path%\*", "%extra_module_path%\*") do (
    set "folder_name=%%~nD"
    echo !all_features! | findstr /I /C:" %%~nD " > nul
    if errorlevel 1 (
        set /a feature_count+=1
        echo   [!feature_count!] %%~nD
        set "feature_folders[!feature_count!]=%%~nD"
        set "all_features=!all_features! %%~nD "
    )
)

echo.
echo -------------------------------------------------------
echo.
echo   (a) 全部打包
echo   (n) 全部跳过上方模块目录
echo.
echo 请输入要打包的模块编号，多个用空格隔开，或输入 a/n:
set /p "user_choice="

set "excluded_feature_paths="
if /i "%user_choice%"=="a" (
    echo.
    echo 选择: 全部打包。
) else if /i "%user_choice%"=="n" (
    echo.
    echo 选择: 全部跳过模块目录。
    for %%F in (!all_features!) do (
        set "excluded_feature_paths=!excluded_feature_paths! \%%F\ "
    )
) else (
    set "selected_features= "
    for %%C in (%user_choice%) do (
        set "selected_features=!selected_features!!feature_folders[%%C]! "
    )
    for %%F in (!all_features!) do (
        echo !selected_features! | findstr /I /C:" %%F " > nul
        if errorlevel 1 (
            set "excluded_feature_paths=!excluded_feature_paths! \%%F\ "
        )
    )
    echo.
    echo 将跳过包含以下路径的模块: !excluded_feature_paths!
)

echo.
echo 正在汇总文件...
echo.
(
    for /R . %%F in (*.cs *.xml) do (
        set "file_path=%%F"
        set "should_exclude=false"
        for %%G in (!global_ignore_list!) do (
            if not "!file_path:%%G=!" == "!file_path!" set "should_exclude=true"
        )
        if defined excluded_feature_paths (
            for %%E in (!excluded_feature_paths!) do (
                if not "!file_path:%%E=!" == "!file_path!" set "should_exclude=true"
            )
        )
        if "!should_exclude!"=="false" (
            echo =
            echo File: !file_path!
            echo =
            echo.
            type "!file_path!"
            echo.
            echo.
        )
    )
) > "%output_file%"

cls
echo.
echo =======================================================
echo  打包完成!
echo =======================================================
echo.
echo 已全局忽略列表: !global_ignore_list!
echo 已跳过的模块路径: !excluded_feature_paths!
echo.
echo 结果已保存到: %output_file%
echo.
pause