#!/bin/bash

echo "Starting complete cleanup..."

# Check for existence of Docker Compose configuration files
function cleanup_compose_file() {
    local file=$1
    if [ -f "$file" ]; then
        echo "Stopping services defined in $file..."
        docker compose -f "$file" down --remove-orphans 2>/dev/null || true
    else
        echo "Warning: $file not found, skipping..."
    fi
}

# Stop services defined in configuration files
cleanup_compose_file "services.yml"
cleanup_compose_file "proxy.yml"

echo "Removing reverse-proxy container..."
docker rm -f reverse-proxy 2>/dev/null || echo "No reverse-proxy container found."

echo "Stopping and removing all containers..."
docker ps -aq | xargs -r docker stop
docker ps -aq | xargs -r docker rm -f

echo "Removing all images..."
docker images -q | xargs -r docker rmi -f

echo "Removing unused networks..."
docker network prune -f

echo "Removing unused volumes..."
docker volume prune -f

echo "Cleaning up system..."
docker system prune -f --volumes

echo "Process completed!"
