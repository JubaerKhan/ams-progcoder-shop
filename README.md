# AMS test monitor project

Two independent pieces: a monitored application, and the AMS monitor that
watches it. They are deliberately separate — nothing but HTTP crosses between
them, so the monitor can point at a different target without being rebuilt.

| Directory | What it is |
|---|---|
| [`progcoder-shop/`](progcoder-shop/) | The monitored application — a 10-container slice of a .NET 8 / React e-commerce app with a full OpenTelemetry stack (Collector, Loki, Tempo, Prometheus, Grafana) and two deliberately seeded defects. Start here: [`progcoder-shop/DEV-RUNBOOK.md`](progcoder-shop/DEV-RUNBOOK.md). |
| [`monitor/`](monitor/) | The AMS monitor — a standalone (non-Docker) host process that receives a duplicated log stream, deduplicates issues, fires webhooks, and serves them to an AI agent as MCP tools. See [`monitor/README.md`](monitor/README.md). |

## How the two connect

```
progcoder-shop            otel-collector                 monitor (host process)
  Catalog.Api  --OTLP-->  logs pipeline  --+--> Loki
                                           |
                                           +--> http://host.docker.internal:7003/v1/logs
```

The coupling is a single exporter entry in
[`progcoder-shop/config/otel-collector/otel-collector-config.yaml`](progcoder-shop/config/otel-collector/otel-collector-config.yaml)
that duplicates the existing log stream to the monitor. No application code
was changed to make that path exist.

One value must match across the boundary: `MONITOR_API_KEY` in
`progcoder-shop/.env` (what the collector sends) and in the monitor's own
environment (what it checks). They are separate processes with separate env
files.

## Running

Each directory is self-contained — run commands from *inside* it, not from
this root, since compose files and scripts resolve paths relative to
themselves.

```bash
cd progcoder-shop && docker compose up -d
```

```bash
cd monitor && ./run-monitor.sh
```
