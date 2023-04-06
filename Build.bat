@echo off

set rhubarb_proj_path=./RhubarbEasyBuild/RhubarbEasyBuild.csproj

:: check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ".NET runtime not found. Please download and install .NET."
    pause
    exit /b
)

:: check if .NET 6 and 7 runtimes are installed
dotnet --list-runtimes | findstr /i "Microsoft.NETCore.App 7.\." >nul 2>&1
if %errorlevel% neq 0 (
    echo "Both .NET 6 runtime and 7 sdk are required. Please download and install them."
    pause
    exit /b
)

:: run the project
dotnet run --project %rhubarb_proj_path%