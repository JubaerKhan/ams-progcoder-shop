# Deployment — Windows Server (192.168.28.129)

The Linux deploy target and this one differ in more than paths, so this is its
own document. Two things to hold onto before reading further:

- **This repo is two independent pieces.** `progcoder-shop/` is a 10-container
  Docker Compose stack. `monitor/` is a **host process** — plain Python, not a
  container. A push can redeploy the first; it cannot restart the second.
- **Commands run from inside each directory**, never from the repo root. Both
  the compose file and the monitor scripts resolve paths relative to
  themselves.

Everything below assumes the server as it is today: Windows 10 (19045),
Git 2.54, Docker 29.5.3 with Compose v5.1.4, Python 3.11.15 on `PATH` as
`python` (there is **no** `py` launcher on this box, unlike the dev machines —
`py -3.11 monitor.py` from `monitor/README.md` will fail here).

---

## Running Locally

Requires Docker and Docker Compose.

```bash
cd progcoder-shop
docker compose up -d --build
```

Ten containers come up: **postgres-sql** (`7301`), **minio** (`7402` API /
`7403` console), **keycloak** (`7401`), **otel-collector** (`7605` OTLP gRPC,
plus its diagnostic ports `7606`–`7610`), **loki** (`7603`), **tempo**
(`7604`), **prometheus** (`7602`), **grafana** (`7601`), **catalog-api**
(`http://localhost:7101`) and **app-admin** (the React SPA, on
`http://localhost:7002`).

Then, in a second terminal, the monitor — it is deliberately outside Docker:

```bash
cd monitor
python monitor.py
```

It listens on `http://0.0.0.0:7003` (`/mcp`, `/v1/logs`, `/health`). The
collector reaches it at `host.docker.internal:7003`, which is how a container
talks to the host under Docker Desktop; nothing else connects the two.

