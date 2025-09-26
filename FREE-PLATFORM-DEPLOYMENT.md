# 🆓 Ücretsiz Platform Deployment Rehberi

Bu rehber projeyi tamamen ücretsiz platformlarda nasıl yayınlayacağınızı adım adım anlatır.

## 🎯 **Ücretsiz Platform Kombinasyonu**

### **Frontend**: Netlify/Vercel (Ücretsiz)
### **Backend**: Railway (Ücretsiz $5 kredi/ay)
### **Database**: FreeSQLDatabase (Ücretsiz 100MB MySQL)

---

## 📋 **1. Database - FreeSQLDatabase**

### **A. Hesap Oluştur**
1. **https://www.freesqldatabase.com** adresine git
2. **"Create Free MySQL Database"** tıkla
3. Form doldur:
   - **Database Name**: `freedb_documentmgmt`
   - **Username**: `freedb_docuser`
   - **Password**: Güçlü şifre oluştur

### **B. Database Bilgileri**
```
Server: sql.freedb.tech
Port: 3306
Database: freedb_documentmgmt
Username: freedb_docuser
Password: [your_password]
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

## 🚀 **2. Backend - Railway**

### **A. Railway Hesabı**
1. **https://railway.app** adresine git
2. **GitHub ile giriş** yap
3. **$5 ücretsiz kredi** al

### **B. Proje Deploy**
```bash
# Railway CLI yükle
npm install -g @railway/cli

# Login
railway login

# Proje klasöründe
railway init

# Deploy
railway up
```

### **C. Environment Variables**
Railway Dashboard → Variables:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sql.freedb.tech;Database=freedb_documentmgmt;User=freedb_docuser;Password=YOUR_PASSWORD;Port=3306;SslMode=Required;
JwtSettings__SecretKey=production-super-secure-secret-key-minimum-32-characters-required-change-this
```

### **D. Custom Domain (Opsiyonel)**
```
your-app-name.railway.app → API URL'in bu olacak
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
VITE_API_BASE_URL=https://your-app-name.railway.app/api
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
    "https://*.vercel.app",
    "http://localhost:5173"
)
```

### **B. File Upload Limiti**
Ücretsiz platformlar için dosya boyutunu düşür:
```json
"FileUpload": {
  "MaxFileSize": 52428800  // 50MB
}
```

### **C. Database Connection**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql.freedb.tech;Database=freedb_documentmgmt;User=freedb_docuser;Password=YOUR_PASSWORD;Port=3306;SslMode=Required;"
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
# Railway'e push
git add .
git commit -m "Deploy to Railway"
git push origin main

# Railway otomatik deploy eder
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
- **Railway**: $5 kredi/ay (yeterli küçük projeler için)
- **FreeSQLDatabase**: Ücretsiz (100MB)
- **Domain**: Subdomain ücretsiz
- **SSL**: Otomatik ücretsiz

### **Limitler**
- **Database**: 100MB limit
- **Railway**: $5 kredi bitince duraklama
- **File Storage**: Geçici (restart'ta silinir)

---

## 🎯 **Alternatif Ücretsiz Seçenekler**

### **Backend Alternatifleri**
- **Render**: 750 saat/ay ücretsiz
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

1. **File Storage**: Railway'de dosyalar geçici, AWS S3 entegrasyonu gerekli
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
mysql -h sql.freedb.tech -P 3306 -u freedb_docuser -p freedb_documentmgmt
```

### **Railway Deploy Hatası**
```bash
# Logs kontrol et
railway logs

# Restart
railway restart
```

Bu rehberle projenizi tamamen ücretsiz platformlarda çalıştırabilirsiniz! 🎉