#!/bin/bash

echo "Starting complete cleanup..."

# Verificar existência de arquivos de configuração do Docker Compose
function cleanup_compose_file() {
    local file=$1
    if [ -f "$file" ]; then
        echo "Stopping services defined in $file..."
        docker compose -f "$file" down --remove-orphans 2>/dev/null || true
    else
        echo "Warning: $file not found, skipping..."
    fi
}

# Parar serviços definidos nos arquivos de configuração
cleanup_compose_file "services.yml"
cleanup_compose_file "proxy.yml"

# Forçar remoção de um container específico
echo "Removing reverse-proxy container..."
docker rm -f reverse-proxy 2>/dev/null || echo "No reverse-proxy container found."

# Parar e remover todos os containers
echo "Stopping and removing all containers..."
docker ps -aq | xargs -r docker stop
docker ps -aq | xargs -r docker rm -f

# Remover todas as imagens
echo "Removing all images..."
docker images -q | xargs -r docker rmi -f

# Remover todas as redes não utilizadas
echo "Removing unused networks..."
docker network prune -f

# Remover volumes não utilizados
echo "Removing unused volumes..."
docker volume prune -f

# Realizar uma limpeza completa do sistema
echo "Cleaning up system..."
docker system prune -f --volumes

echo "Process completed!"