`app-admin` is a static SPA — Vite inlines the `VITE_*` values into the bundle
at **build** time, so every address the browser will use has to be known when
the image is built, not when the container starts. See
[Addresses baked at build time](#notes) below; it is the one part of this stack
that does not survive a mere restart.

To stop:

```bash
cd progcoder-shop && docker compose down
```

```bash
cd monitor && ./stop-monitor.sh
```

### One-time: the two .env files

Both are gitignored and **never travel with a push** — they have to exist on
every machine that runs this, the server included. They are separate files for
two separate processes:

```bash
cp progcoder-shop/.env.sample progcoder-shop/.env
cp monitor/.env.sample monitor/.env
```

The one value that must **match across both files** is `MONITOR_API_KEY`. The
collector sends it as a bearer token on its `otlphttp/monitor` exporter; the
monitor checks it on `/v1/logs`, `/mcp` and the REST routes. A mismatch is
silent from the collector's side — logs simply stop arriving, with a 401.

Also check in `progcoder-shop/.env`:

- `APP_ORIGIN` — appended to the Catalog API's CORS list
  (`CorsConfig__Domains__1`). Must be the origin the SPA is actually served
  from, i.e. `http://192.168.28.129:7002` here. Left at its
  `http://localhost:7002` default, browsing the dashboard by IP fails on CORS
  even though the API is reachable.
- the `VITE_*` block — see the build-time note in [Notes](#notes).

And in `monitor/.env`: `MONITOR_WEBHOOK_URL` (blank disables outbound POSTs)
and `MONITOR_DATA_DIR` (issue JSON files; relative to `monitor/`, so each
checkout keeps its own).

Note the monitor does **not** auto-load its `.env` — it reads `os.environ`
only. `run-monitor.cmd` / `run-monitor.sh` do that loading for you; if you
launch `python monitor.py` directly, export the variables yourself
(`set -a; source .env` in bash, `$env:VAR=...` in PowerShell).

### One-time: Keycloak realm

Login is Keycloak-backed (`prog-coder-realm`, client `prog-coder-client-id`).
A realm whose valid redirect URIs list only `http://localhost:7002/*` will
refuse a login made against the LAN IP — add `http://192.168.28.129:7002/*` to
that client's redirect URIs and web origins, in the admin console at
`http://192.168.28.129:7401`. Postgres holds the realm in `keycloak_db`, so
this survives redeploys and is lost only by destroying the volumes.

---

## Deploying to the Windows Server

The server is at `192.168.28.129`. The repo lives at
`D:\BJIT AMS\ProG-shop-monitor-app-github`, which today is an empty git repo:
a `.git` folder, `deploy` checked out with **no commits yet**, no remotes, and
an empty work-tree. Pushing to the `deploy` branch triggers an automatic
deployment via a post-receive hook.

### One-time setup — on the server

Over SSH (`ssh bjit@192.168.28.129`), in **Git Bash** — the hook is a POSIX
script and the paths below are MSYS-style (`D:\` is `/d/`):

```bash
git -C "/d/BJIT AMS/ProG-shop-monitor-app-github" config receive.denyCurrentBranch ignore
```

That config line is not optional: this is a non-bare repo with `deploy` checked
out, and git refuses by default to accept a push into the branch a work-tree is
sitting on. `ignore` lets the push land in the ref, and the hook is what
updates the files — which is also what makes `git push --force` behave
predictably here.

Then install the hook, which ships in this repo as
[progcoder-shop/deploy/post-receive-windows](progcoder-shop/deploy/post-receive-windows):

```bash
cp "/d/BJIT AMS/ProG-shop-monitor-app-github/progcoder-shop/deploy/post-receive-windows" "/d/BJIT AMS/ProG-shop-monitor-app-github/.git/hooks/post-receive"
```

The repo is empty right now, so there is nothing there to copy yet: push once,
then install the hook from the checkout that push created, then push again.
(Or paste the hook file in by hand first, and the very first push deploys.)

The server also needs its own two `.env` files, created once as above, with
`APP_ORIGIN=http://192.168.28.129:7002`.

### One-time setup — on your machine

```bash
git remote add prog-win "ssh://bjit@192.168.28.129/D:/BJIT AMS/ProG-shop-monitor-app-github"
```

```bash
git config core.sshCommand "C:/Windows/System32/OpenSSH/ssh.exe"
```

> On Linux/macOS, skip the `core.sshCommand` line.

### Deploying

Push your branch to the `deploy` branch on the server:

```bash
git push prog-win prog-shop-monitor-test-app:deploy
```

The hook checks out the code and runs:

```bash
docker compose -f docker-compose.yml up --build -d
```

On the LAN the stack is then at `http://192.168.28.129:7002` (admin SPA),
`http://192.168.28.129:7101` (Catalog API, `/swagger`),
`http://192.168.28.129:7601` (Grafana), and `http://192.168.28.129:7003` for
the monitor (`/health`, `/stats`, `/issues`, `/mcp`).

### The monitor is not deployed by the hook

It is a host process, so a push cannot restart it. After a push that touched
anything under `monitor/`, restart it by hand on the server:

```bash
cd "/d/BJIT AMS/ProG-shop-monitor-app-github/monitor" && ./stop-monitor.sh && ./run-monitor.sh
```

Two things to know before you do:

- **Port 7003 is already taken on that server** — a monitor is listening right
  now, run out of the older checkout at `D:\BJIT AMS\p1888_ai_trans_agentic_ams`.
  Only one process can hold the port; stop that one first or the new one dies
  on bind. The Windows firewall rule for 7003 already exists on this box, so
  nothing is needed there.
- A background process tied to an SSH or editor session **dies with that
  session**. Use `run-monitor.cmd` (it detaches), or wire it into Task
  Scheduler if it needs to survive reboots.

---

## Notes

- **This server already runs this same stack from a different directory.**
  `D:\BJIT AMS\p1888_ai_trans_agentic_ams\progcoder-shop` has all ten
  containers up right now. Compose derives its project name from the compose
  file's directory — `progcoder-shop` in both checkouts — so a deploy here does
  not start a second stack beside it: it **adopts and recreates those very
  containers**, and the old checkout stops being the source of what is running.
  Fine if that is the intent, confusing if it isn't. Keeping them genuinely
  separate needs a distinct `-p <name>` **and** a different set of host ports;
  as written they collide on every one.
- **Addresses baked in at build time.** `VITE_KEYCLOAK_URL`,
  `VITE_KEYCLOAK_BASE_URL` and `VITE_API_GATEWAY` come from
  `progcoder-shop/.env`; `VITE_KEYCLOAK_REDIRECT_URI` is hardcoded to
  `http://localhost:${APP_ADMIN_PORT}/` in the compose file's `build.args`.
  All four are compiled into the bundle. To browse the SPA from another machine
  on the LAN, point them at `192.168.28.129` (editing the compose file for the
  redirect URI) **and rebuild** — a restart keeps serving the old addresses:

  ```bash
  cd progcoder-shop && docker compose build app-admin && docker compose up -d --force-recreate app-admin
  ```

  `--force-recreate` matters — `up -d` alone does not reliably swap a running
  container onto a freshly rebuilt `latest` image.

  `MinIO__PublicEndpoint` in the compose file is `localhost:${MINIO_API_PORT}`
  for the same class of reason: it is the address the *browser* uses for
  product image URLs, so it needs the server's IP too. That one is a plain
  environment variable, so a recreate is enough — no rebuild.
- **A frontend code change always needs a rebuild.** There is no hot-reload
  profile in this compose file; for live iteration run the SPA's dev server
  locally against the deployed API instead.
- **Any branch can be pushed to `deploy`** — whoever pushes last is what gets
  deployed. Use `git push --force prog-win your-branch:deploy` if a push is
  rejected (after a rebase, or if someone pushed ahead of you).
- **Data lives in named volumes** — Postgres (accounts, realm, catalog), MinIO
  objects, Grafana, Loki, Tempo, Prometheus. It survives redeployments;
  `docker compose down -v` is what destroys it. The monitor's issues are *not*
  in a volume — they are plain JSON under `monitor/data/`, and the monitor
  rebuilds its in-memory index from them at startup, so a restart loses nothing
  but the window while it is down (the collector retries, then drops the batch
  once past its retry budget).
- **No TLS anywhere.** The bearer token, the Keycloak credentials and every log
  record cross the LAN in plaintext. Acceptable inside the office network and
  not outside it — put a TLS-terminating reverse proxy in front before this is
  reachable from anywhere else, then update `APP_ORIGIN`, the Keycloak redirect
  URIs and the SPA build args to the https origin.
