# What the AMS Edge Agent runs when a reviewer presses Deploy.
#
# AMS never sends a command over that channel. It says only "run it now", and
# everything a deploy actually does is decided here, in the repository, where a
# pull request reviews it. That is the whole reason this file exists rather than
# a text box in a dashboard.
#
# It does the same work as the push-based hook,
# progcoder-shop/deploy/post-receive-windows, with one addition. The hook is
# handed new code by the push that triggered it. Nothing pushes to this script,
# so it fetches the code itself.
#
# The agent invokes it as:
#   powershell -NoProfile -ExecutionPolicy Bypass -File <repo>\.ams\deploy.ps1
#
# AMS puts three values in the environment. This script uses them for the log
# only, and would work with none of them set:
#   AMS_DEPLOY_ID, AMS_REMEDIATION_ID, AMS_REPOSITORY

# Continue, not Stop. Under $ErrorActionPreference = "Stop", Windows PowerShell
# 5.1 can abort on a native command that merely writes to stderr, and git writes
# ordinary progress there. So every external call below checks $LASTEXITCODE.
$ErrorActionPreference = "Continue"

# AMS does not send a branch, so the server decides which one it runs.
$Branch = $env:AMS_DEPLOY_BRANCH
if (-not $Branch) { $Branch = "prog-shop-monitor-test-app" }

# The repository is public, so a fetch needs no credential and this script never
# handles one.
$OriginUrl = "https://github.com/JubaerKhan/ams-progcoder-shop.git"

# Taken from the script's own location rather than the working directory, so the
# paths are right however it is invoked.
$RepoDir = Split-Path -Parent $PSScriptRoot
$ComposeDir = Join-Path $RepoDir "progcoder-shop"

$MonitorTouched = $false

function Note($text) { Write-Host $text }

function Fail($text) {
    Write-Host ""
    Write-Host "DEPLOY FAILED: $text"
    Write-Host "result=failed"
    exit 1
}

function Step($what, $exe, $arguments) {
    Note ""
    Note "==> $what"
    & $exe @arguments
    if ($LASTEXITCODE -ne 0) { Fail "$what exited $LASTEXITCODE." }
}

# --- Context ---------------------------------------------------------------
#
# Printed first for a human reading the log, and printed again at the end. AMS
# keeps only the last 20 KB of output, and a Docker build is easily longer than
# that, so anything worth reading has to appear after the build as well.

Note "machine    : $env:COMPUTERNAME"
Note "repository : $RepoDir"
Note "branch     : $Branch"
Note "deploy id  : $env:AMS_DEPLOY_ID"
Note "fix id     : $env:AMS_REMEDIATION_ID"
Note "started    : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

# --- Checks before anything changes ----------------------------------------

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Fail "git is not on PATH for the account the agent runs as."
}
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Fail "docker is not on PATH for the account the agent runs as."
}
if (-not (Test-Path (Join-Path $RepoDir ".git"))) {
    Fail "$RepoDir is not a git repository."
}
if (-not (Test-Path $ComposeDir)) {
    Fail "$ComposeDir does not exist, so there is no compose stack to deploy."
}

Set-Location $RepoDir

# --- Get the code ----------------------------------------------------------

# This checkout was created to receive pushes, so it may have no remote at all.
$remotes = & git remote
if ($LASTEXITCODE -ne 0) { Fail "git remote failed in $RepoDir." }
if ($remotes -notcontains "origin") {
    Note ""
    Note "This checkout has no 'origin' remote. Adding $OriginUrl"
    Step "git remote add origin" "git" @("remote", "add", "origin", $OriginUrl)
}

# Empty on a repository with no commits yet, which is how this checkout starts.
$before = & git rev-parse HEAD
if ($LASTEXITCODE -ne 0) { $before = "" }

Step "Fetching origin/$Branch" "git" @("fetch", "--prune", "origin", $Branch)

# --hard, and deliberately no `git clean`. The two .env files this stack needs
# are gitignored, so they are untracked: a reset leaves them alone, and a clean
# would delete them and break the next build for no visible reason.
Step "Moving the work-tree to origin/$Branch" "git" @("reset", "--hard", "origin/$Branch")

$after = & git rev-parse HEAD
if ($LASTEXITCODE -ne 0) { Fail "Could not read HEAD after the reset." }
$subject = & git log -1 --pretty=%s

Note ""
Note "commit     : $($after.Substring(0,7))  $subject"
if ($before -eq $after) {
    Note "The checkout was already at this commit. Rebuilding it anyway, because"
    Note "a previous deploy may have failed part way through."
} elseif ($before) {
    $changed = @(& git diff --name-only $before $after)
    Note "changed    : $($changed.Count) file(s) since the last deploy"
    $MonitorTouched = @($changed | Where-Object { $_ -like "monitor/*" }).Count -gt 0
}

# --- Build and start -------------------------------------------------------

Set-Location $ComposeDir

$envFile = Join-Path $ComposeDir ".env"
if (-not (Test-Path $envFile)) {
    # The same fallback the post-receive hook uses. .env never travels with the
    # code, and the build needs values to interpolate.
    Note ""
    Note "No .env in progcoder-shop. Seeding it from .env.sample."
    Copy-Item (Join-Path $ComposeDir ".env.sample") $envFile
    Note "WARNING: the sample points APP_ORIGIN and the VITE_* addresses at"
    Note "         localhost. The stack will start, but the SPA will fail on CORS"
    Note "         when browsed by LAN address until .env is corrected and"
    Note "         app-admin is rebuilt."
}

# --build, because app-admin is a static SPA whose addresses Vite compiles into
# the bundle. A restart of the old image serves the old code.
Step "docker compose up --build -d" "docker" `
    @("compose", "-f", "docker-compose.yml", "up", "--build", "-d")

Note ""
Note "==> Container state"
& docker compose -f docker-compose.yml ps

# --- Say what happened -----------------------------------------------------

Note ""
Note "--- summary ---"
Note "machine        : $env:COMPUTERNAME"
Note "branch         : $Branch"
Note "commit subject : $subject"
Note "finished       : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

if ($MonitorTouched) {
    Note ""
    Note "ACTION NEEDED: this commit changed files under monitor/. The monitor is"
    Note "a host process, not a container, so nothing here restarted it. On this"
    Note "server, run:"
    Note "  cd `"$RepoDir\monitor`" ; .\stop-monitor.cmd ; .\run-monitor.cmd"
}

# Machine-readable, and last. AMS keeps the tail of this output, so a line it
# may want to read later has to be at the end.
Note ""
Note "AMS_DEPLOYED_SHA=$after"
Note "result=succeeded"
exit 0
