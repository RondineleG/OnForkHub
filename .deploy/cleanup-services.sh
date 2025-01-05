#!/bin/bash

echo "Starting complete cleanup..."

echo "Stopping all containers..."
docker compose down --remove-orphans

echo "Removing all containers..."
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
