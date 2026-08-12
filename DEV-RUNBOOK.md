# Dev / deploy runbook (minimal slice)

This is a cut-down fork: **App.Admin** (React) + **Catalog.Api** (.NET 8) + the
databases/identity/observability they need. No gateway, no other business
services. See [`APPLICATION-OVERVIEW.md`](APPLICATION-OVERVIEW.md) for the
full rationale.

## What's running

| Piece | Where | Role |
|---|---|---|
| Postgres | localhost:7301 | Catalog_Service (Marten doc store) + Keycloak's own schema |
| MinIO | localhost:7402 (API), 7403 (console) | Object storage - **required**, not optional (see below) |
| Keycloak | http://localhost:7401 | Identity - `admin` / `123456789Aa` |
| OTEL Collector | localhost:7605 (OTLP), 7608 (health), 7609 (metrics) | Telemetry ingest |
| Loki | localhost:7603 | Logs |
| Tempo | localhost:7604 | Traces |
| Prometheus | http://localhost:7602 | Metrics - `admin` / `123456789Aa` |
| Grafana | http://localhost:7601 | Dashboards - `admin` / `admin` |
| **Catalog.Api** | http://localhost:7101/swagger | The one business service |
| **App.Admin** | http://localhost:7002 | The one front-end |
| pgAdmin (dev-only, `docker-compose.infrastructure.yml`) | http://localhost:7701 | `admin@progcoder.com` / `123456789Aa` |

## Start

```bash
docker compose build
docker compose up -d
```

For local hot-reload development instead of full-Docker:

```bash
docker compose -f docker-compose.infrastructure.yml up -d
cd src/Services/Catalog/Api/Catalog.Api && dotnet watch run
cd src/Apps/App.Admin && npm install && npm run dev
```

## Stop

```bash
docker compose down
```

Add `-v` to wipe data volumes.

## Why MinIO can't be dropped

It's tempting to cut MinIO too since it looks like a "storage nice-to-have."
It can't be: Admin's **Create Product** form submits a `FormData` file upload,
and both the frontend validation and the backend
(`CreateProductCommandHandler` → `IMinIOCloudService`) require a thumbnail.
Without MinIO running, product creation fails validation before the request
even reaches the database.

RabbitMQ, by contrast, was verified **not** needed - Catalog.Api has zero
MassTransit/RabbitMQ references anywhere in its own dependency chain. It only
existed in the outbox worker and consumer projects, both removed in this
slice.

## Why there's no gateway

The original stack routed through a YARP gateway that stripped a
`/catalog-service/*` path prefix before forwarding to Catalog.Api. With only
one backend service left, that indirection serves no purpose, so
`VITE_API_GATEWAY` now points straight at Catalog.Api and
`src/Apps/App.Admin/src/api/endpoints.js`'s `CATALOG` entries had the prefix
removed to match. (The other sections in that file - `INVENTORY`, `DISCOUNT`,
`ORDER`, `REPORT`, `NOTIFICATION`, `COMMUNICATION` - are left in place but
dead; nothing calls them successfully since those services don't exist here.)

Also removed: the Admin header's live notification bell
(`components/partials/header/Tools/Notification.jsx` was still imported and
rendered, polling `/notification-service/...` every 30 seconds once logged
in). With Notification.Api gone that polling would 404 forever and pop an
error toast on the same interval - the exact class of bug documented below
for App.Store. Removed the render call in `header/index.jsx`, left the
component file in place in case Notification is restored later.

## Machine-specific setup traps (still apply)

1. **CRLF breaks Postgres init.** Git clones `config/postgres-sql/*.sh` with
   CRLF on Windows, which makes the container fail silently with
   `/usr/bin/env: 'bash\r': No such file or directory` and skip creating both
   databases while reporting healthy. A `.gitattributes` pins `*.sh` to LF. If
   you re-clone on Windows, verify LF before first `up`, or wipe
   `docker-volumes/postgres-sql` afterward (initdb only runs once).

2. **No Keycloak realm ships with the repo.** `prog-coder-realm` must be
   created by hand (or via `kcadm`) before Admin's login works at all:
   - `prog-coder-client-id` - public, PKCE S256, redirect URI
     `http://localhost:7002/*`
   - `prog-coder-service-account` - confidential, service accounts on,
     secret matching `appsettings.json`
   - a test user

3. **The healthcheck tools don't exist in the base images.** `curl` and
   `sqlcmd` aren't in `dotnet/aspnet:8.0`. Catalog.Api's healthcheck uses
   `bash`'s `/dev/tcp` to do a real HTTP GET instead - and it must be invoked
   as `["CMD", "bash", "-c", "..."]`, not `CMD-SHELL` (which runs `/bin/sh`,
   and `/dev/tcp` is a bash-only feature).

4. **`.npmrc` may be machine-specific.** If your clone path contains an `&`
   (cmd.exe treats it as a command separator, breaking npm's script shim),
   `src/Apps/App.Admin/.npmrc` pins `script-shell` to a Windows Git-bash path.
   Delete it if your path has no `&`, or if you're not on Windows.

## Seeding demo data

Catalog ships a built-in seeder, idempotent past 15 products:

```bash
TOK=$(curl -s -X POST "http://localhost:7401/realms/prog-coder-realm/protocol/openid-connect/token" -d "client_id=prog-coder-client-id" -d "username=demo" -d "password=demo123" -d "grant_type=password" -d "scope=openid" | python -c "import sys,json;print(json.load(sys.stdin)['access_token'])")
curl -s -X POST http://localhost:7101/admin/system/initialize-data -H "Authorization: Bearer $TOK"
```

Seeded products are created `Published=false`; publish them individually via
`POST /admin/products/{id}/publish` to raise `UpsertedProductDomainEvent` (it
only writes to Catalog's own outbox table now - there's no worker left to
forward it anywhere, since Search and RabbitMQ were both cut).

## Known gaps in this slice

- **Everything else in Admin's sidebar is a dead link or a dead page.**
  Orders, Inventory, Coupons, Reports, and the Communication/SignalR bits
  still exist in the UI (routes, nav entries, pages) but their backing
  services were removed. Visiting them will show request errors - this was a
  deliberate scope decision (see `APPLICATION-OVERVIEW.md`), not an oversight.
  The one exception already fixed is the header notification poller, which
  ran unconditionally rather than only on a page visit.
- **Grafana dashboards for RabbitMQ/Redis/Elasticsearch/cAdvisor/node_exporter
  were removed** along with the containers they monitored. `MinIO`,
  `Prometheus Stats`, and `Logging & Tracing via Loki` remain.
- **Prometheus no longer scrapes exporters that don't exist** (node_exporter,
  cadvisor, rabbitmq, elasticsearch, redis). Its `microservices` file_sd job
  still points at `config/prometheus/services/catalog-service.yml`, which is
  redundant with OTLP push but harmless.
- Port scheme is unchanged from the full stack (7xxx grouped by tier) even
  though most tiers are now empty - kept for consistency in case services are
  restored later.
