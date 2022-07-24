#!/bin/bash

THIS_FILE_FOLDER=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
export MYSHARPCHAT_SKIP_TEST=True
$THIS_FILE_FOLDER/buildRelease.sh %*
