#!/bin/bash

echo "Starting complete cleanup..."

echo "Stopping all containers..."
sudo docker compose down --remove-orphans

echo "Removing all containers..."
sudo docker ps -aq | xargs -r sudo docker stop
sudo docker ps -aq | xargs -r sudo docker rm -f

echo "Removing all images..."
sudo docker images -q | xargs -r sudo docker rmi -f

echo "Removing unused networks..."
sudo docker network prune -f

echo "Removing unused volumes..."
sudo docker volume prune -f

echo "Cleaning up system..."
sudo docker system prune -f --volumes

echo "Process completed!"