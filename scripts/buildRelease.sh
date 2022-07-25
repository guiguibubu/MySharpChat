#!/bin/bash

THIS_FILE_FOLDER=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
dotnet build $THIS_FILE_FOLDER/MySharpChat.sln -c Release $*
