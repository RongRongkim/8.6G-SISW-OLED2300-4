@echo off
echo [v2] Build-LadderTextExtractor
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run-LadderTextExtractor.ps1" -Mode Build
echo.
pause
