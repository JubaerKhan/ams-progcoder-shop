---
name: verify-catalog-change
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
- verify
- reproduce
- test the fix
- test harness
- postgres
- database
- integration test
---

# Verifying a change to the Catalog service

Work through these in order and **stop at the first one that answers the
question.** Most changes are settled by step 1.

## 1. Read the diff

For a null guard, a boundary check, a comment, or a rename, `git --no-pager diff`
is sufficient verification. State in the PR body why the change is correct. Stop
here.

## 2. Compile the one project you touched

See the `dotnet-sdk` skill. Use this when the change could fail to compile —
a new type, a changed signature, a new using.

## 3. Use the infrastructure the repository already ships

If you genuinely need a running database, **do not install one.** The repository
provides it:

```bash
cd progcoder-shop
docker compose -f docker-compose.infrastructure.yml up -d
```

That starts Postgres on `7301`, Keycloak, MinIO, and the observability stack,
without building the application containers.

## What never to do

The following were all attempted on a previous run. Together they consumed 18 of
69 steps and proved nothing the diff had not already shown:

- `sudo apt-get install postgresql`, then `pg_ctlcluster ... start`, then
  `CREATE DATABASE` — the repository already provides Postgres, see step 3.
- Writing a scratch console project under `/tmp` with its own `.csproj`, then
  hand-copying entity and DTO source into it to exercise one method.
- Reading `Marten.dll` with `strings` to work out an API signature. Read the
  source in `Core/Catalog.Domain/` instead.

If a change truly cannot be verified without a full running stack, say so in the
PR body and let the human decide. That is a better outcome than an hour of
scaffolding.

## Reproducing the seeded defects

The catalog API needs no authentication.

```bash
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:7101/admin/products/all          # DivideByZero -> 500
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:7101/admin/products/{unpublished-id}  # NullRef -> 500
```

`GET /products/{id}` will **not** reproduce the NullReferenceException. That
route filters unpublished products and returns 404 before the defect runs. Only
the `/admin/` route reaches the handler.
