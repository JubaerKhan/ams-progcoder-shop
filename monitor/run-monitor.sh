#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

if [ ! -f .env ] && [ -f .env.sample ]; then
    echo "[setup] .env not found - creating it from .env.sample"
    cp .env.sample .env
fi

PYBIN="$(command -v python3 || command -v python || true)"
if [ -z "$PYBIN" ]; then
    echo "[error] No usable Python 3 found on PATH." >&2
    exit 1
fi

if [ ! -d .venv ]; then
    echo "[setup] Creating virtual environment in .venv"
    "$PYBIN" -m venv .venv
fi

# shellcheck disable=SC1091
source .venv/bin/activate

echo "[setup] Installing dependencies"
python -m pip install -q --upgrade pip
python -m pip install -q -r requirements.txt

echo "[env] Loading .env"
set -a
# shellcheck disable=SC1091
source .env
set +a

echo "[run] Starting monitor.py  (Ctrl+C to stop)"
python monitor.py
