@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

where docker >nul 2>nul
if errorlevel 1 (
    echo [error] Docker was not found on PATH. Install Docker Desktop and retry.
    exit /b 1
)

if not exist ".env" (
    if exist ".env.sample" (
        echo [setup] .env not found - creating it from .env.sample
        copy /y ".env.sample" ".env" >nul
    ) else (
        echo [error] Neither .env nor .env.sample found in %cd%
        exit /b 1
    )
)

echo [build] docker compose build --parallel
docker compose build --parallel
if errorlevel 1 (
    echo [error] docker compose build failed
    exit /b 1
)

echo [done] Images built. Start the stack with: docker compose up -d
endlocal
