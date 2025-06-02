#!/bin/bash
set -e

# Helper to run sudo with or without password
run_sudo() {
    if [ -n "$SUDO_PASSWORD" ]; then
        echo "$SUDO_PASSWORD" | sudo -S "$@"
    else
        sudo "$@"
    fi
}

echo "🔄 Updating system packages..."
run_sudo apt-get update && run_sudo apt-get upgrade -y

echo "📦 Installing dependencies..."
run_sudo apt-get install -y ca-certificates curl gnupg lsb-release apt-transport-https software-properties-common

echo "🧹 Removing old Docker installations if any..."
run_sudo apt-get remove -y docker docker-engine docker.io containerd runc || true

echo "🔑 Adding Docker's official GPG key..."
run_sudo install -m 0755 -d /etc/apt/keyrings

# Download and install Docker's GPG key with better error handling
curl -fsSL https://download.docker.com/linux/debian/gpg | run_sudo tee /etc/apt/keyrings/docker.asc > /dev/null
run_sudo chmod a+r /etc/apt/keyrings/docker.asc

echo "📋 Setting up Docker repository..."
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/debian \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  run_sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

echo "🔄 Updating package list with Docker repo..."
run_sudo apt-get update

echo "🐳 Installing Docker engine and Compose plugin..."
run_sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo "🔄 Enabling and starting Docker service..."
run_sudo systemctl enable docker
run_sudo systemctl start docker

echo "👤 Adding current user to docker group (applies after new login)..."
run_sudo usermod -aG docker $USER

echo "⏹️ Stopping and disabling nginx (if running)..."
run_sudo systemctl stop nginx || true
run_sudo systemctl disable nginx || true

echo "✅ Installation completed! Printing debug info and versions..."
export PATH="/usr/bin:/usr/local/bin:$PATH"
hash -r

echo "PATH is: $PATH"
echo "which docker: $(which docker || echo 'not in current PATH')"
ls -l /usr/bin/docker* || echo "'docker' binary not found in /usr/bin/"

if command -v docker &> /dev/null; then
    echo "✅ Docker version (command):"
    sudo docker --version
elif [ -x "/usr/bin/docker" ]; then
    echo "✅ Docker version (/usr/bin/docker):"
    sudo /usr/bin/docker --version
else
    echo "❌ Docker installation failed - command not found"
    run_sudo systemctl status docker || echo "Docker service not found"
    exit 1
fi

if sudo docker compose version &> /dev/null; then
    echo "✅ Docker Compose version:"
    sudo docker compose version
else
    echo "❌ Docker Compose not available"
    exit 1
fi

if command -v nginx &> /dev/null; then
    echo "✅ Nginx version:"
    nginx -v
elif [ -x "/usr/sbin/nginx" ]; then
    echo "✅ Nginx version (/usr/sbin/nginx):"
    /usr/sbin/nginx -v
else
    echo "⚠️ Nginx not found - installing..."
    run_sudo apt-get install -y nginx
    if [ -x "/usr/sbin/nginx" ]; then
        echo "✅ Nginx installed successfully:"
        /usr/sbin/nginx -v
    else
        echo "❌ Nginx installation failed"
    fi
fi

echo "🎉 All services installed successfully!"
echo "⚠️ IMPORTANT: You must log out and log in again for docker group membership to take effect for your user."
