set -e

echo "🔄 Starting HTTPS environment setup..."

GITHUB_TOKEN="$1"
GITHUB_USERNAME="$2"

if [ -z "$GITHUB_TOKEN" ] || [ -z "$GITHUB_USERNAME" ]; then
    echo "Usage: $0 <github_token> <github_username>"
    echo "Error: GitHub token and username are required"
    exit 1
fi

echo "Using GitHub account: $GITHUB_USERNAME"

echo "🔐 Checking SSL certificates..."
if [ ! -f "/etc/ssl/zerossl/fullchain.crt" ] || [ ! -f "/etc/ssl/zerossl/private.key" ]; then
    echo "❌ SSL certificates not found!"
    echo "Please ensure certificates are installed at:"
    echo "  - /etc/ssl/zerossl/fullchain.crt"
    echo "  - /etc/ssl/zerossl/private.key"
    exit 1
fi

echo "✅ SSL certificates found"

echo "🔍 Checking certificate validity..."
if openssl x509 -in /etc/ssl/zerossl/fullchain.crt -checkend 86400 -noout > /dev/null; then
    echo "✅ Certificate is valid"
    CERT_EXPIRY=$(openssl x509 -in /etc/ssl/zerossl/fullchain.crt -enddate -noout | cut -d= -f2)
    echo "📅 Certificate expires: $CERT_EXPIRY"
else
    echo "⚠️ Certificate expires within 24 hours or is invalid!"
    echo "Consider renewing your certificate"
fi

sudo mkdir -p logs/nginx logs/onforkhub-api logs/onforkhub-web

check_disk_space() {
    local space=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$space" -gt 85 ]; then
        echo "WARNING: Disk space is above 85% ($space%)"
        echo "Running cleanup..."
        sudo ./cleanup-services.sh
    fi
}

check_disk_space

echo "🔑 Logging into GitHub Container Registry..."
echo "$GITHUB_TOKEN" | sudo docker login ghcr.io -u "$GITHUB_USERNAME" --password-stdin

if [ $? -ne 0 ]; then
    echo "❌ Failed to login to GitHub Container Registry"
    exit 1
fi

echo "⏹️ Stopping existing services..."
sudo docker compose down --remove-orphans

echo "🧹 Pruning unused Docker resources..."
sudo docker system prune -f

echo "📥 Pulling latest images..."
sudo docker compose pull

if [ $? -ne 0 ]; then
    echo "❌ Failed to pull Docker images"
    exit 1
fi

echo "🚀 Starting all services with HTTPS..."
sudo docker compose up -d

if [ $? -ne 0 ]; then
    echo "❌ Failed to start services"
    exit 1
fi

echo "⏳ Waiting for services to start..."
sleep 15

echo "📋 Checking container status..."
sudo docker ps

echo "🔍 Checking if containers are healthy..."
if ! sudo docker ps | grep -q "onforkhub-api"; then
    echo "⚠️ WARNING: onforkhub-api container is not running"
fi

if ! sudo docker ps | grep -q "onforkhub-web"; then
    echo "⚠️ WARNING: onforkhub-web container is not running"
fi

if ! sudo docker ps | grep -q "reverse-proxy"; then
    echo "⚠️ WARNING: reverse-proxy container is not running"
fi

echo "📝 Checking container logs..."
echo "=== API Logs ==="
sudo docker logs onforkhub-api --tail 10 || echo "Failed to get API logs"

echo "=== Web Logs ==="
sudo docker logs onforkhub-web --tail 10 || echo "Failed to get Web logs"

echo "=== Proxy Logs ==="
sudo docker logs reverse-proxy --tail 10 || echo "Failed to get Proxy logs"

echo "🌐 Testing HTTPS endpoints..."
echo "Testing Web HTTPS..."
if curl -k -s -o /dev/null -w "%{http_code}" https://172.245.152.43 | grep -q "200"; then
    echo "✅ Web HTTPS is working"
else
    echo "⚠️ Web HTTPS test failed"
fi

echo "Testing API HTTPS..."
if curl -k -s -o /dev/null -w "%{http_code}" https://172.245.152.43:9443/health | grep -q "200"; then
    echo "✅ API HTTPS is working"
else
    echo "⚠️ API HTTPS test failed"
fi

echo "🎉 HTTPS Environment is ready!"
echo ""
echo "🔗 Access URLs:"
echo "  Web (HTTPS): https://172.245.152.43"
echo "  API (HTTPS): https://172.245.152.43:9443"
echo ""
echo "📊 SSL Certificate Status:"
openssl x509 -in /etc/ssl/zerossl/fullchain.crt -subject -dates -noout
