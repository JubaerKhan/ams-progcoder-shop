#!/usr/bin/env bash
# One-time host preparation for this stack on Linux.
#
# Loki and Tempo run as uid 10001 and write to bind-mounted directories.
# Docker creates a missing bind-mount source as root, so both containers fail
# with "permission denied" and restart forever. Docker Desktop never shows
# this, because its file sharing layer rewrites ownership.
#
# A throwaway root container does the chown, so no host sudo is needed.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

mkdir -p docker-volumes/loki docker-volumes/tempo

docker run --rm \
  -v "$PWD/docker-volumes:/v" \
  alpine:latest \
  sh -c 'chown -R 10001:10001 /v/loki /v/tempo && echo "ownership set: $(stat -c %u:%g /v/loki) /v/loki, $(stat -c %u:%g /v/tempo) /v/tempo"'
