# AMS OTEL Monitor

A standalone (non-Docker) process that turns the otel-collector's live log
stream into deduplicated, file-backed "issues" — exposed both as MCP tools
for an AI agent and as outbound webhooks.

## Why this exists

The main stack already has Loki/Tempo/Prometheus/Grafana for humans to look
at. This tool is for machines: an AI agent evaluating this app (AMS) needs a
queryable, deduplicated feed of "what's currently broken" without having to
learn LogQL, and something needs to push a notification the moment a new
issue appears rather than waiting for someone to open a dashboard.

## How it works

```
Catalog.Api --(OTLP gRPC, already existed)--> otel-collector --+--> Loki (unchanged)
                                                                 |
                                          (new) otlphttp/monitor exporter
                                                                 |
                                                                 v
                                            monitor.py  (runs on the host,
                                                          not in Docker)
                                              |
                                              +-- filters to WARN/ERROR/FATAL
                                              +-- deduplicates by fingerprint
                                              +-- writes data/{SEVERITY}/{fingerprint}.json
                                              +-- POSTs to MONITOR_WEBHOOK_URL
                                              +-- serves MCP tools at /mcp
```

**otel-collector config change** (`../progcoder-shop/config/otel-collector/otel-collector-config.yaml`):
the existing `logs` pipeline already exports to Loki; a second exporter,
`otlphttp/monitor`, was added to the *same* pipeline, so the collector now
duplicates every log record to both destinations. No application code
changed, no new instrumentation was added — this reuses the log stream
that was already flowing. The endpoint is `host.docker.internal:7787`
because this monitor runs on the host, outside Docker, and that's how a
container reaches the host on Docker Desktop.

Two things are non-obvious about that config and are commented in the YAML:
- `otlphttp` exporters append their own signal-specific path
  (`/v1/logs`) to whatever `endpoint` you give them — the endpoint here is
  just the bare host:port.
- `otlphttp` gzip-compresses by default; `compression: none` is set so this
  lightweight receiver doesn't need a gzip-decoding dependency.

**Filtering** happens in the monitor, not the collector: every log record
is still received, but anything below Warning severity is dropped
immediately, before it's fingerprinted or touches disk. (The collector's
`otel/opentelemetry-collector` core image doesn't include the `filter`
processor — that's contrib-only — so filtering here avoided changing the
collector's image.)

**Deduplication**: a fingerprint is computed from
`service_name | severity | (exception_type or scope_name) | message`. The
same crash happening 50 times produces one file with `count: 50` and a
capped list of the most recent occurrence timestamps/trace IDs
(`MONITOR_MAX_OCCURRENCES`, default 100) — not 50 files. Note that one
underlying crash can still legitimately produce 2-3 *different* fingerprints,
because ASP.NET itself logs it more than once (the request-logging
middleware's "HTTP GET ... -> 500" line, the framework's own
"UnhandledException" diagnostic log, and this app's `CustomExceptionHandler`
log line all fire for the same exception, from different scopes) — each is
tracked as its own issue, which in practice gives you both a
"summary" line and the full exception detail with stack trace, without
losing either.

**Storage layout**:
```
data/
  WARN/
    <fingerprint>.json
  ERROR/
    <fingerprint>.json
```
Each file: `fingerprint`, `severity`, `service_name`, `exception_type`,
`scope_name`, `message`, `sample_stack_trace`, `first_seen`, `last_seen`,
`count`, `occurrences: [{timestamp, trace_id, span_id, request_path}, ...]`.
`trace_id`/`span_id` match Tempo's IDs exactly, so you can jump from an
issue straight to its trace.

**Webhook**: if `MONITOR_WEBHOOK_URL` is set, every new issue and every
repeat occurrence fires an HTTP POST:
```json
{
  "event": "new_issue",
  "severity": "ERROR",
  "service_name": "catalog.service",
  "fingerprint": "95166b8e2d1a6bf9",
  "exception_type": "System.DivideByZeroException",
  "message": "Attempted to divide by zero.",
  "count": 1,
  "first_seen": "...", "last_seen": "...",
  "occurrence": { "timestamp": "...", "trace_id": "...", "span_id": "...", "request_path": "/admin/products/all" }
}
```

**MCP tools** (served at `http://<host>:7787/mcp`, Streamable HTTP transport):
- `list_issues(severity?, limit=50)` — deduplicated issue summaries, most recent first
- `get_issue(fingerprint)` — full detail incl. stack trace and every occurrence
- `get_stats()` — counts by severity, total occurrences, services seen
- `list_recent_occurrences(minutes=60, severity?)` — flat, non-deduplicated feed for "what just happened"

