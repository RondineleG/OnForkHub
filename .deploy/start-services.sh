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

aggressive_cleanup() {
    echo "🔥 Starting aggressive cleanup..."
    
    echo "Stopping all containers..."
    docker stop $(docker ps -q) 2>/dev/null || true
    
    echo "Removing all containers..."
    docker rm -f $(docker ps -aq) 2>/dev/null || true
    
    echo "Cleaning networks..."
    docker network prune -f
    
    echo "Cleaning volumes..."
    docker volume prune -f
    
    echo "Stopping system web services..."
    sudo systemctl stop apache2 2>/dev/null || true
    sudo systemctl stop nginx 2>/dev/null || true
    sudo systemctl stop httpd 2>/dev/null || true
    sudo systemctl disable apache2 2>/dev/null || true
    sudo systemctl disable nginx 2>/dev/null || true
    sudo systemctl disable httpd 2>/dev/null || true
    
    echo "Killing processes on ports (attempt 1)..."
    sudo fuser -k 80/tcp 2>/dev/null || true
    sudo fuser -k 443/tcp 2>/dev/null || true
    sudo fuser -k 9443/tcp 2>/dev/null || true
    
    sleep 3
    
    echo "Killing processes on ports (attempt 2)..."
    sudo pkill -f ".*:80" 2>/dev/null || true
    sudo pkill -f ".*:443" 2>/dev/null || true
    sudo pkill -f ".*:9443" 2>/dev/null || true
    
    sleep 2
    
    echo "Killing docker-proxy processes..."
    sudo pkill -f docker-proxy 2>/dev/null || true
    
    echo "Checking Docker service..."
    if ! docker system info >/dev/null 2>&1; then
        echo "Restarting Docker service..."
        sudo systemctl restart docker
        sleep 10
    fi
    
    sleep 5
}

echo "🔍 Initial diagnosis..."
diagnose_ports

aggressive_cleanup

echo "🔍 Post-cleanup diagnosis..."
diagnose_ports

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


echo "🔍 Final port verification before starting..."
for port in 80 443 9443; do
    if sudo lsof -i :$port 2>/dev/null; then
        echo "❌ Port $port still in use! Trying to force kill..."
        PIDS=$(sudo lsof -t -i :$port 2>/dev/null || true)
        if [ ! -z "$PIDS" ]; then
            echo "Killing PIDs: $PIDS"
            sudo kill -9 $PIDS 2>/dev/null || true
            sleep 2
        fi
    fi
done


if sudo lsof -i :80 2>/dev/null; then
    echo "❌ CRITICAL: Port 80 is still occupied after cleanup!"
    echo "Process details:"
    sudo lsof -i :80
    echo "Attempting emergency Docker restart..."
    sudo systemctl restart docker
    sleep 15
    
   
    if sudo lsof -i :80 2>/dev/null; then
        echo "⚠️ Using alternative ports configuration..."
        
        sed -i 's/"80:80"/"8080:80"/g' docker-compose.yml
        sed -i 's/"443:443"/"8443:443"/g' docker-compose.yml
        sed -i 's/listen 80;/listen 8080;/g' custom.conf
        sed -i 's/listen 443/listen 8443/g' custom.conf
        echo "✅ Modified to use alternative ports: 8080 (HTTP), 8443 (HTTPS), 9443 (API)"
    fi
fi

echo "🚀 Starting all services with HTTPS..."
docker compose up -d --force-recreate

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
