#!/bin/bash

set -e

run_sudo() {
    if [ -n "$SUDO_PASSWORD" ]; then
        echo "$SUDO_PASSWORD" | sudo -S "$@"
    else
        sudo "$@"
    fi
}

echo "🔄 Starting system update..."
run_sudo apt-get update && run_sudo apt-get upgrade -y

echo "📦 Installing dependencies..."
run_sudo apt-get install -y ca-certificates curl gnupg lsb-release apt-transport-https software-properties-common

echo "🔑 Adding Docker GPG key..."
run_sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg | run_sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
run_sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo "📋 Adding Docker repository..."
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | run_sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

echo "🔄 Updating package list with Docker repository..."
run_sudo apt-get update

echo "🐳 Installing Docker and Docker Compose..."
run_sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

echo "🔄 Starting Docker service..."
run_sudo systemctl enable docker
run_sudo systemctl start docker

echo "👤 Adding user to docker group..."
run_sudo usermod -aG docker $USER

echo "⏹️ Stopping and disabling nginx..."
run_sudo systemctl stop nginx || true
run_sudo systemctl disable nginx || true

echo "✅ Installation completed! Checking versions..."

export PATH="/usr/bin:/usr/local/bin:$PATH"
hash -r

echo "PATH is: $PATH"
which docker || echo "❌ 'docker' not in PATH"
ls -l /usr/bin/docker* || echo "❌ 'docker' not found in /usr/bin/"

if command -v docker &> /dev/null; then
    echo "✅ Docker version via command:"
    sudo docker --version
elif [ -x "/usr/bin/docker" ]; then
    echo "✅ Docker version via /usr/bin/docker:"
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
    echo "✅ Nginx version (via full path):"
    /usr/sbin/nginx -v
else
    echo "⚠️ Nginx not found - checking installation..."
    run_sudo apt-get install -y nginx
    if [ -x "/usr/sbin/nginx" ]; then
        echo "✅ Nginx installed successfully:"
        /usr/sbin/nginx -v
    else
        echo "❌ Nginx installation failed"
    fi
fi

echo "🎉 All services installed successfully!"
echo "⚠️ You may need to log out and back in for docker group changes to take effect."
