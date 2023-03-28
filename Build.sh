#!/bin/bash

rhubarb_proj_path="./RhubarbEasyBuild/RhubarbEasyBuild.csproj"

# check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo ".NET runtime not found. Please download and install .NET."
    exit 1
fi

# check if .NET 6 and 7 runtimes are installed
if ! dotnet --list-runtimes | grep -q -E "Microsoft\.NETCore\.App 6\.|Microsoft\.NETCore\.App 7\."
then
    echo "Both .NET 6 runtime and 7 sdk are required. Please download and install them."
    exit 1
fi

# run the project
dotnet run --project $rhubarb_proj_path
