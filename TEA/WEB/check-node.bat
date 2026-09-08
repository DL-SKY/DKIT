@echo off
chcp 866 >nul
setlocal
cd /d "%~dp0"

echo === Web TEA: Node.js check ===
echo.

where node >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Node.js not found in PATH.
    echo.
    echo Need Node.js 20 LTS or newer.
    echo Install: https://nodejs.org/  ^(download LTS, keep Add to PATH checked^)
    echo Or in PowerShell:
    echo   winget install OpenJS.NodeJS.LTS --accept-package-agreements --accept-source-agreements
    echo.
    echo After install close all terminals and Cursor, then run this script again.
    goto :fail
)

where npm >nul 2>&1
if errorlevel 1 (
    echo [ERROR] npm not found.
    echo Reinstall Node.js LTS from https://nodejs.org/ and keep Add to PATH.
    echo Then close the terminal / Cursor and check again.
    goto :fail
)

echo node:
call node -v
echo npm:
call npm -v
echo.

for /f "tokens=1 delims=v." %%A in ('node -v') do set "NODE_MAJOR=%%A"
if not defined NODE_MAJOR (
    echo [WARN] Could not parse node version. Continue only if node -v shows 20+.
    goto :ok
)

if %NODE_MAJOR% LSS 20 (
    echo [WARN] Need Node.js 20+. Current version is too old.
    echo Update LTS: https://nodejs.org/
    goto :fail
)

echo Node.js OK. You can run start-web-tea.bat
goto :ok

:ok
if /i not "%~1"=="nopause" pause
exit /b 0

:fail
if /i not "%~1"=="nopause" pause
exit /b 1
