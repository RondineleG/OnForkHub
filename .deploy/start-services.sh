#!/bin/bash

set -e

REGISTRY_TOKEN="$1"
GITHUB_USERNAME="$2"

if [ -z "$REGISTRY_TOKEN" ] || [ -z "$GITHUB_USERNAME" ]; then
    echo "❌ Usage: $0 <registry_token> <github_username>"
    exit 1
fi

GITHUB_USERNAME_LOWER=$(echo "$GITHUB_USERNAME" | tr '[:upper:]' '[:lower:]')

echo "🔐 Logging into GitHub Container Registry..."
echo "$REGISTRY_TOKEN" | docker login ghcr.io -u "$GITHUB_USERNAME" --password-stdin

echo "📥 Pulling latest images..."
docker pull ghcr.io/${GITHUB_USERNAME_LOWER}/onforkhub-api:latest || {
    echo "❌ Failed to pull API image"
    exit 1
}

docker pull ghcr.io/${GITHUB_USERNAME_LOWER}/onforkhub-web:latest || {
    echo "❌ Failed to pull Web image"
    exit 1
}

echo "🚀 Starting all services with HTTPS..."
docker compose up -d

echo "⏳ Waiting for services to be healthy..."
for i in {1..20}; do
    if docker compose ps | grep -q "healthy"; then
        echo "✅ Services are starting up..."
        break
    fi
    echo "Attempt $i/20 - Waiting for health checks..."
    sleep 10
done

echo "📋 Final container status:"
docker compose ps

echo "📝 API container logs:"
docker logs onforkhub-api --tail 20 || echo "API container not running"

echo "📝 Web container logs:"
docker logs onforkhub-web --tail 20 || echo "Web container not running"

echo "📝 Reverse proxy logs:"
docker logs reverse-proxy --tail 20 || echo "Reverse proxy container not running"

echo "✅ Deployment completed!"
