@echo off
REM ================================================================
REM NgLib - Build and Package Launcher
REM Executes the PowerShell build script
REM ================================================================

powershell.exe -Version 5.1 -ExecutionPolicy Bypass -File "%~dp0RunBuilder.ps1"

if errorlevel 1 (
    echo.
    echo Build script failed!
    pause
    exit /b 1
)

exit /b 0
