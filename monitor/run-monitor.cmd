@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

if not exist ".env" (
    if exist ".env.sample" (
        echo [setup] .env not found - creating it from .env.sample
        copy /y ".env.sample" ".env" >nul
    )
)

rem Prefer "py -3.11": the mingw64 python bundled with Git Bash has no
rem prebuilt wheel for this project's compiled deps (pydantic-core).
set "PYLAUNCH="
py -3.11 -c "exit()" >nul 2>nul
if not errorlevel 1 (
    set "PYLAUNCH=py -3.11"
) else (
    where python >nul 2>nul
    if not errorlevel 1 (
        set "PYLAUNCH=python"
    )
)
if not defined PYLAUNCH (
    echo [error] No usable Python found. Install Python 3.11 and retry.
    echo         Note: Git Bash's bundled mingw64 python lacks prebuilt wheels for
    echo         this project's dependencies - use a python.org/Windows Store build.
    exit /b 1
)

if not exist ".venv" (
    echo [setup] Creating virtual environment in .venv
    %PYLAUNCH% -m venv .venv
)

rem Call the venv's python.exe directly instead of ".venv\Scripts\activate.bat":
rem that script's auto-generated "set VIRTUAL_ENV=<path>" line is unquoted, so
rem it breaks cmd.exe's parser on any path containing "&" (as this one does).
set "VENV_PY=.venv\Scripts\python.exe"

echo [setup] Installing dependencies
"%VENV_PY%" -m pip install -q --upgrade pip
"%VENV_PY%" -m pip install -q -r requirements.txt

echo [env] Loading .env
for /f "usebackq eol=# tokens=1,* delims==" %%A in (".env") do (
    set "%%A=%%B"
)

echo [run] Starting monitor.py  (Ctrl+C to stop)
"%VENV_PY%" monitor.py
endlocal
