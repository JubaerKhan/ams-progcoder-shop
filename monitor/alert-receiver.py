"""
AMS Alert Receiver
==================
Runs on the OTHER PC (not the monitor host). Catches the webhook the monitor
fires on every error/issue and prints it + appends it to a local log file.

Dependency-free: standard-library only, so it runs on ANY Python 3.7+ on the
receiving machine (no pip install, no venv, no compiled-wheel headaches).

Run on the receiving PC:
    python alert-receiver.py                 # listens on 0.0.0.0:9000, path /hook
    python alert-receiver.py --port 9001      # different port
    py alert-receiver.py                      # Windows py launcher also fine

Then on the MONITOR host, set in monitor/.env:
    MONITOR_WEBHOOK_URL=http://<THIS-PC-IP>:9000/hook
and restart the monitor. Open port 9000 in this PC's firewall for the LAN.
"""

from __future__ import annotations

import argparse
import json
from datetime import datetime
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

LOG_FILE = Path(__file__).with_name("alerts.jsonl")
HOOK_PATH = "/hook"


class Handler(BaseHTTPRequestHandler):
    # Silence the default per-request stderr logging; we print our own line.
    def log_message(self, *args) -> None:  # noqa: D401
        pass

    def do_GET(self) -> None:
        if self.path.rstrip("/") in ("", "/health"):
            self._send(200, b'{"status":"ok"}')
        else:
            self._send(404, b"not found")

    def do_POST(self) -> None:
        if self.path.split("?", 1)[0].rstrip("/") != HOOK_PATH.rstrip("/"):
            self._send(404, b"not found")
            return

        length = int(self.headers.get("Content-Length", 0) or 0)
        raw = self.rfile.read(length) if length else b""
        try:
            alert = json.loads(raw.decode("utf-8"))
        except Exception:
            self._send(400, b"bad json")
            return

        self._print_alert(alert)
        try:
            with LOG_FILE.open("a", encoding="utf-8") as f:
                f.write(json.dumps({"received_at": datetime.now().isoformat(), **alert}) + "\n")
        except Exception as e:
            print(f"  [warn] could not append to {LOG_FILE.name}: {e}")

        self._send(204, b"")

    def _send(self, status: int, body: bytes) -> None:
        self.send_response(status)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        if body:
            self.wfile.write(body)

    @staticmethod
    def _print_alert(a: dict) -> None:
        event = a.get("event", "?")
        tag = "NEW  " if event == "new_issue" else "REPEAT"
        now = datetime.now().strftime("%H:%M:%S")
        print(
            f"[{now}] {tag} {a.get('severity','?'):<5} "
            f"{a.get('service_name','?')}  {a.get('exception_type') or '(no type)'}  "
            f"x{a.get('count','?')}  fp={a.get('fingerprint','?')}"
        )
        msg = a.get("message")
        if msg:
            print(f"           {str(msg)[:120]}")
        occ = a.get("occurrence") or {}
        if occ.get("request_path") or occ.get("trace_id"):
            print(f"           path={occ.get('request_path')}  trace={occ.get('trace_id')}")


def main() -> None:
    ap = argparse.ArgumentParser(description="Receive AMS monitor webhook alerts.")
    ap.add_argument("--host", default="0.0.0.0")
    ap.add_argument("--port", type=int, default=9000)
    args = ap.parse_args()

    server = ThreadingHTTPServer((args.host, args.port), Handler)
    print(f"[receiver] listening on http://{args.host}:{args.port}{HOOK_PATH}")
    print(f"[receiver] logging alerts to {LOG_FILE}")
    print(f"[receiver] set MONITOR_WEBHOOK_URL=http://<this-pc-ip>:{args.port}{HOOK_PATH} on the monitor host")
    print("[receiver] Ctrl+C to stop")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\n[receiver] stopped")


if __name__ == "__main__":
    main()
