#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

if ! command -v docker >/dev/null 2>&1; then
    echo "[error] Docker was not found on PATH. Install Docker and retry." >&2
    exit 1
fi

if [ ! -f .env ]; then
    if [ -f .env.sample ]; then
        echo "[setup] .env not found - creating it from .env.sample"
        cp .env.sample .env
    else
        echo "[error] Neither .env nor .env.sample found in $(pwd)" >&2
        exit 1
    fi
fi

echo "[build] docker compose build --parallel"
docker compose build --parallel

echo "[done] Images built. Start the stack with: docker compose up -d"
