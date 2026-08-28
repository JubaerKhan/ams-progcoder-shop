# Shop Microservices (minimal slice)

A deliberately minimal cut of a larger .NET 8 + React microservices e-commerce
reference project, kept down to one working use case - **catalog management** -
plus the full observability stack, so it can be deployed and exercised on a
small server without the resource cost of the full system.

This is a fork of a larger multi-service application. See
[`APPLICATION-OVERVIEW.md`](APPLICATION-OVERVIEW.md) for what was kept, what
was cut, and why.

## What's here

| Kept | Cut |
|---|---|
| App.Admin (React/Vite back office) | App.Store (customer storefront) |
| Catalog.Api (products, brands, categories) | Basket, Inventory, Order, Discount, Notification, Search, Report, Communication |
| Postgres, MinIO, Keycloak | MongoDB, MySQL, SQL Server, Elasticsearch, Redis, RabbitMQ, Mailhog |
| OTEL Collector, Loki, Tempo, Prometheus, Grafana (all 3 observability pillars) | YARP API Gateway, Job Orchestrator, all gRPC hosts, all background workers |

Admin talks to Catalog.Api directly - there is no gateway in this slice.

## Getting started

**Read [`DEV-RUNBOOK.md`](DEV-RUNBOOK.md) first.** It documents the working
setup, ports, seeding, and several non-obvious traps (CRLF line endings
breaking Postgres init, the missing Keycloak realm, MinIO being a hard
dependency for product creation, and more).

```bash
docker compose build
docker compose up -d
```

Prerequisites: Docker Desktop, ~4 GB of disk for images, ~4-5 GB free RAM.

## Repository layout

```
src/
  Apps/App.Admin/    the one front-end
  Services/Catalog/   the one business service (Api / Core)
  Shared/             BuildingBlocks, Common, EventSourcing
config/               per-container configuration (Grafana, Prometheus, Loki, Keycloak, Postgres, pgAdmin)
```

## Licence

MIT - see [`LICENSE`](LICENSE). This project is derived from an MIT-licensed
upstream work; the original copyright notice is retained there as the licence
requires.
