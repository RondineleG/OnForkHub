#!/bin/bash

echo "Starting complete cleanup..."

# Stop all services with --remove-orphans
echo "Stopping services with orphan removal..."
docker compose -f services.yml down --remove-orphans
docker compose -f proxy.yml down --remove-orphans

# Force removal of the reverse-proxy container specifically
echo "Removing reverse-proxy container..."
docker rm -f reverse-proxy 2>/dev/null || true

# Stop and remove all containers
echo "Stopping and removing all containers..."
docker ps -aq | xargs -r docker stop
docker ps -aq | xargs -r docker rm -f

# Remove all images
echo "Removing images..."
docker images -q | xargs -r docker rmi -f

# Remove all networks
echo "Removing networks..."
docker network prune -f

# Remove unused volumes
echo "Removing volumes..."
docker volume prune -f

# Remove everything that is "hanging"
echo "Cleaning up system..."
docker system prune -f --volumes

# Wait a moment to ensure everything is cleaned up
sleep 2

# Create a new network
echo "Creating new network..."
docker network create my-network

# Start the services with --remove-orphans
echo "Starting services..."
docker compose -f proxy.yml up -d --remove-orphans
docker compose -f services.yml up -d --remove-orphans

echo "Checking container status..."
docker ps

echo "Process completed!"
