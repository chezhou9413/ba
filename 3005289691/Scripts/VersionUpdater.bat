@echo off

::Set variables
set "DashLine=----------"

::Set custom title
title RimWorld Together - Version Updater

::Set mod folder path
echo %DashLine%
echo - Mods folder located at "%~1"
echo %DashLine%
echo.

::Wait for RimWorld safe close
echo %DashLine%
echo - Waiting for RimWorld to safely close...
echo %DashLine%
timeout /t 5

::Replace with new installation
echo.
echo %DashLine%
echo - Installing new version...
rmdir /s /q "%~1/3005289691"
move "%~1/3005289691-Temp" "%~1/3005289691"
echo %DashLine%

::Wait at end
echo.
echo %DashLine%
echo - Operation finished...
echo %DashLine%

::End pause
echo.
pause

::Remove leftover files
del "%~dp0/VersionUpdater.bat"