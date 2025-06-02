#!/bin/bash

echo "Starting development environment setup..."

# Aceita o token como parâmetro
GITHUB_TOKEN="$1"
GITHUB_USERNAME="$2"

if [ -z "$GITHUB_TOKEN" ] || [ -z "$GITHUB_USERNAME" ]; then
    echo "Usage: $0 <github_token> <github_username>"
    echo "Error: GitHub token and username are required"
    exit 1
fi

echo "Using GitHub account: $GITHUB_USERNAME"

# Criar diretórios de logs
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

if [ $? -ne 0 ]; then
    echo "Failed to login to GitHub Container Registry"
    exit 1
fi

echo "Stopping existing services..."
sudo docker compose down --remove-orphans

echo "Pruning unused Docker resources..."
sudo docker system prune -f

echo "Pulling latest images..."
sudo docker compose pull

if [ $? -ne 0 ]; then
    echo "Failed to pull Docker images"
    exit 1
fi

echo "Starting all services..."
sudo docker compose up -d

if [ $? -ne 0 ]; then
    echo "Failed to start services"
    exit 1
fi

echo "Waiting for services to start..."
sleep 10

echo "Checking container status..."
sudo docker ps

echo "Checking if containers are healthy..."
if ! sudo docker ps | grep -q "onforkhub-api"; then
    echo "WARNING: onforkhub-api container is not running"
fi

if ! sudo docker ps | grep -q "onforkhub-web"; then
    echo "WARNING: onforkhub-web container is not running"
fi

if ! sudo docker ps | grep -q "reverse-proxy"; then
    echo "WARNING: reverse-proxy container is not running"
fi

echo "Checking container logs..."
echo "=== API Logs ==="
sudo docker logs onforkhub-api --tail 10 || echo "Failed to get API logs"

echo "=== Web Logs ==="
sudo docker logs onforkhub-web --tail 10 || echo "Failed to get Web logs"

echo "=== Proxy Logs ==="
sudo docker logs reverse-proxy --tail 10 || echo "Failed to get Proxy logs"

echo "Environment is ready!"
