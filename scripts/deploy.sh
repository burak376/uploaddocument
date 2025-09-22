#!/bin/bash

# Production Deployment Script

set -e

# Configuration
PROJECT_DIR="/var/www/document-management"
API_DIR="$PROJECT_DIR/api"
FRONTEND_DIR="$PROJECT_DIR/frontend"
BACKUP_DIR="/backups/deployment"
SERVICE_NAME="document-management"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "${GREEN}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log "Starting deployment process..."

# Check if running as root or with sudo
if [[ $EUID -eq 0 ]]; then
    error "This script should not be run as root for security reasons"
fi

# Create backup of current deployment
log "Creating backup of current deployment..."
mkdir -p $BACKUP_DIR
BACKUP_NAME="backup_$(date +%Y%m%d_%H%M%S)"

if [ -d "$API_DIR" ]; then
    tar -czf "$BACKUP_DIR/${BACKUP_NAME}_api.tar.gz" -C "$API_DIR" .
    log "API backup created: ${BACKUP_NAME}_api.tar.gz"
fi

if [ -d "$FRONTEND_DIR" ]; then
    tar -czf "$BACKUP_DIR/${BACKUP_NAME}_frontend.tar.gz" -C "$FRONTEND_DIR" .
    log "Frontend backup created: ${BACKUP_NAME}_frontend.tar.gz"
fi

# Stop the service
log "Stopping application service..."
sudo systemctl stop $SERVICE_NAME || warning "Service was not running"

# Backend deployment
log "Deploying backend..."
if [ -f "DocumentManagementAPI/bin/Release/net8.0/publish.zip" ]; then
    mkdir -p $API_DIR
    unzip -o DocumentManagementAPI/bin/Release/net8.0/publish.zip -d $API_DIR
    
    # Set permissions
    sudo chown -R www-data:www-data $API_DIR
    sudo chmod -R 755 $API_DIR
    
    # Create uploads directory
    mkdir -p $API_DIR/uploads
    sudo chown -R www-data:www-data $API_DIR/uploads
    sudo chmod -R 755 $API_DIR/uploads
    
    log "Backend deployed successfully"
else
    error "Backend build not found. Please run 'dotnet publish -c Release' first"
fi

# Frontend deployment
log "Deploying frontend..."
if [ -d "dist" ]; then
    mkdir -p $FRONTEND_DIR
    cp -r dist/* $FRONTEND_DIR/
    
    # Set permissions
    sudo chown -R www-data:www-data $FRONTEND_DIR
    sudo chmod -R 755 $FRONTEND_DIR
    
    log "Frontend deployed successfully"
else
    error "Frontend build not found. Please run 'npm run build' first"
fi

# Database migration
log "Running database migrations..."
cd $API_DIR
sudo -u www-data dotnet DocumentManagementAPI.dll --migrate || warning "Migration failed or not needed"

# Start the service
log "Starting application service..."
sudo systemctl start $SERVICE_NAME
sudo systemctl enable $SERVICE_NAME

# Wait for service to start
sleep 5

# Health check
log "Performing health check..."
if curl -f http://localhost:5000/health > /dev/null 2>&1; then
    log "Health check passed - Application is running"
else
    error "Health check failed - Application may not be running correctly"
fi

# Reload nginx
log "Reloading nginx..."
sudo nginx -t && sudo systemctl reload nginx

log "Deployment completed successfully!"
log "Application is available at: https://yourdomain.com"
log "API is available at: https://api.yourdomain.com"

# Clean old backups (keep last 5)
log "Cleaning old deployment backups..."
cd $BACKUP_DIR
ls -t backup_*_api.tar.gz 2>/dev/null | tail -n +6 | xargs -r rm
ls -t backup_*_frontend.tar.gz 2>/dev/null | tail -n +6 | xargs -r rm

log "Deployment process completed!"