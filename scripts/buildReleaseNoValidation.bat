@echo off

setlocal
SET THIS_FILE_FOLDER=%~dp0
SET MYSHARPCHAT_SKIP_TEST=True
call %THIS_FILE_FOLDER%\buildRelease.bat %*
endlocal
