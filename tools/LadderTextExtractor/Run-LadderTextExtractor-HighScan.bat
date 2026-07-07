@echo off
echo [v2] Run-LadderTextExtractor (HighScan)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run-LadderTextExtractor.ps1" -Mode HighScan
echo.
pause
