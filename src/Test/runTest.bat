echo off
setlocal 

SET NUNIT_CONSOLE_PATH=%1
SET ASSEMBLY_PATH=%2
SET WORK_DIRECTORY=%3

IF NOT DEFINED NUNIT_CONSOLE_PATH goto :failParameters
IF NOT DEFINED ASSEMBLY_PATH goto :failParameters
IF NOT DEFINED WORK_DIRECTORY goto :failParameters

IF "%MYSHARPCHAT_SKIP_TEST%" NEQ "True" (
	call %NUNIT_CONSOLE_PATH% %ASSEMBLY_PATH% --work %WORK_DIRECTORY%
) ELSE (
	echo MYSHARPCHAT_SKIP_TEST set to "%MYSHARPCHAT_SKIP_TEST%". Test will be skip
)

endlocal

goto :eof

:failParameters
echo 3 parameters are needed
echo 1 : full path to nunit console
echo 2 : full path to assembly to test
echo 3 : full path to output directory for nunit files
exit /B 1