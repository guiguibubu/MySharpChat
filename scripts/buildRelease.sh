#!/bin/bash

THIS_FILE_FOLDER=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
$THIS_FILE_FOLDER/build.sh -c Release $*
