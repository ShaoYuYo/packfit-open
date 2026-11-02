@echo off
setlocal
cd /d "%~dp0"
echo ============================================
echo  PackFit Engineering - Start and diagnostics
echo ============================================
echo.
if not exist "PackFit.Engineering.exe" (
  echo ERROR: Missing EXE. Extract the entire WinUI ZIP first.
  pause
  exit /b 2
)
echo Starting PackFit from this extracted folder...
echo A brief Windows firewall or security dialog may be shown by Windows.
echo.
"PackFit.Engineering.exe"
set "RESULT=%ERRORLEVEL%"
echo.
echo PackFit returned exit code: %RESULT%
echo Private log path:
echo %LOCALAPPDATA%\PackFitOpen\Logs\startup.log
echo.
echo To collect a local startup report, open PowerShell here and run:
echo    .\Diagnostics\Diagnose-PackFit.ps1
echo.
echo Never bypass company security rules. Review and redact logs before sharing.
pause
exit /b %RESULT%
