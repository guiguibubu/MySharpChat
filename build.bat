@echo off

setlocal
SET THIS_FILE_FOLDER=%~dp0
dotnet build %THIS_FILE_FOLDER%\MySharpChat.sln %*
endlocal