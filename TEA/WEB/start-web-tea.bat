@echo off
chcp 866 >nul
setlocal
cd /d "%~dp0"

echo === Web TEA: start ===
echo Folder: %CD%
echo.

call "%~dp0check-node.bat" nopause
if errorlevel 1 (
    echo.
    echo Start cancelled. Install Node.js first, then run check-node.bat again.
    echo.
    pause
    exit /b 1
)

echo.

if not exist "package.json" (
    echo [ERROR] package.json not found. Run this script from TEA\WEB
    pause
    exit /b 1
)

if not exist "node_modules\" (
    echo First run: installing dependencies ^(npm install^)...
    echo.
    call npm install
    if errorlevel 1 (
        echo.
        echo [ERROR] npm install failed. Check internet and run the script again.
        pause
        exit /b 1
    )
    echo.
)

echo Starting dev server. Keep this window open while you use the editor.
echo Stop server: Ctrl+C
echo Browser should open at http://localhost:5173/
echo.
call npm run dev -- --open

echo.
echo Server stopped.
pause
exit /b 0
