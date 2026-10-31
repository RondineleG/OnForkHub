#!/bin/bash

set -e

REGISTRY_TOKEN="$1"
GITHUB_USERNAME="$2"

if [ -z "$REGISTRY_TOKEN" ] || [ -z "$GITHUB_USERNAME" ]; then
    echo "❌ Usage: $0 <registry_token> <github_username>"
    exit 1
fi

# Converter username para minúsculo
GITHUB_USERNAME_LOWER=$(echo "$GITHUB_USERNAME" | tr '[:upper:]' '[:lower:]')

echo "🔍 Comprehensive port and process diagnosis..."

diagnose_ports() {
    echo "=== Port 80 Analysis ==="
    sudo lsof -i :80 2>/dev/null || echo "No lsof results for port 80"
    sudo netstat -tlnp | grep :80 || echo "No netstat results for port 80"
    sudo ss -tlnp | grep :80 || echo "No ss results for port 80"
    
    echo "=== Port 443 Analysis ==="
    sudo lsof -i :443 2>/dev/null || echo "No lsof results for port 443"
    sudo netstat -tlnp | grep :443 || echo "No netstat results for port 443"
    
    echo "=== Port 9443 Analysis ==="
    sudo lsof -i :9443 2>/dev/null || echo "No lsof results for port 9443"
    sudo netstat -tlnp | grep :9443 || echo "No netstat results for port 9443"
    
    echo "=== Docker Network Analysis ==="
    docker network ls
    docker ps -a
}


echo "🔍 Initial diagnosis..."
diagnose_ports

echo 'Skipping destructive host-wide cleanup; managing only OnForkHub services.'

echo "🔍 Post-cleanup diagnosis..."
diagnose_ports

echo "🔐 Logging into GitHub Container Registry..."
echo "$REGISTRY_TOKEN" | docker login ghcr.io -u "$GITHUB_USERNAME" --password-stdin

echo "📥 Pulling latest images..."
docker compose pull onforkhub-api || {
    echo "❌ Failed to pull API image"
    exit 1
}

docker compose pull onforkhub-web || {
    echo "❌ Failed to pull Web image"
    exit 1
}


echo "🚀 Starting all services with HTTPS..."
docker compose up -d --wait

echo "⏳ Waiting for services to start..."
sleep 30

echo "📋 Container status:"
docker compose ps

echo "📝 Container logs:"
echo "=== API Logs ==="
docker logs onforkhub-api --tail 20 || echo "API container not running"
echo "=== Web Logs ==="
docker logs onforkhub-web --tail 20 || echo "Web container not running"
echo "=== Reverse Proxy Logs ==="
docker logs reverse-proxy --tail 20 || echo "Reverse proxy container not running"

echo "✅ Deployment script completed!"
