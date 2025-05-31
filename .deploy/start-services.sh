#!/bin/bash

echo "Starting development environment setup..."

if ! command -v gh &> /dev/null; then
    echo "GitHub CLI is not installed. Please install it first."
    exit 1
fi

if ! gh auth status &> /dev/null; then
    echo "You are not authenticated with GitHub CLI. Please run 'gh auth login' first."
    exit 1
fi

GITHUB_USERNAME=$(gh api user --jq '.login')
GITHUB_TOKEN=$(gh auth token)

echo "Using GitHub account: $GITHUB_USERNAME"

 mkdir -p logs/nginx logs/onforkhub-api logs/onforkhub-web

check_disk_space() {
    local space=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$space" -gt 85 ]; then
        echo "WARNING: Disk space is above 85% ($space%)"
        echo "Running cleanup..."
         ./cleanup-services.sh
    fi
}

check_disk_space

echo "Logging into GitHub Container Registry..."
echo "$GITHUB_TOKEN" |  docker login ghcr.io -u "$GITHUB_USERNAME" --password-stdin

echo "Stopping existing services..."
 docker compose down --remove-orphans

echo "Pruning unused Docker resources..."
 docker system prune -f

echo "Starting all services..."
 docker compose pull
 docker compose up -d

echo "Waiting for services to start..."
sleep 5

echo "Checking container status..."
 docker ps

echo "Checking container logs..."
 docker logs onforkhub-api --tail 10 || true
 docker logs onforkhub-web --tail 10 || true
 docker logs reverse-proxy --tail 10 || true

echo "Environment is ready!"
