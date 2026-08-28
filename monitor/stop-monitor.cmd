@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

set "PORT=7003"
if exist ".env" (
    for /f "usebackq eol=# tokens=1,* delims==" %%A in (".env") do (
        if /i "%%A"=="MONITOR_PORT" if not "%%B"=="" set "PORT=%%B"
    )
)

set "PID="
for /f "tokens=5" %%P in ('netstat -ano ^| findstr /R /C:":%PORT% .*LISTENING"') do (
    set "PID=%%P"
)

if not defined PID (
    echo [stop] Nothing listening on port %PORT% - monitor is not running.
    exit /b 0
)

echo [stop] Stopping monitor (PID !PID!, port %PORT%)
taskkill /PID !PID! /F >nul
if errorlevel 1 (
    echo [error] Failed to stop PID !PID! - it may need to be closed manually ^(Task Manager^).
    exit /b 1
)

echo [done] Monitor stopped.
endlocal
