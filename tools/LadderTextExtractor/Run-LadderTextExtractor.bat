@echo off
echo [v2] Run-LadderTextExtractor (All)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run-LadderTextExtractor.ps1" -Mode All
echo.
pause
