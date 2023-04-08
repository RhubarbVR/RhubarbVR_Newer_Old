#!/bin/bash

rhubarb_proj_path="./RhubarbEasyBuild/RhubarbEasyBuild.csproj"

# check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo ".NET sdk not found. Please download and install .NET 7 sdk."
    exit 1
fi


if ! dotnet --list-runtimes | grep -q -E "Microsoft\.NETCore\.App 7\."
then
    echo ".NET 7 sdk is required. Please download and install."
    exit 1
fi

# run the project
dotnet run --project $rhubarb_proj_path
