#!/bin/bash

# Quick Deploy Script for Oracle VPS
# Bu script projeyi hızlıca deploy etmek için kullanılır

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Configuration
PROJECT_DIR="/var/www/document-management"
SERVICE_NAME="document-management"
DOMAIN="yourdomain.com"

log() {
    echo -e "${GREEN}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1"
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

# Check if we're in the right directory
if [ ! -f "package.json" ] && [ ! -d "DocumentManagementAPI" ]; then
    error "Bu script proje ana dizininde çalıştırılmalıdır"
fi

log "Starting quick deployment..."

# Create backup of current deployment
if [ -d "$PROJECT_DIR/api" ]; then
    log "Creating backup of current deployment..."
    BACKUP_NAME="backup_$(date +%Y%m%d_%H%M%S)"
    mkdir -p /home/$USER/deployment-backups
    tar -czf "/home/$USER/deployment-backups/${BACKUP_NAME}.tar.gz" -C "$PROJECT_DIR" . 2>/dev/null || true
fi

# Stop service
log "Stopping application service..."
sudo systemctl stop $SERVICE_NAME || warning "Service was not running"

# Backend deployment
if [ -d "DocumentManagementAPI" ]; then
    log "Building and deploying backend..."
    cd DocumentManagementAPI
    
    # Restore packages
    dotnet restore
    
    # Build and publish
    dotnet publish -c Release -o $PROJECT_DIR/api
    
    cd ..
    
    # Set permissions
    sudo chown -R www-data:www-data $PROJECT_DIR/api
    sudo chmod -R 755 $PROJECT_DIR/api
    
    # Create necessary directories
    mkdir -p $PROJECT_DIR/api/uploads
    mkdir -p $PROJECT_DIR/api/logs
    sudo chown -R www-data:www-data $PROJECT_DIR/api/uploads
    sudo chown -R www-data:www-data $PROJECT_DIR/api/logs
    sudo chmod -R 777 $PROJECT_DIR/api/uploads
    sudo chmod -R 777 $PROJECT_DIR/api/logs
    
    log "Backend deployed successfully"
else
    warning "DocumentManagementAPI directory not found, skipping backend deployment"
fi

# Frontend deployment
if [ -f "package.json" ]; then
    log "Building and deploying frontend..."
    
    # Install dependencies
    npm install
    
    # Build for production
    npm run build
    
    # Deploy to nginx directory
    sudo mkdir -p /var/www/html/document-management
    sudo cp -r dist/* /var/www/html/document-management/
    
    # Set permissions
    sudo chown -R www-data:www-data /var/www/html/document-management
    sudo chmod -R 755 /var/www/html/document-management
    
    log "Frontend deployed successfully"
else
    warning "package.json not found, skipping frontend deployment"
fi

# Database migration (if needed)
if [ -d "$PROJECT_DIR/api" ]; then
    log "Running database migrations..."
    cd $PROJECT_DIR/api
    sudo -u www-data dotnet DocumentManagementAPI.dll --migrate || warning "Migration failed or not needed"
    cd -
fi

# Start service
log "Starting application service..."
sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
sudo systemctl start $SERVICE_NAME

# Wait for service to start
sleep 5

# Check service status
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    log "Service started successfully"
else
    error "Service failed to start. Check logs: sudo journalctl -u $SERVICE_NAME -n 20"
fi

# Test nginx configuration and reload
log "Reloading nginx..."
if sudo nginx -t; then
    sudo systemctl reload nginx
    log "Nginx reloaded successfully"
else
    error "Nginx configuration test failed"
fi

# Health check
log "Performing health check..."
sleep 3

if curl -f -s http://localhost:5000/health > /dev/null; then
    log "✅ Health check passed - API is responding"
else
    warning "⚠️  Health check failed - API may not be responding correctly"
fi

# Check if frontend is accessible
if curl -f -s http://localhost > /dev/null; then
    log "✅ Frontend is accessible"
else
    warning "⚠️  Frontend may not be accessible"
fi

log "🚀 Deployment completed!"
log ""
log "📊 Service Status:"
sudo systemctl status $SERVICE_NAME --no-pager -l

log ""
log "🌐 Access URLs:"
log "  Frontend: http://$(curl -s ifconfig.me)"
log "  API: http://$(curl -s ifconfig.me)/api"

if [ "$DOMAIN" != "yourdomain.com" ]; then
    log "  Domain: https://$DOMAIN"
fi

log ""
log "📝 Useful commands:"
log "  Check logs: sudo journalctl -u $SERVICE_NAME -f"
log "  Restart service: sudo systemctl restart $SERVICE_NAME"
log "  Check nginx: sudo nginx -t"
log "  View nginx logs: sudo tail -f /var/log/nginx/error.log"

# Clean old deployment backups (keep last 5)
log "Cleaning old deployment backups..."
cd /home/$USER/deployment-backups 2>/dev/null || true
ls -t backup_*.tar.gz 2>/dev/null | tail -n +6 | xargs -r rm || true

log "✨ Deployment process completed successfully!"