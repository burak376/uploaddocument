#!/bin/bash

# Oracle VPS Ubuntu Setup Script
# Bu script Oracle Cloud VPS'te Document Management System kurulumu yapar

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="/var/www/document-management"
DB_NAME="DocumentManagementDB"
DB_USER="docuser"
DB_PASS="VeryStrongPassword123!"
DOMAIN="yourdomain.com"
EMAIL="your-email@domain.com"

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

info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

# Check if running as root
if [[ $EUID -eq 0 ]]; then
    error "This script should not be run as root for security reasons"
fi

log "Starting Oracle VPS Ubuntu setup for Document Management System..."

# Update system
log "Updating system packages..."
sudo apt update && sudo apt upgrade -y

# Install basic packages
log "Installing basic packages..."
sudo apt install -y curl wget git unzip software-properties-common apt-transport-https ca-certificates gnupg lsb-release htop

# Install .NET 8
log "Installing .NET 8 Runtime..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0 dotnet-sdk-8.0

# Verify .NET installation
if ! command -v dotnet &> /dev/null; then
    error ".NET installation failed"
fi
log ".NET $(dotnet --version) installed successfully"

# Install Node.js
log "Installing Node.js..."
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt install -y nodejs

# Verify Node.js installation
if ! command -v node &> /dev/null; then
    error "Node.js installation failed"
fi
log "Node.js $(node --version) installed successfully"

# Install Nginx
log "Installing Nginx..."
sudo apt install -y nginx
sudo systemctl start nginx
sudo systemctl enable nginx

# Install MySQL
log "Installing MySQL Server..."
sudo apt install -y mysql-server
sudo systemctl start mysql
sudo systemctl enable mysql

# Secure MySQL installation (automated)
log "Securing MySQL installation..."
sudo mysql -e "ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '${DB_PASS}';"
sudo mysql -u root -p${DB_PASS} -e "DELETE FROM mysql.user WHERE User='';"
sudo mysql -u root -p${DB_PASS} -e "DELETE FROM mysql.user WHERE User='root' AND Host NOT IN ('localhost', '127.0.0.1', '::1');"
sudo mysql -u root -p${DB_PASS} -e "DROP DATABASE IF EXISTS test;"
sudo mysql -u root -p${DB_PASS} -e "DELETE FROM mysql.db WHERE Db='test' OR Db='test\\_%';"
sudo mysql -u root -p${DB_PASS} -e "FLUSH PRIVILEGES;"

# Create database and user
log "Creating database and user..."
sudo mysql -u root -p${DB_PASS} -e "CREATE DATABASE ${DB_NAME} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
sudo mysql -u root -p${DB_PASS} -e "CREATE USER '${DB_USER}'@'localhost' IDENTIFIED BY '${DB_PASS}';"
sudo mysql -u root -p${DB_PASS} -e "GRANT ALL PRIVILEGES ON ${DB_NAME}.* TO '${DB_USER}'@'localhost';"
sudo mysql -u root -p${DB_PASS} -e "FLUSH PRIVILEGES;"

# Test database connection
if mysql -u ${DB_USER} -p${DB_PASS} -e "USE ${DB_NAME};" 2>/dev/null; then
    log "Database connection test successful"
else
    error "Database connection test failed"
fi

# Create project directory
log "Creating project directory..."
sudo mkdir -p ${PROJECT_DIR}
sudo chown -R $USER:$USER ${PROJECT_DIR}

# Setup firewall
log "Configuring UFW firewall..."
sudo apt install -y ufw
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable

# Install Fail2Ban
log "Installing Fail2Ban..."
sudo apt install -y fail2ban

# Create fail2ban configuration
sudo tee /etc/fail2ban/jail.local > /dev/null <<EOF
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5

[nginx-http-auth]
enabled = true

[nginx-limit-req]
enabled = true
filter = nginx-limit-req
action = iptables-multiport[name=ReqLimit, port="http,https", protocol=tcp]
logpath = /var/log/nginx/error.log
findtime = 600
bantime = 7200
maxretry = 10
EOF

sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Create swap file (2GB)
log "Creating swap file..."
if [ ! -f /swapfile ]; then
    sudo fallocate -l 2G /swapfile
    sudo chmod 600 /swapfile
    sudo mkswap /swapfile
    sudo swapon /swapfile
    echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
    log "2GB swap file created"
