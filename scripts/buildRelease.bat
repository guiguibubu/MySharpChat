@echo off

setlocal
SET THIS_FILE_FOLDER=%~dp0
call %THIS_FILE_FOLDER%\build.bat -c Release %*
endlocal
