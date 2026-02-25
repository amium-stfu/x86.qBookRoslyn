@echo off
setlocal enabledelayedexpansion



rem Ins Verzeichnis der Batch wechseln (Projektroot)
cd /d "%~dp0"

echo === Build VS Code Extension ===

echo.
echo === Install dependencies ===
call npm install
if errorlevel 1 goto :error

echo.
echo === Compile TypeScript ===
call npm run compile
if errorlevel 1 goto :error

echo.
echo === Bump extension version ===
call npm run version:bump
if errorlevel 1 goto :error

echo.
echo === Package VS Code extension (VSIX) ===
rem vsce ueber npx ausfuehren (kein globales Install nötig)
call npx vsce package
if errorlevel 1 goto :error

rem Setup-Ordner neben der Batch anlegen
set "SETUP_DIR=%~dp0Setup"
if not exist "%SETUP_DIR%" (
    echo.
    echo === Create Setup directory ===
    mkdir "%SETUP_DIR%"
    if errorlevel 1 goto :error
)

rem Neueste VSIX-Datei im aktuellen Ordner ermitteln
set "VSIX_FILE="
for /f "delims=" %%F in ('dir /b /a:-d /o:-d "*.vsix" 2^>nul') do (
    set "VSIX_FILE=%%F"
    goto :have_vsix
)

echo.
echo *** Keine VSIX-Datei gefunden. ***
goto :done

:have_vsix
echo.
echo === Copy VSIX to Setup ===
copy /y "%VSIX_FILE%" "%SETUP_DIR%\%VSIX_FILE%" >nul
if errorlevel 1 goto :error

echo.
echo *** Fertig. VSIX liegt in: "%SETUP_DIR%\%VSIX_FILE%" ***
goto :done

:error
echo.
echo *** Fehler beim Bauen / Packen der Erweiterung ***

:done
echo.
echo Taste druecken zum Schliessen ...
pause >nul

endlocal
exit /b 0