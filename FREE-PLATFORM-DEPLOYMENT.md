# 🆓 Ücretsiz Platform Deployment Rehberi

Bu rehber projeyi tamamen ücretsiz platformlarda nasıl yayınlayacağınızı adım adım anlatır.

## 🎯 **Ücretsiz Platform Kombinasyonu**

### **Frontend**: Netlify/Vercel (Ücretsiz)
### **Backend**: Render (Ücretsiz 750 saat/ay)
### **Database**: FreeSQLDatabase (Ücretsiz 5MB MySQL)

---

## 📋 **1. Database - FreeSQLDatabase (Hazır)**

### **A. Mevcut Database Bilgileri**
Görüntüdeki bilgilere göre database zaten hazır:

### **B. Database Bilgileri**
```
Server: sql.freedb.tech
Port: 3306
Database: sql7800199
Username: sql7800199
Password: xa3L1w7xpG
```

### **C. Database Oluştur**
```sql
-- phpMyAdmin'den çalıştır
CREATE TABLE IF NOT EXISTS Companies (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    TaxNumber VARCHAR(20) NOT NULL UNIQUE,
    Address VARCHAR(500),
    Phone VARCHAR(20),
    Email VARCHAR(255) NOT NULL UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Diğer tablolar için migration kullanacağız
```

---

## 🚀 **2. Backend - Render (Tamamen Ücretsiz)**

### **A. Render Hesabı**
1. **https://render.com** adresine git
2. **GitHub ile giriş** yap
3. **750 saat/ay ücretsiz** tier

### **B. Proje Deploy**
```bash
# GitHub repo'yu Render'a bağla
# 1. New Web Service
# 2. Connect GitHub repo
# 3. Docker environment seç
# 4. render.yaml dosyası otomatik algılanır
```

### **C. Environment Variables**
Render Dashboard → Environment:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sql.freedb.tech;Database=sql7800199;User=sql7800199;Password=xa3L1w7xpG;Port=3306;SslMode=Required;
JwtSettings__SecretKey=FreePlatform-SecureKey-32Chars-Min-Change-This-Production-Key-2024
```

### **D. Custom Domain (Opsiyonel)**
```
your-app-name.onrender.com → API URL'in bu olacak
```

---

## 🌐 **3. Frontend - Netlify**

### **A. Build Ayarları**
Netlify Dashboard → Site Settings:
```
Build command: npm run build
Publish directory: dist
Node version: 18
```

### **B. Environment Variables**
```
VITE_API_BASE_URL=https://your-app-name.onrender.com/api
```

### **C. netlify.toml**
```toml
[build]
  publish = "dist"
  command = "npm run build"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

---

## ⚙️ **4. Konfigürasyon Değişiklikleri**

### **A. CORS Ayarları**
`Program.cs` dosyasında:
```csharp
policy.WithOrigins(
    "https://*.netlify.app",
    "https://*.render.com",
    "http://localhost:5173"
)
```

### **B. File Upload Limiti**
Ücretsiz platformlar için dosya boyutunu düşür:
```json
"FileUpload": {
  "MaxFileSize": 10485760  // 10MB
}
```

### **C. Database Connection**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql.freedb.tech;Database=sql7800199;User=sql7800199;Password=xa3L1w7xpG;Port=3306;SslMode=Required;"
  }
}
```

---

## 🔄 **5. Deployment Süreci**

### **Adım 1: Database Hazırla**
```bash
# Migration oluştur
dotnet ef migrations add InitialCreate

# SQL script'i export et
dotnet ef migrations script > migration.sql

# FreeSQLDatabase phpMyAdmin'de çalıştır
```

### **Adım 2: Backend Deploy**
```bash
# Render'a push (otomatik deploy)
git add .
git commit -m "Deploy to Render"
git push origin main

# Render otomatik deploy eder
```

### **Adım 3: Frontend Deploy**
```bash
# Netlify'e push (otomatik deploy)
git push origin main
```

---

## 💰 **Maliyet Analizi**

### **Tamamen Ücretsiz**
- **Netlify**: Ücretsiz (100GB bandwidth)
- **Render**: Ücretsiz (750 saat/ay)
- **FreeSQLDatabase**: Ücretsiz (5MB)
- **Domain**: Subdomain ücretsiz
- **SSL**: Otomatik ücretsiz

### **Limitler**
- **Database**: 5MB limit
- **Render**: 750 saat/ay sonra duraklama
- **File Storage**: Geçici (restart'ta silinir)

---

## 🎯 **Alternatif Ücretsiz Seçenekler**

### **Backend Alternatifleri**
- **Railway**: $5 kredi/ay
- **Heroku**: 550 saat/ay ücretsiz (deprecated)
- **Fly.io**: Küçük uygulamalar ücretsiz

### **Database Alternatifleri**
- **PlanetScale**: 5GB ücretsiz
- **Supabase**: 500MB ücretsiz
- **MongoDB Atlas**: 512MB ücretsiz

### **File Storage**
- **Cloudinary**: 25GB ücretsiz
- **AWS S3**: 5GB ücretsiz (12 ay)

---

## 🚨 **Önemli Notlar**

1. **File Storage**: Render'da dosyalar geçici, AWS S3 entegrasyonu gerekli
2. **Database Backup**: Manuel backup yapın
3. **SSL**: Otomatik sağlanır
4. **Custom Domain**: Ücretli domain gerekebilir
5. **Monitoring**: Ücretsiz tier'larda sınırlı

---

## 🔧 **Troubleshooting**

### **CORS Hatası**
```csharp
// Program.cs'de wildcard kullan
policy.WithOrigins("https://*.netlify.app")
```

### **Database Connection**
```bash
# Connection string test et
mysql -h sql.freedb.tech -P 3306 -u sql7800199 -p sql7800199
```

### **Render Deploy Hatası**
```bash
# Logs kontrol et
# Render dashboard'dan logs kontrol et

# Restart
# Render dashboard'dan restart
```

Bu rehberle projenizi tamamen ücretsiz platformlarda çalıştırabilirsiniz! 🎉