#!/bin/bash

NUNIT_CONSOLE_PATH=$1
ASSEMBLY_PATH=$2
WORK_DIRECTORY=$3

echo NUNIT_CONSOLE_PATH=$NUNIT_CONSOLE_PATH
echo ASSEMBLY_PATH=$ASSEMBLY_PATH
echo WORK_DIRECTORY=$WORK_DIRECTORY

function failParameters {
	echo 3 parameters are needed
	echo 1 : full path to nunit console
	echo 2 : full path to assembly to test
	echo 3 : full path to output directory for nunit files
	exit 1
}

function isEnvVarDefined {
	envar=$1
	if [[ -z "${envar}" ]]; then
		return 1
	else
		return 0
	fi
}

echo $(isEnvVarDefined NUNIT_CONSOLE_PATH)

if isEnvVarDefined NUNIT_CONSOLE_PATH; then failParameters; fi
if isEnvVarDefined ASSEMBLY_PATH; then failParameters; fi
if isEnvVarDefined WORK_DIRECTORY; then failParameters; fi

if [ ! "$MYSHARPCHAT_SKIP_TEST" = "true" ]; then
	$NUNIT_CONSOLE_PATH $ASSEMBLY_PATH --work $WORK_DIRECTORY
else
	echo MYSHARPCHAT_SKIP_TEST set to "$MYSHARPCHAT_SKIP_TEST". Test will be skip	
fi
