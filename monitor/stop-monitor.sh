#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

PORT=7003
if [ -f .env ]; then
    ENV_PORT="$(grep -E '^MONITOR_PORT=' .env | tail -1 | cut -d= -f2- || true)"
    [ -n "$ENV_PORT" ] && PORT="$ENV_PORT"
fi

PID=""
if command -v lsof >/dev/null 2>&1; then
    PID="$(lsof -ti tcp:"$PORT" || true)"
elif command -v fuser >/dev/null 2>&1; then
    PID="$(fuser "$PORT"/tcp 2>/dev/null || true)"
fi

if [ -z "$PID" ]; then
    echo "[stop] Nothing listening on port $PORT - monitor is not running."
    exit 0
fi

echo "[stop] Stopping monitor (PID $PID, port $PORT)"
kill "$PID"

for _ in $(seq 1 10); do
    kill -0 "$PID" 2>/dev/null || { echo "[done] Monitor stopped."; exit 0; }
    sleep 0.5
done

echo "[stop] Still running - forcing kill."
kill -9 "$PID"
echo "[done] Monitor stopped."
