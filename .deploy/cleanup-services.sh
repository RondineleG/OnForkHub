#!/bin/bash
set -e

echo "Starting complete cleanup..."

echo "Stopping all containers..."
docker compose down --remove-orphans || true

echo "Removing all containers..."
docker ps -aq | xargs -r docker stop || true
docker ps -aq | xargs -r docker rm -f || true

echo "Removing all images..."
docker images -q | xargs -r docker rmi -f || true

echo "Removing unused networks..."
docker network prune -f || true

echo "Removing unused volumes..."
docker volume prune -f || true

echo "Cleaning up system..."
docker system prune -f --volumes || true

echo "Process completed!"
