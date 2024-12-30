#!/bin/bash

echo "Starting development environment setup..."

mkdir -p logs/nginx logs/onforkhub-api logs/onforkhub-web

check_disk_space() {
    local space=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$space" -gt 85 ]; then
        echo "WARNING: Disk space is above 85% ($space%)"
        echo "Running cleanup..."
        ./docker-cleanup.sh
    fi
}

check_disk_space

echo "Stopping existing services..."
docker compose down --remove-orphans

echo "Pruning unused Docker resources..."
docker system prune -f

echo "Starting all services..."
docker compose up -d

echo "Waiting for services to start..."
sleep 3

echo "Checking container status..."
docker ps

echo "Checking container logs..."
docker logs onforkhub-api --tail 10
docker logs onforkhub-web --tail 10
docker logs reverse-proxy --tail 10

echo "Development environment is ready!"