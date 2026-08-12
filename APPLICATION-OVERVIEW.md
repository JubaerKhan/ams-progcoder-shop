# Application Overview (minimal slice)

This repo was cut down from a much larger .NET 8 + React microservices
reference project (9 business services, a YARP gateway, 2 front-ends, a job
orchestrator, 5 gRPC hosts, 10 workers, 47 containers) to a small, single-use-case
deployment for testing on a real server. This document records what was kept,
what was cut, and why - so the reasoning isn't lost.

For setup instructions, see [`DEV-RUNBOOK.md`](DEV-RUNBOOK.md). For a
quickstart, see [`README.md`](README.md).

## What's kept, and why

The one use case kept is **catalog management**: creating, editing, and
publishing products, brands, and categories through App.Admin. It was chosen
because it's the cleanest vertical slice - one frontend, one backend service -
and it's the one already proven end-to-end (seed → publish → verify).

| Kept | Why |
|---|---|
| **App.Admin** | The one front-end that exercises the use case |
| **Catalog.Api** | The one business service |
| **Postgres** | Catalog's Marten document store + Keycloak's own schema |
| **MinIO** | **Verified hard dependency** - Admin's Create Product form requires a thumbnail upload; both frontend validation and the backend `CreateProductCommandHandler` enforce it via `IMinIOCloudService` |
| **Keycloak** | Admin's entire login/JWT flow depends on it |
| **OTEL Collector, Loki, Tempo, Prometheus, Grafana** | All 3 observability pillars (logs, metrics, traces), explicitly kept as a deliverable in their own right, not just app plumbing |

## What was cut, and why

| Cut | Why it could go |
|---|---|
| App.Store | Not the app being tested; a separate, unrelated React codebase from App.Admin |
| Basket, Inventory, Order, Discount, Notification, Search, Report, Communication (APIs, 5 gRPC hosts, 10 workers) | Not part of the one kept use case. Catalog.Api was verified to have **zero** dependency on any of them - no MassTransit/RabbitMQ, no gRPC client to Discount (an earlier automated scan suggesting otherwise was a false positive, corrected by reading the actual config) |
| YARP API Gateway | With one backend service left, path-prefix routing has nothing to route between. Admin now calls Catalog.Api directly (`VITE_API_GATEWAY` repointed; the `/catalog-service` prefix removed from `endpoints.js`) |
| App.Job (Quartz orchestrator) | Its scheduled jobs (`ExpireInventoryReservations`, `SyncDashboardReport`) call gRPC services that no longer exist |
| MongoDB, MySQL, SQL Server, Elasticsearch, Redis | Each belonged to exactly one of the cut services |
| RabbitMQ | **Verified not needed** by Catalog.Api itself - only its outbox worker and consumer (both cut) referenced it |
| Mailhog | Only Notification used it |
| pgAdmin, node_exporter, cAdvisor, Portainer, Promtail | Dev/ops conveniences, not part of the deployable minimum. pgAdmin was kept in `docker-compose.infrastructure.yml` only, for local Postgres inspection |

## A bug the cut exposed and fixed

Removing the gateway meant Admin calls Catalog.Api directly, which meant
Admin's hardcoded `/catalog-service/...` path prefixes (a relic of the
gateway's routing) would have 404'd every request. Fixed by stripping the
prefix from `endpoints.js`'s `CATALOG` section to match Catalog.Api's actual
root-mounted routes.

Separately, cutting Notification.Api surfaced a real, independent bug:
Admin's header notification bell polled `/notification-service/...` every 30
seconds *unconditionally* once a user was logged in - not just when a
specific page was visited. With that service gone, it would have popped an
error toast every 30 seconds forever. This is the same class of bug found and
fixed earlier in App.Store (a background poller with no gate on service
availability). Fixed by removing the component from the header rather than
leaving it to fail silently or loudly.

## Dependency verification, not assumption

Before cutting RabbitMQ and the gateway, both were checked directly in
source rather than assumed:

- `grep`'d Catalog.Api, Catalog.Application, and Catalog.Infrastructure for
  MassTransit/RabbitMQ references - none found.
- Read Catalog.Api's actual `appsettings.Development.json` - its real
  dependencies are exactly Postgres, MinIO, Keycloak, and OTLP. An earlier
  automated port-scan had suggested a gRPC dependency on Discount; re-reading
  the source file directly showed that was a false positive.
- Checked whether `AddMarten(...)` and `AddMinio(...)` connect eagerly at
  startup (they don't - both are lazy client factories, and
  `InitializeMartenWith(...)` is commented out in the source) - meaning the
  service can boot even before its dependencies are ready.
- Read Admin's `create-product.jsx` and `CreateProductCommand.cs` directly to
  confirm the thumbnail requirement, rather than assuming MinIO was optional
  because it "sounds like storage, probably unused."

## Solution/project cleanup

`progcoder-shop-microservices.sln` went from 56 projects to 7, via
`dotnet sln remove` (not hand-edited - the `.sln` GUID/config format is easy
to corrupt by hand):

```
BuildingBlocks, Common, EventSourcing        (Shared)
Catalog.Api, Catalog.Application,
Catalog.Domain, Catalog.Infrastructure       (the one service)
```

`Catalog.Grpc` and both Catalog workers were also removed - their dependency
graph confirmed only `Catalog.Api` + its 3 Core projects were needed.
`Catalog.Contract` (a gRPC DTO project) was only referenced by `Catalog.Grpc`,
so it went too.

## Observability - what each piece does and why (unchanged from the full stack)

None of these are part of the *business* application - they're instrumentation,
wired in via OpenTelemetry so the system's behavior can be observed rather
than guessed at. Kept in full because the observability stack itself was an
explicit requirement of this cut, not incidental.

| Tool | Role | Why it's there |
|---|---|---|
| **OTEL Collector** | Ingest point | Catalog.Api pushes logs/metrics/traces here over OTLP; fans out to the three backends below |
| **Loki** | Log storage | Structured logs, queryable by label (`service.name`, `trace_id`) |
| **Tempo** | Trace storage | Distributed traces with per-hop timing. (Caveat, inherited from the full stack: nothing emits a gateway-hop span anymore, since there's no gateway) |
| **Prometheus** | Metrics storage | Scraped from the OTEL Collector's Prometheus exporter and MinIO's own metrics endpoint |
| **Grafana** | Visualization | Dashboards over all three; `MinIO`, `Prometheus Stats`, and `Logging & Tracing via Loki` are provisioned in this slice |

## Ports

Unchanged 7xxx scheme from the full stack (70xx apps, 71xx APIs, 73xx
databases, 74xx identity/storage, 76xx observability), even though most
tiers are now sparsely populated - kept for consistency in case a cut service
is restored later. Full details in `DEV-RUNBOOK.md`.
