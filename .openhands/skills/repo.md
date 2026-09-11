# ProgCoder Shop — repository instructions

This repository is the test application for AMS (the Agentic Monitoring System).
AMS scans its telemetry, finds anomalies, and sends you here to fix them.

## Two defects are seeded on purpose

`ARCHITECTURE.md` marks both as live. They exist so the monitoring agent always
has a real incident to detect. Read this before you change anything.

| Defect | File | Trigger |
|---|---|---|
| `NullReferenceException` | `Catalog.Application/Features/Product/Queries/GetProductByIdQuery.cs` | `GET /admin/products/{id}` on an **unpublished** product |
| `DivideByZeroException` | `Catalog.Application/Features/Product/Queries/GetAllProductsQuery.cs` | any product with `SalePrice = 0` poisons the **whole** admin list |

**Never fix a seeded defect unless the task explicitly names it.** Fixing one
that was not asked for destroys the test AMS is running. If a task names one
defect, leave the other exactly as it is — including its comment.

## Where things live

Paths are relative to `progcoder-shop/src/Services/Catalog/`.

| What | Where |
|---|---|
| Query and command handlers | `Core/Catalog.Application/Features/Product/Queries/` |
| Result records | `Core/Catalog.Application/Models/Results/` |
| DTOs | `Core/Catalog.Application/Dtos/Products/` |
| Entities | `Core/Catalog.Domain/Entities/ProductEntity.cs` |
| AutoMapper profiles | `Core/Catalog.Application/` (files matching `*Profile*.cs`) |
| HTTP endpoints | `Api/Catalog.Api/Endpoints/` |
| Target framework | `src/Directory.Build.props` |

Persistence is **Marten** over PostgreSQL — documents, not EF Core. There are no
migrations. `session.LoadAsync<T>(id)` loads one document.

## Ports, when the stack is running

`7002` admin UI · `7101` catalog API · `7301` Postgres · `7401` Keycloak ·
`7601` Grafana · `7603` Loki · `7605` OTLP gRPC.

The catalog API needs **no authentication**. You can call it with plain `curl`.

## Two rules that override your own judgement

### 1. A failed `git push` is not yours to fix

If `git push` returns `403`, `Permission denied`, or
`Resource not accessible by personal access token` — **stop immediately and
report the exact error text.**

Do not rewrite the remote URL. Do not try other usernames. Do not look for a
fork. Do not inspect environment variables for tokens. Do not probe network
ports. The credential is managed by AMS outside the sandbox and cannot be
repaired from inside. Guessing wastes the entire run and still fails.

### 2. Do not build a throwaway test harness

Do not `apt-get install postgresql`, do not start a database cluster, and do not
write a scratch console project under `/tmp`. See the `verify-catalog-change`
skill for what to do instead.
