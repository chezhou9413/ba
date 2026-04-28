@echo off
chcp 65001 > nul

:: 脚本职责：递归汇总当前 Mod 目录下所有 C# 和 XML 文件，便于快速交给模型或人工审阅。
set "output_file=combined_output_all.txt"

if exist "%output_file%" del "%output_file%"

echo 正在递归汇总当前目录及所有子目录下的 .cs 和 .xml 文件，请稍候...

:: 汇总职责：保留文件路径和正文，方便定位定义和源码。
(for /R . %%F in (*.cs *.xml) do (
    echo =
    echo File: %%F
    echo =
    echo.
    type "%%F"
    echo.
    echo.
)) > "%output_file%"

echo.
echo 完成！所有 .cs 和 .xml 文件已汇总到 %output_file%
pause