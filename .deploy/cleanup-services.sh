#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
echo "Cleanup restricted to OnForkHub Compose services."
docker compose down --remove-orphans
echo "Other Docker containers, volumes, networks and images were not touched."
