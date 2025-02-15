#!/bin/bash

if [ $# -ne 2 ]; then
    echo "Usage: $0 <github_token> <github_username>"
    exit 1
fi

GITHUB_TOKEN=$1
GITHUB_USERNAME=$2

echo "Starting development environment setup..."

sudo mkdir -p logs/nginx logs/onforkhub-api logs/onforkhub-web

check_disk_space() {
    local space=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$space" -gt 85 ]; then
        echo "WARNING: Disk space is above 85% ($space%)"
        echo "Running cleanup..."
        sudo ./cleanup-services.sh
    fi
}

check_disk_space

echo "Logging into GitHub Container Registry..."
echo "$GITHUB_TOKEN" | sudo docker login ghcr.io -u "$GITHUB_USERNAME" --password-stdin

echo "Stopping existing services..."
sudo docker compose down --remove-orphans

echo "Pruning unused Docker resources..."
sudo docker system prune -f

echo "Starting all services..."
sudo docker compose pull
sudo docker compose up -d

echo "Waiting for services to start..."
sleep 5

echo "Checking container status..."
sudo docker ps

echo "Checking container logs..."
sudo docker logs onforkhub-api --tail 10 || true
sudo docker logs onforkhub-web --tail 10 || true
sudo docker logs reverse-proxy --tail 10 || true

echo "Development environment is ready!"
