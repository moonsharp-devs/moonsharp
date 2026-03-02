@echo off
setlocal

set "ROOT=%~dp0.."

call :sync_cs_tree "%ROOT%\MoonSharp.Interpreter" "%~dp0MoonSharp\Assets\Plugins\MoonSharp\Interpreter" "MoonSharp interpreter sources"
if errorlevel 1 exit /b 1

call :sync_cs_tree "%ROOT%\MoonSharp.VsCodeDebugger" "%~dp0MoonSharp\Assets\Plugins\MoonSharp\Debugger" "MoonSharp debugger sources"
if errorlevel 1 exit /b 1

call :sync_cs_tree "%ROOT%\MoonSharp.Interpreter.Tests" "%~dp0MoonSharp\Assets\Tests" "MoonSharp test sources"
if errorlevel 1 exit /b 1

call :copy_filtered "%ROOT%\MoonSharp.Interpreter.Tests\bin\Release\net45" "%~dp0UnityTestBed\Assets\Plugins" "UnityTestBed"
if errorlevel 1 exit /b 1

echo Done.
exit /b 0

:sync_cs_tree
set "SOURCE=%~1"
set "DEST=%~2"
set "LABEL=%~3"

if not exist "%SOURCE%" (
    echo %LABEL%: source directory not found: %SOURCE%
    exit /b 1
)

if not exist "%DEST%" mkdir "%DEST%"

rem Remove only mirrored C# files; keep existing Unity .meta GUID files.
for /R "%DEST%" %%F in (*.cs) do (
    del /F /Q "%%F" >nul
)

robocopy "%SOURCE%" "%DEST%" *.cs /E /XD obj /XF *.csproj >nul
set "RC=%ERRORLEVEL%"
if %RC% GEQ 8 (
    echo %LABEL%: robocopy failed with exit code %RC%.
    exit /b 1
)

echo %LABEL%: synced.
exit /b 0

:copy_filtered
set "SOURCE=%~1"
set "DEST=%~2"
set "LABEL=%~3"

if not exist "%SOURCE%" (
    echo %LABEL%: source directory not found: %SOURCE%
    exit /b 1
)

if not exist "%DEST%" mkdir "%DEST%"

set "COUNT=0"
for %%F in ("%SOURCE%\*.dll") do (
    if /I not "%%~nxF"=="nunit.framework.dll" (
        copy /Y "%%F" "%DEST%\" >nul
        set /a COUNT+=1
    )
)
for %%F in ("%SOURCE%\*.pdb") do (
    copy /Y "%%F" "%DEST%\" >nul
    set /a COUNT+=1
)
for %%F in ("%SOURCE%\*.xml") do (
    copy /Y "%%F" "%DEST%\" >nul
    set /a COUNT+=1
)

if "%COUNT%"=="0" (
    echo %LABEL%: no matching files found in %SOURCE%
    exit /b 1
)

echo %LABEL%: copied %COUNT% file(s).
exit /b 0
