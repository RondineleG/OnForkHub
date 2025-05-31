#!/bin/bash

 apt-get update &&  apt-get upgrade -y

 apt-get install -y ca-certificates curl gnupg lsb-release nginx

 mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg |  gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" |  tee /etc/apt/sources.list.d/docker.list > /dev/null

 apt-get update
 apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

 usermod -aG docker $USER

 systemctl stop nginx
 systemctl disable nginx

docker --version
docker compose version
nginx -v