**REST read API** (same data, for plain HTTP clients — dashboards, `curl`,
non-agent services — served at `http://<host>:7787`):
- `GET /issues?severity=WARN|ERROR&limit=50` — deduplicated issue summaries
- `GET /issues/{fingerprint}` — full detail for one issue (404 if unknown)
- `GET /stats` — counts by severity, total occurrences, services seen
- `GET /occurrences?minutes=60&severity=WARN|ERROR` — flat recent-occurrence feed

The MCP tools and the REST routes call the same shared read helpers, so the
two interfaces can never drift. Both read from the same in-memory index the
ingestion path writes to, so they always reflect the current on-disk state.
The REST routes sit behind the same auth as `/mcp` — when `MONITOR_API_KEY`
is set they require `Authorization: Bearer <key>` (only `/health` stays open).

Example:

```bash
curl -H "Authorization: Bearer <key>" http://<host>:7787/stats
```

## Exposing this beyond localhost (LAN access)

`MONITOR_HOST` already defaults to `0.0.0.0`, so the moment the port is
reachable, anyone who can reach it can too — there's nothing else to change
here to make it *reachable*. Three separate things matter for making it
*reachable safely*:

1. **Firewall**: Windows blocks inbound connections from other machines by
   default even though the process is listening on all interfaces. Open it
   yourself (this is a system-settings change, so it's intentionally not
   automated):
   ```powershell
   New-NetFirewallRule -DisplayName "AMS OTEL Monitor" -Direction Inbound -Protocol TCP -LocalPort 7787 -Action Allow
   ```
2. **Auth**: set `MONITOR_API_KEY` (see `.env.sample`) once this is reachable
   by more than just localhost/Docker. Every request to `/mcp` and `/v1/logs`
   then requires `Authorization: Bearer <key>` or gets a 401; `/health` stays
   open (it reveals nothing but a count). The same key must be set as
   `MONITOR_API_KEY` in `../progcoder-shop/.env`, since that's what the
   otel-collector sends as the `Authorization` header on `otlphttp/monitor`
   (see its `headers:` block) — the collector and the monitor are two
   separate processes with two separate env files that both need the same
   value.
3. **Process lifetime**: this needs to actually still be running for anyone
   else to reach it. A background task tied to someone's editor/agent session
   dies with that session — run it in a terminal you intend to leave open, or
   wire it into Task Scheduler / a Windows service if it needs to survive
   reboots.

There's still no TLS — traffic (including the API key) is plaintext HTTP.
Fine on a trusted LAN; don't put this on anything routable from the internet
without adding TLS in front of it (e.g. a reverse proxy).

## Running it

This does **not** run in Docker. The `mcp` package depends on `pydantic-core`
(a Rust extension) which has no prebuilt wheel for the mingw64 Python that
ships with Git Bash on this machine — use the standard Windows Python
instead:

```bash
py -3.11 -m pip install -r requirements.txt
py -3.11 monitor.py
```

Config is via environment variables (see `.env.sample`) — copy it to `.env`
and load it however you prefer (this script does not auto-load `.env`;
it only reads `os.environ`, so either `set -a; source .env` in bash,
or `$env:VAR=...` in PowerShell, or your process manager's env config).

Once running, you should see:
```
[monitor] listening on http://0.0.0.0:7787  (MCP: /mcp, OTLP log ingest: /v1/logs, health: /health)
```

`GET /health` is a quick liveness/sanity check (`{"status":"ok","issues_tracked":N}`).

## Wiring an MCP client to it

For Claude Code, add to `.mcp.json`:
```json
{
  "mcpServers": {
    "ams-otel-monitor": {
      "type": "http",
      "url": "http://localhost:7003/mcp"
    }
  }
}
```

## Known limitations (by design, not oversights)

- **In-process, single-node**: one Python process holds the in-memory index;
  if it restarts, the index is rebuilt by re-reading every JSON file under
  `data/` at startup (see `IssueStore.load`), so nothing is lost, but there's
  a brief gap where the collector's retries queue up (or, past its retry
  budget, drops a batch — same as any exporter destination being briefly
  unreachable).
- **No auth** on `/v1/logs` or the MCP endpoint — acceptable for a local
  dev/test tool reached only via `host.docker.internal` and `localhost`;
  do not expose this port beyond that without adding authentication.
- **This only sees what otel-collector forwards it.** If a future service is
  added to the stack and its logs should also be monitored, nothing extra is
  needed here — it already receives everything the collector's `logs`
  pipeline carries; the per-service scoping happens naturally via each log's
  `service_name` attribute.
