# 🚀 Oracle VPS Ubuntu Deployment Rehberi

Bu rehber, Document Management System projesini Oracle Cloud VPS'te Ubuntu 22.04 üzerinde canlıya almak için gerekli tüm adımları içerir.

## 📋 İçindekiler

1. [Oracle VPS Hazırlığı](#oracle-vps-hazırlığı)
2. [Ubuntu Server Kurulumu](#ubuntu-server-kurulumu)
3. [Gerekli Yazılımları Yükleme](#gerekli-yazılımları-yükleme)
4. [MySQL Kurulumu](#mysql-kurulumu)
5. [Backend Deployment](#backend-deployment)
6. [Frontend Deployment](#frontend-deployment)
7. [Nginx Konfigürasyonu](#nginx-konfigürasyonu)
8. [SSL Certificate](#ssl-certificate)
9. [Firewall ve Güvenlik](#firewall-ve-güvenlik)
10. [Monitoring ve Backup](#monitoring-ve-backup)

---

## 🔧 Oracle VPS Hazırlığı

### 1. Oracle Cloud Instance Oluştur

```bash
# Oracle Cloud Console'da:
# - Compute > Instances > Create Instance
# - Shape: VM.Standard.E2.1.Micro (Always Free)
# - Image: Ubuntu 22.04 LTS
# - Boot Volume: 50GB
# - Network: Public IP assign et
# - SSH Keys: Public key'ini ekle
```

### 2. SSH Bağlantısı

```bash
# Local makinenden VPS'e bağlan
ssh -i ~/.ssh/your-private-key ubuntu@your-vps-ip

# İlk bağlantıda sistem güncellemesi
sudo apt update && sudo apt upgrade -y
```

---

## 🖥️ Ubuntu Server Kurulumu

### 1. Sistem Güncellemesi

```bash
# Paket listesini güncelle
sudo apt update

# Sistemı güncelle
sudo apt upgrade -y

# Gerekli temel paketleri yükle
sudo apt install -y curl wget git unzip software-properties-common apt-transport-https ca-certificates gnupg lsb-release
```

### 2. Swap Dosyası Oluştur (Opsiyonel)

```bash
# 2GB swap dosyası oluştur
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile

# Kalıcı hale getir
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

---

## 📦 Gerekli Yazılımları Yükleme

### 1. .NET 8 Runtime Kurulumu

```bash
# Microsoft paket repository'sini ekle
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# .NET 8 Runtime ve SDK yükle
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0 dotnet-sdk-8.0

# Kurulumu doğrula
dotnet --version
```

### 2. Node.js Kurulumu

```bash
# NodeSource repository ekle
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -

# Node.js yükle
sudo apt install -y nodejs

# Kurulumu doğrula
node --version
npm --version
```

### 3. Nginx Kurulumu

```bash
# Nginx yükle
sudo apt install -y nginx

# Nginx'i başlat ve enable et
sudo systemctl start nginx
sudo systemctl enable nginx

# Status kontrol et
sudo systemctl status nginx
```

---

## 🗄️ MySQL Kurulumu

### 1. MySQL Server Kurulumu

```bash
# MySQL Server yükle
sudo apt install -y mysql-server

# MySQL'i başlat ve enable et
sudo systemctl start mysql
sudo systemctl enable mysql

# Güvenlik konfigürasyonu
sudo mysql_secure_installation
```

### 2. Database ve User Oluştur

```bash
# MySQL'e root olarak bağlan
sudo mysql -u root -p

# Database oluştur
CREATE DATABASE DocumentManagementDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# User oluştur ve yetki ver
CREATE USER 'docuser'@'localhost' IDENTIFIED BY 'VeryStrongPassword123!';
GRANT ALL PRIVILEGES ON DocumentManagementDB.* TO 'docuser'@'localhost';
FLUSH PRIVILEGES;

# Çıkış
EXIT;

# Bağlantıyı test et
mysql -u docuser -p DocumentManagementDB
```

---

## 🔧 Backend Deployment

### 1. Proje Dosyalarını Yükle

```bash
# Proje klasörü oluştur
sudo mkdir -p /var/www/document-management
sudo chown -R $USER:$USER /var/www/document-management

# Git ile projeyi clone et (veya SCP ile dosyaları kopyala)
cd /var/www/document-management
git clone https://github.com/your-username/document-management.git .

# Veya local'den dosyaları kopyala
# scp -r -i ~/.ssh/your-key ./DocumentManagementAPI ubuntu@your-vps-ip:/var/www/document-management/
```

### 2. Backend Build

```bash
# Backend klasörüne git
cd /var/www/document-management/DocumentManagementAPI

# Dependencies restore et
dotnet restore

# Production build
dotnet publish -c Release -o /var/www/document-management/api

# Uploads klasörü oluştur
mkdir -p /var/www/document-management/api/uploads
mkdir -p /var/www/document-management/api/logs
```

### 3. Production Configuration

```bash
# Production appsettings oluştur
sudo nano /var/www/document-management/api/appsettings.Production.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DocumentManagementDB;User=docuser;Password=VeryStrongPassword123!;"
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
  "AllowedHosts": "yourdomain.com,www.yourdomain.com,your-vps-ip"
}
```

### 4. Database Migration

```bash
# Migration çalıştır
cd /var/www/document-management/api
dotnet DocumentManagementAPI.dll --migrate

# Veya EF Core tools ile
# dotnet ef database update --project /var/www/document-management/DocumentManagementAPI
```

### 5. Systemd Service Oluştur

```bash
# Service dosyası oluştur
sudo nano /etc/systemd/system/document-management.service
```

```ini
[Unit]
Description=Document Management API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/document-management/api/DocumentManagementAPI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=document-management
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
WorkingDirectory=/var/www/document-management/api

[Install]
WantedBy=multi-user.target
```

```bash
# Service'i enable et ve başlat
sudo systemctl daemon-reload
sudo systemctl enable document-management
sudo systemctl start document-management

# Status kontrol et
sudo systemctl status document-management

# Logs kontrol et
sudo journalctl -u document-management -f
```

---

## 🌐 Frontend Deployment

### 1. Frontend Build

```bash
# Frontend klasörüne git
cd /var/www/document-management

# Production environment ayarla
echo "VITE_API_BASE_URL=https://yourdomain.com/api" > .env.production

# Dependencies yükle
npm install

# Production build
npm run build

# Build dosyalarını nginx klasörüne kopyala
sudo mkdir -p /var/www/html/document-management
sudo cp -r dist/* /var/www/html/document-management/
```

### 2. Permissions Ayarla

```bash
# Dosya sahipliklerini ayarla
sudo chown -R www-data:www-data /var/www/document-management
sudo chown -R www-data:www-data /var/www/html/document-management

# Permissions ayarla
sudo chmod -R 755 /var/www/document-management
sudo chmod -R 755 /var/www/html/document-management
sudo chmod -R 777 /var/www/document-management/api/uploads
sudo chmod -R 777 /var/www/document-management/api/logs
```

---

## ⚙️ Nginx Konfigürasyonu

### 1. Nginx Site Konfigürasyonu

```bash
# Site konfigürasyon dosyası oluştur
sudo nano /etc/nginx/sites-available/document-management
```

```nginx
# HTTP to HTTPS redirect
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com your-vps-ip;
    return 301 https://$server_name$request_uri;
}

# HTTPS Configuration
server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;
    
    # SSL Configuration (Let's Encrypt certificates)
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
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
        try_files $uri $uri/ /index.html;
        
        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
    
    # API Backend
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # File upload endpoint (extended timeout)
    location /api/documents/upload {
        proxy_pass http://localhost:5000/api/documents/upload;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # Extended timeouts for file uploads
        proxy_connect_timeout 300s;
        proxy_send_timeout 300s;
        proxy_read_timeout 300s;
        client_max_body_size 100M;
    }
}

# IP-based access (for initial setup)
server {
    listen 80;
    server_name your-vps-ip;
    
    location / {
        root /var/www/html/document-management;
        index index.html;
        try_files $uri $uri/ /index.html;
    }
    
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 2. Site'ı Enable Et

```bash
# Site'ı enable et
sudo ln -s /etc/nginx/sites-available/document-management /etc/nginx/sites-enabled/

# Default site'ı disable et
sudo rm /etc/nginx/sites-enabled/default

# Nginx konfigürasyonunu test et
sudo nginx -t

# Nginx'i restart et
sudo systemctl restart nginx
```

---

## 🔒 SSL Certificate (Let's Encrypt)

### 1. Certbot Kurulumu

```bash
# Certbot yükle
sudo apt install -y certbot python3-certbot-nginx

# SSL certificate al (domain'in DNS'i VPS IP'sine yönlendirilmiş olmalı)
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal test et
sudo certbot renew --dry-run

# Crontab'a auto-renewal ekle
echo "0 12 * * * /usr/bin/certbot renew --quiet" | sudo crontab -
```

### 2. SSL Konfigürasyonunu Test Et

```bash
# SSL test et
curl -I https://yourdomain.com

# Certificate bilgilerini kontrol et
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com
```

---

## 🔥 Firewall ve Güvenlik

### 1. UFW Firewall Kurulumu

```bash
# UFW yükle ve enable et
sudo apt install -y ufw

# Default policies
sudo ufw default deny incoming
sudo ufw default allow outgoing

# SSH, HTTP, HTTPS portlarını aç
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# MySQL portunu sadece localhost'a aç (opsiyonel)
sudo ufw allow from 127.0.0.1 to any port 3306

# Firewall'ı enable et
sudo ufw enable

# Status kontrol et
sudo ufw status verbose
```

### 2. Oracle Cloud Security List

```bash
# Oracle Cloud Console'da:
# Networking > Virtual Cloud Networks > Your VCN > Security Lists
# Ingress Rules ekle:
# - Port 80 (HTTP): 0.0.0.0/0
# - Port 443 (HTTPS): 0.0.0.0/0
# - Port 22 (SSH): Your IP/32 (güvenlik için sadece kendi IP'n)
```

### 3. Fail2Ban Kurulumu

```bash
# Fail2ban yükle
sudo apt install -y fail2ban

# Nginx için jail konfigürasyonu
sudo nano /etc/fail2ban/jail.local
```

```ini
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
```

```bash
# Fail2ban'ı başlat
sudo systemctl enable fail2ban
sudo systemctl start fail2ban

# Status kontrol et
sudo fail2ban-client status
```

---

## 📊 Monitoring ve Backup

### 1. Log Monitoring

```bash
# Log dosyalarını kontrol et
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
sudo journalctl -u document-management -f

# Logrotate konfigürasyonu
sudo nano /etc/logrotate.d/document-management
```

```
/var/www/document-management/api/logs/*.txt {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 www-data www-data
}
```

### 2. Backup Script

```bash
# Backup script oluştur
sudo nano /home/ubuntu/backup.sh
```

```bash
#!/bin/bash

# Backup Configuration
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/home/ubuntu/backups"
DB_NAME="DocumentManagementDB"
DB_USER="docuser"
DB_PASS="VeryStrongPassword123!"
UPLOADS_DIR="/var/www/document-management/api/uploads"

# Create backup directory
mkdir -p $BACKUP_DIR

# Database backup
mysqldump -u$DB_USER -p$DB_PASS $DB_NAME | gzip > $BACKUP_DIR/db_$DATE.sql.gz

# Files backup
tar -czf $BACKUP_DIR/files_$DATE.tar.gz -C $(dirname $UPLOADS_DIR) $(basename $UPLOADS_DIR)

# Clean old backups (keep 7 days)
find $BACKUP_DIR -name "db_*.sql.gz" -mtime +7 -delete
find $BACKUP_DIR -name "files_*.tar.gz" -mtime +7 -delete

echo "Backup completed: $DATE"
```

```bash
# Script'i executable yap
chmod +x /home/ubuntu/backup.sh

# Crontab'a günlük backup ekle
crontab -e
# Şu satırı ekle:
# 0 2 * * * /home/ubuntu/backup.sh >> /home/ubuntu/backup.log 2>&1
```

### 3. Health Check Script

```bash
# Health check script oluştur
sudo nano /home/ubuntu/health-check.sh
```

```bash
#!/bin/bash

# Health check
API_URL="http://localhost:5000/api"
SERVICE_NAME="document-management"

# Check API
if ! curl -f -s $API_URL/health > /dev/null; then
    echo "API is down, restarting service..."
    sudo systemctl restart $SERVICE_NAME
    
    # Wait and check again
    sleep 10
    if ! curl -f -s $API_URL/health > /dev/null; then
        echo "Service restart failed!" | mail -s "API Alert" your-email@domain.com
    fi
fi

# Check disk space
DISK_USAGE=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
if [ $DISK_USAGE -gt 80 ]; then
    echo "Disk usage is ${DISK_USAGE}%" | mail -s "Disk Alert" your-email@domain.com
fi
```

```bash
# Script'i executable yap
chmod +x /home/ubuntu/health-check.sh

# Crontab'a 5 dakikada bir health check ekle
crontab -e
# Şu satırı ekle:
# */5 * * * * /home/ubuntu/health-check.sh
```

---

## 🚀 Final Test ve Doğrulama

### 1. Servis Durumlarını Kontrol Et

```bash
# Tüm servislerin durumunu kontrol et
sudo systemctl status nginx
sudo systemctl status mysql
sudo systemctl status document-management

# Port'ların açık olduğunu kontrol et
sudo netstat -tlnp | grep -E ':80|:443|:3306|:5000'
```

### 2. API Test

```bash
# Health check
curl http://localhost:5000/health
curl https://yourdomain.com/api/health

# Login test
curl -X POST https://yourdomain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"superadmin","password":"12345"}'
```

### 3. Frontend Test

```bash
# Frontend erişim test
curl -I https://yourdomain.com
curl -I http://your-vps-ip
```

---

## 🎯 Deployment Checklist

### Pre-Deployment
- [ ] Oracle VPS instance oluşturuldu
- [ ] Domain DNS ayarları yapıldı
- [ ] SSH key'leri ayarlandı

### System Setup
- [ ] Ubuntu güncellemeleri yapıldı
- [ ] .NET 8 runtime yüklendi
- [ ] Node.js yüklendi
- [ ] Nginx yüklendi
- [ ] MySQL yüklendi ve konfigüre edildi

### Application Deployment
- [ ] Backend build edildi ve deploy edildi
- [ ] Frontend build edildi ve deploy edildi
- [ ] Database migration çalıştırıldı
- [ ] Systemd service oluşturuldu ve çalışıyor

### Security & SSL
- [ ] Firewall konfigüre edildi
- [ ] SSL certificate kuruldu
- [ ] Security headers ayarlandı
- [ ] Fail2ban kuruldu

### Monitoring & Backup
- [ ] Log monitoring ayarlandı
- [ ] Backup script'i oluşturuldu ve crontab'a eklendi
- [ ] Health check script'i ayarlandı

### Final Tests
- [ ] API endpoints test edildi
- [ ] Frontend erişilebilir
- [ ] SSL certificate çalışıyor
- [ ] File upload/download test edildi
- [ ] Authentication flow test edildi

---

## 🔧 Troubleshooting

### Yaygın Sorunlar

#### 1. .NET Service Başlamıyor
```bash
# Logs kontrol et
sudo journalctl -u document-management -n 50

# Permissions kontrol et
sudo chown -R www-data:www-data /var/www/document-management

# Port kullanımını kontrol et
sudo netstat -tlnp | grep :5000
```

#### 2. Nginx 502 Bad Gateway
```bash
# Backend service çalışıyor mu?
sudo systemctl status document-management

# Nginx error logs
sudo tail -f /var/log/nginx/error.log

# Proxy ayarlarını kontrol et
sudo nginx -t
```

#### 3. Database Connection Error
```bash
# MySQL çalışıyor mu?
sudo systemctl status mysql

# Connection test
mysql -u docuser -p DocumentManagementDB

# Firewall MySQL portunu engelliyor mu?
sudo ufw status
```

#### 4. SSL Certificate Sorunları
```bash
# Certificate durumu
sudo certbot certificates

# Renewal test
sudo certbot renew --dry-run

# Nginx SSL config test
sudo nginx -t
```

---

## 💡 Performance Optimizasyonu

### 1. Nginx Caching

```bash
# Nginx cache konfigürasyonu ekle
sudo nano /etc/nginx/nginx.conf
```

```nginx
http {
    # Cache zone tanımla
    proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=api_cache:10m max_size=1g inactive=60m use_temp_path=off;
    
    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/xml+rss application/json;
}
```

### 2. MySQL Optimizasyonu

```bash
# MySQL konfigürasyonu
sudo nano /etc/mysql/mysql.conf.d/mysqld.cnf
```

```ini
[mysqld]
# Performance optimizations
innodb_buffer_pool_size = 256M
innodb_log_file_size = 64M
query_cache_type = 1
query_cache_size = 32M
max_connections = 100
```

### 3. System Monitoring

```bash
# htop yükle
sudo apt install -y htop

# System resource monitoring
htop
iostat -x 1
free -h
df -h
```

---

Bu rehber ile Oracle VPS'te Ubuntu üzerinde Document Management System'i başarıyla canlıya alabilirsin! Her adımı dikkatli takip et ve sorun yaşarsan troubleshooting bölümüne bak.

**Önemli**: Production'da mutlaka güçlü şifreler kullan ve güvenlik güncellemelerini takip et! 🔒