fi

# Create backup directory
log "Creating backup directory..."
mkdir -p /home/$USER/backups

# Create backup script
log "Creating backup script..."
tee /home/$USER/backup.sh > /dev/null <<EOF
#!/bin/bash

# Backup Configuration
DATE=\$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/home/$USER/backups"
DB_NAME="${DB_NAME}"
DB_USER="${DB_USER}"
DB_PASS="${DB_PASS}"
UPLOADS_DIR="${PROJECT_DIR}/api/uploads"

# Create backup directory
mkdir -p \$BACKUP_DIR

# Database backup
mysqldump -u\$DB_USER -p\$DB_PASS \$DB_NAME | gzip > \$BACKUP_DIR/db_\$DATE.sql.gz

# Files backup
if [ -d "\$UPLOADS_DIR" ]; then
    tar -czf \$BACKUP_DIR/files_\$DATE.tar.gz -C \$(dirname \$UPLOADS_DIR) \$(basename \$UPLOADS_DIR)
fi

# Clean old backups (keep 7 days)
find \$BACKUP_DIR -name "db_*.sql.gz" -mtime +7 -delete
find \$BACKUP_DIR -name "files_*.tar.gz" -mtime +7 -delete

echo "Backup completed: \$DATE"
EOF

chmod +x /home/$USER/backup.sh

# Create health check script
log "Creating health check script..."
tee /home/$USER/health-check.sh > /dev/null <<EOF
#!/bin/bash

# Health check
API_URL="http://localhost:5000"
SERVICE_NAME="document-management"

# Check API
if ! curl -f -s \$API_URL/health > /dev/null; then
    echo "API is down, restarting service..."
    sudo systemctl restart \$SERVICE_NAME
    
    # Wait and check again
    sleep 10
    if ! curl -f -s \$API_URL/health > /dev/null; then
        echo "Service restart failed!" | logger -t health-check
    fi
fi

# Check disk space
DISK_USAGE=\$(df / | tail -1 | awk '{print \$5}' | sed 's/%//')
if [ \$DISK_USAGE -gt 80 ]; then
    echo "Disk usage is \${DISK_USAGE}%" | logger -t health-check
fi
EOF

chmod +x /home/$USER/health-check.sh

# Install Certbot for SSL
log "Installing Certbot for SSL certificates..."
sudo apt install -y certbot python3-certbot-nginx

# Create systemd service template
log "Creating systemd service template..."
sudo tee /etc/systemd/system/document-management.service > /dev/null <<EOF
[Unit]
Description=Document Management API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet ${PROJECT_DIR}/api/DocumentManagementAPI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=document-management
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
WorkingDirectory=${PROJECT_DIR}/api

[Install]
WantedBy=multi-user.target
EOF

# Create nginx configuration template
log "Creating Nginx configuration template..."
sudo tee /etc/nginx/sites-available/document-management > /dev/null <<EOF
# HTTP to HTTPS redirect
server {
    listen 80;
    server_name ${DOMAIN} www.${DOMAIN};
    return 301 https://\$server_name\$request_uri;
}

# HTTPS Configuration
server {
    listen 443 ssl http2;
    server_name ${DOMAIN} www.${DOMAIN};
    
    # SSL Configuration (will be configured by Certbot)
    # ssl_certificate /etc/letsencrypt/live/${DOMAIN}/fullchain.pem;
    # ssl_certificate_key /etc/letsencrypt/live/${DOMAIN}/privkey.pem;
    
    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Frame-Options DENY always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    # File upload size
    client_max_body_size 100M;
    
    # Frontend (React App)
    location / {
        root /var/www/html/document-management;
        index index.html;
        try_files \$uri \$uri/ /index.html;
        
        # Cache static assets
        location ~* \\.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)\$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
    
    # API Backend
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # File upload endpoint (extended timeout)
    location /api/documents/upload {
        proxy_pass http://localhost:5000/api/documents/upload;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # Extended timeouts for file uploads
        proxy_connect_timeout 300s;
        proxy_send_timeout 300s;
        proxy_read_timeout 300s;
        client_max_body_size 100M;
    }
}

