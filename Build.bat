@echo off

set rhubarb_proj_path=./RhubarbEasyBuild/RhubarbEasyBuild.csproj

:: check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ".NET SDK not found. Please download and install .NET 7 SDK."
    pause
    exit /b
)


dotnet --list-runtimes | findstr /i "Microsoft.NETCore.App 7.\." >nul 2>&1
if %errorlevel% neq 0 (
    echo ".NET 7 sdk is required. Please download and install."
    pause
    exit /b
)

:: run the project
dotnet run --project %rhubarb_proj_path%