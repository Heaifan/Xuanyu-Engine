
@echo off
setlocal

cd /d "%~dp0"

set "EDITOR_DLL=src\XuanYu.Editor\bin\Release\net10.0\XuanYu.Editor.dll"

if not exist "%EDITOR_DLL%" (
    echo [XYE] Editor Release DLL not found.
    echo [XYE] Run the Release build first.
    exit /b 1
)

echo [XYE] Starting XuanYu Engine Editor...
dotnet "%EDITOR_DLL%"

set "EXIT_CODE=%ERRORLEVEL%"
echo [XYE] Editor exited with code %EXIT_CODE%.

exit /b %EXIT_CODE%