# Temporary HTTP configuration for initial setup
server {
    listen 80;
    server_name \$(curl -s ifconfig.me);
    
    location / {
        root /var/www/html/document-management;
        index index.html;
        try_files \$uri \$uri/ /index.html;
    }
    
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

# Create production appsettings template
log "Creating production appsettings template..."
mkdir -p ${PROJECT_DIR}/api
tee ${PROJECT_DIR}/api/appsettings.Production.json > /dev/null <<EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=${DB_NAME};User=${DB_USER};Password=${DB_PASS};"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secure-production-secret-key-at-least-32-characters-long-change-this-now",
    "Issuer": "DocumentManagementAPI",
    "Audience": "DocumentManagementClient",
    "ExpiryMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "${DOMAIN},www.${DOMAIN}"
}
EOF

# Create deployment script
log "Creating deployment script..."
tee /home/$USER/deploy.sh > /dev/null <<EOF
#!/bin/bash

set -e

PROJECT_DIR="${PROJECT_DIR}"
SERVICE_NAME="document-management"

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] \$1"
}

log "Starting deployment..."

# Stop service
sudo systemctl stop \$SERVICE_NAME || true

# Backend deployment
if [ -d "DocumentManagementAPI" ]; then
    log "Deploying backend..."
    cd DocumentManagementAPI
    dotnet restore
    dotnet publish -c Release -o \$PROJECT_DIR/api
    cd ..
    
    # Set permissions
    sudo chown -R www-data:www-data \$PROJECT_DIR/api
    sudo chmod -R 755 \$PROJECT_DIR/api
    
    # Create directories
    mkdir -p \$PROJECT_DIR/api/uploads
    mkdir -p \$PROJECT_DIR/api/logs
    sudo chown -R www-data:www-data \$PROJECT_DIR/api/uploads
    sudo chown -R www-data:www-data \$PROJECT_DIR/api/logs
    sudo chmod -R 777 \$PROJECT_DIR/api/uploads
    sudo chmod -R 777 \$PROJECT_DIR/api/logs
fi

# Frontend deployment
if [ -d "dist" ]; then
    log "Deploying frontend..."
    sudo mkdir -p /var/www/html/document-management
    sudo cp -r dist/* /var/www/html/document-management/
    sudo chown -R www-data:www-data /var/www/html/document-management
    sudo chmod -R 755 /var/www/html/document-management
elif [ -f "package.json" ]; then
    log "Building and deploying frontend..."
    npm install
    npm run build
    sudo mkdir -p /var/www/html/document-management
    sudo cp -r dist/* /var/www/html/document-management/
    sudo chown -R www-data:www-data /var/www/html/document-management
    sudo chmod -R 755 /var/www/html/document-management
fi

# Start service
sudo systemctl daemon-reload
sudo systemctl enable \$SERVICE_NAME
sudo systemctl start \$SERVICE_NAME

# Reload nginx
sudo nginx -t && sudo systemctl reload nginx

log "Deployment completed!"
EOF

chmod +x /home/$USER/deploy.sh

# Setup cron jobs
log "Setting up cron jobs..."
(crontab -l 2>/dev/null; echo "0 2 * * * /home/$USER/backup.sh >> /home/$USER/backup.log 2>&1") | crontab -
(crontab -l 2>/dev/null; echo "*/5 * * * * /home/$USER/health-check.sh") | crontab -

log "Oracle VPS setup completed successfully!"
info ""
info "Next steps:"
info "1. Upload your project files to ${PROJECT_DIR}"
info "2. Run the deployment script: ./deploy.sh"
info "3. Configure your domain DNS to point to this server"
info "4. Run: sudo certbot --nginx -d ${DOMAIN} -d www.${DOMAIN}"
info "5. Test your application"
info ""
info "Important files created:"
info "- Backup script: /home/$USER/backup.sh"
info "- Health check: /home/$USER/health-check.sh"
info "- Deploy script: /home/$USER/deploy.sh"
info "- Nginx config: /etc/nginx/sites-available/document-management"
info "- Systemd service: /etc/systemd/system/document-management.service"
info ""
info "Database credentials:"
info "- Database: ${DB_NAME}"
info "- User: ${DB_USER}"
info "- Password: ${DB_PASS}"
info ""
warning "Don't forget to:"
warning "1. Change the JWT secret key in appsettings.Production.json"
warning "2. Update the domain name in configuration files"
warning "3. Configure Oracle Cloud Security Lists for ports 80 and 443"