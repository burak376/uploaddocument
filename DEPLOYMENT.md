# 🚀 Production Deployment Rehberi

Bu rehber, Document Management System projesini canlıya almak için gerekli tüm adımları içerir.

## 📋 İçindekiler

1. [Genel Hazırlık](#genel-hazırlık)
2. [Backend Deployment](#backend-deployment)
3. [Frontend Deployment](#frontend-deployment)
4. [Database Setup](#database-setup)
5. [Domain ve SSL](#domain-ve-ssl)
6. [Monitoring ve Backup](#monitoring-ve-backup)

---

## 🔧 Genel Hazırlık

### 1. Production Environment Variables

#### Backend (.NET API)
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-mysql-server;Database=DocumentManagementDB;User=your-user;Password=your-password;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secure-secret-key-at-least-32-characters-long-for-production",
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
  "AllowedHosts": "yourdomain.com,www.yourdomain.com",
  "FileUpload": {
    "MaxFileSize": 104857600,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".txt"]
  }
}
```

#### Frontend (React)
```env
# .env.production
VITE_API_BASE_URL=https://api.yourdomain.com/api
```

---

## 🖥️ Backend Deployment

### Seçenek 1: Azure App Service (Önerilen)

#### 1. Azure Portal'da App Service Oluştur
```bash
# Azure CLI ile
az webapp create \
  --resource-group myResourceGroup \
  --plan myAppServicePlan \
  --name document-management-api \
  --runtime "DOTNET|8.0"
```

#### 2. Database Connection String Ayarla
```bash
az webapp config appsettings set \
  --resource-group myResourceGroup \
  --name document-management-api \
  --settings ConnectionStrings__DefaultConnection="Server=your-server;Database=DocumentManagementDB;User=your-user;Password=your-password;"
```

#### 3. Deploy
```bash
# Visual Studio'dan Publish
# veya
dotnet publish -c Release
# Publish klasörünü Azure'a yükle
```

### Seçenek 2: DigitalOcean Droplet

#### 1. Droplet Oluştur (Ubuntu 22.04)
```bash
# SSH ile bağlan
ssh root@your-server-ip

# .NET 8 Runtime yükle
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
```

#### 2. MySQL Kurulumu
```bash
sudo apt update
sudo apt install mysql-server
sudo mysql_secure_installation

# Database oluştur
sudo mysql -u root -p
CREATE DATABASE DocumentManagementDB;
CREATE USER 'docuser'@'localhost' IDENTIFIED BY 'strong_password';
GRANT ALL PRIVILEGES ON DocumentManagementDB.* TO 'docuser'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

#### 3. Nginx Kurulumu
```bash
sudo apt install nginx

# Nginx config
sudo nano /etc/nginx/sites-available/document-management
```

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/document-management /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

#### 4. Systemd Service
```bash
sudo nano /etc/systemd/system/document-management.service
```

```ini
[Unit]
Description=Document Management API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/document-management/DocumentManagementAPI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=document-management
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable document-management
sudo systemctl start document-management
sudo systemctl status document-management
```

### Seçenek 3: Docker Deployment

#### 1. Dockerfile Oluştur
```dockerfile
# DocumentManagementAPI/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DocumentManagementAPI.csproj", "."]
RUN dotnet restore "DocumentManagementAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DocumentManagementAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentManagementAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DocumentManagementAPI.dll"]
```

#### 2. Docker Compose
```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: document_mysql
    environment:
      MYSQL_ROOT_PASSWORD: your_root_password
      MYSQL_DATABASE: DocumentManagementDB
      MYSQL_USER: docuser
      MYSQL_PASSWORD: your_password
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    restart: unless-stopped

  api:
    build: 
      context: ./DocumentManagementAPI
      dockerfile: Dockerfile
    container_name: document_api
    ports:
      - "5000:80"
    depends_on:
      - mysql
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=mysql;Database=DocumentManagementDB;User=docuser;Password=your_password;
    volumes:
      - ./uploads:/app/uploads
    restart: unless-stopped

volumes:
  mysql_data:
```

```bash
docker-compose -f docker-compose.prod.yml up -d
```

---

## 🌐 Frontend Deployment

### Seçenek 1: Vercel (Önerilen - Ücretsiz)

#### 1. Vercel CLI Kurulumu
```bash
npm i -g vercel
```

#### 2. Build ve Deploy
```bash
# Production build
npm run build

# Vercel'e deploy
vercel --prod
```

#### 3. Environment Variables
Vercel dashboard'da:
- `VITE_API_BASE_URL` = `https://api.yourdomain.com/api`

### Seçenek 2: Netlify

#### 1. Build Settings
```toml
# netlify.toml
[build]
  publish = "dist"
  command = "npm run build"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

#### 2. Environment Variables
Netlify dashboard'da:
- `VITE_API_BASE_URL` = `https://api.yourdomain.com/api`

### Seçenek 3: Nginx Static Hosting

#### 1. Build
```bash
npm run build
```

#### 2. Nginx Config
```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    root /var/www/document-management-frontend/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 🗄️ Database Setup

### 1. Production Database Oluştur

#### MySQL (Önerilen)
```sql
CREATE DATABASE DocumentManagementDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'docuser'@'%' IDENTIFIED BY 'very_strong_password_123!';
GRANT ALL PRIVILEGES ON DocumentManagementDB.* TO 'docuser'@'%';
FLUSH PRIVILEGES;
```

#### Cloud Database Seçenekleri
- **Azure Database for MySQL**
- **AWS RDS MySQL**
- **DigitalOcean Managed Database**
- **PlanetScale** (Serverless MySQL)

### 2. Migration Çalıştır
```bash
# Production ortamında
dotnet ef database update --environment Production
```

### 3. Backup Stratejisi
```bash
# Günlük backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
mysqldump -u docuser -p DocumentManagementDB > /backups/db_backup_$DATE.sql
find /backups -name "db_backup_*.sql" -mtime +7 -delete
```

---

## 🔒 Domain ve SSL

### 1. Domain Ayarları

#### DNS Records
```
A     @              your-server-ip
A     www            your-server-ip
A     api            your-server-ip
CNAME frontend       your-frontend-url (Vercel/Netlify)
```

### 2. SSL Certificate (Let's Encrypt)

```bash
# Certbot kurulumu
sudo apt install certbot python3-certbot-nginx

# SSL certificate al
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com -d api.yourdomain.com

# Auto-renewal test
sudo certbot renew --dry-run
```

### 3. Nginx SSL Config
```nginx
server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    
    # SSL Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Frame-Options DENY always;
    add_header X-Content-Type-Options nosniff always;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# HTTP to HTTPS redirect
server {
    listen 80;
    server_name api.yourdomain.com;
    return 301 https://$server_name$request_uri;
}
```

---

## 📊 Monitoring ve Backup

### 1. Application Monitoring

#### Serilog ile File Logging
```json
// appsettings.Production.json
{
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/document-management/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

#### Health Checks
```bash
# API health check endpoint
curl https://api.yourdomain.com/health

# Monitoring script
#!/bin/bash
if ! curl -f https://api.yourdomain.com/health; then
    echo "API is down!" | mail -s "API Alert" admin@yourdomain.com
    sudo systemctl restart document-management
fi
```

### 2. Database Monitoring
```bash
# MySQL monitoring script
#!/bin/bash
MYSQL_USER="docuser"
MYSQL_PASS="your_password"
MYSQL_DB="DocumentManagementDB"

# Connection test
if ! mysql -u$MYSQL_USER -p$MYSQL_PASS -e "SELECT 1" $MYSQL_DB; then
    echo "Database connection failed!" | mail -s "DB Alert" admin@yourdomain.com
fi

# Disk space check
DISK_USAGE=$(df /var/lib/mysql | tail -1 | awk '{print $5}' | sed 's/%//')
if [ $DISK_USAGE -gt 80 ]; then
    echo "Database disk usage is ${DISK_USAGE}%" | mail -s "Disk Alert" admin@yourdomain.com
fi
```

### 3. Automated Backups
```bash
# Crontab entry
# 0 2 * * * /home/scripts/backup.sh

#!/bin/bash
# backup.sh
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups"
DB_NAME="DocumentManagementDB"
DB_USER="docuser"
DB_PASS="your_password"

# Database backup
mysqldump -u$DB_USER -p$DB_PASS $DB_NAME | gzip > $BACKUP_DIR/db_$DATE.sql.gz

# Files backup
tar -czf $BACKUP_DIR/files_$DATE.tar.gz /var/www/document-management/uploads

# Upload to cloud storage (optional)
# aws s3 cp $BACKUP_DIR/db_$DATE.sql.gz s3://your-backup-bucket/
# aws s3 cp $BACKUP_DIR/files_$DATE.tar.gz s3://your-backup-bucket/

# Clean old backups (keep 30 days)
find $BACKUP_DIR -name "db_*.sql.gz" -mtime +30 -delete
find $BACKUP_DIR -name "files_*.tar.gz" -mtime +30 -delete

echo "Backup completed: $DATE"
```

---

## 🚀 Deployment Checklist

### Pre-Deployment
- [ ] Environment variables ayarlandı
- [ ] Database connection string güncellendi
- [ ] JWT secret key production için değiştirildi
- [ ] CORS ayarları production domain'i içeriyor
- [ ] File upload path'leri doğru
- [ ] SSL certificate hazır

### Backend Deployment
- [ ] .NET 8 runtime yüklendi
- [ ] Database oluşturuldu
- [ ] Migration çalıştırıldı
- [ ] API çalışıyor ve health check geçiyor
- [ ] File upload/download test edildi
- [ ] Logging çalışıyor

### Frontend Deployment
- [ ] Production build başarılı
- [ ] API URL doğru ayarlandı
- [ ] Routing çalışıyor (SPA fallback)
- [ ] Authentication flow test edildi
- [ ] Tüm sayfalar çalışıyor

### Post-Deployment
- [ ] SSL certificate aktif
- [ ] Domain DNS ayarları doğru
- [ ] Backup script çalışıyor
- [ ] Monitoring kuruldu
- [ ] Performance test yapıldı
- [ ] Security scan yapıldı

---

## 💰 Maliyet Tahmini

### Düşük Bütçe (Aylık ~$20-30)
- **DigitalOcean Droplet**: $12/ay (2GB RAM)
- **Domain**: $10-15/yıl
- **Vercel/Netlify**: Ücretsiz
- **Let's Encrypt SSL**: Ücretsiz

### Orta Bütçe (Aylık ~$50-100)
- **Azure App Service**: $30-50/ay
- **Azure Database for MySQL**: $20-40/ay
- **Vercel Pro**: $20/ay
- **Domain + SSL**: $15/yıl

### Yüksek Performans (Aylık ~$200+)
- **Azure Premium**: $100+/ay
- **Managed Database**: $50+/ay
- **CDN**: $20+/ay
- **Monitoring Tools**: $30+/ay

---

## 🔧 Troubleshooting

### Yaygın Sorunlar

#### 1. CORS Hatası
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

#### 2. File Upload Hatası
```bash
# Klasör izinleri
sudo chown -R www-data:www-data /var/www/document-management/uploads
sudo chmod -R 755 /var/www/document-management/uploads
```

#### 3. Database Connection Hatası
```bash
# MySQL connection test
mysql -h your-host -u your-user -p your-database -e "SELECT 1"
```

#### 4. SSL Certificate Hatası
```bash
# Certificate renewal
sudo certbot renew
sudo systemctl reload nginx
```

---

Bu rehber ile projenizi güvenli ve performanslı bir şekilde canlıya alabilirsiniz. Hangi deployment seçeneğini tercih ederseniz edin, adım adım takip ederek başarılı bir deployment gerçekleştirebilirsiniz.