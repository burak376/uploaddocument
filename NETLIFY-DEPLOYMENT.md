# 🚀 Netlify ile Frontend Deployment Rehberi

Netlify sadece **frontend (static site)** hosting yapar. Backend API için ayrı bir servis kullanmamız gerekiyor.

## 📋 Deployment Stratejisi

### **Frontend**: Netlify (Ücretsiz)
### **Backend**: Heroku/Railway/Render (Ücretsiz tier)
### **Database**: PlanetScale/Supabase/Railway (Ücretsiz tier)

---

## 🎯 **1. Frontend - Netlify Deployment**

### **A. Netlify'da Site Oluştur**

1. **Netlify.com**'a git ve hesap oluştur
2. **"New site from Git"** tıkla
3. **GitHub repository**'ni bağla
4. **Build settings**:
   - **Build command**: `npm run build`
   - **Publish directory**: `dist`
   - **Node version**: `18`

### **B. Environment Variables**

Netlify Dashboard → Site Settings → Environment Variables:

```
VITE_API_BASE_URL = https://your-backend-api.herokuapp.com/api
```

### **C. Custom Domain (Opsiyonel)**

1. **Domain settings** → **Add custom domain**
2. **DNS ayarları**:
   ```
   CNAME www your-site.netlify.app
   A     @   75.2.60.5
   ```

---

## 🖥️ **2. Backend - Heroku Deployment**

### **A. Heroku Hesabı ve CLI**

```bash
# Heroku CLI yükle
npm install -g heroku

# Login
heroku login

# Proje klasöründe
cd DocumentManagementAPI
```

### **B. Heroku App Oluştur**

```bash
# App oluştur
heroku create your-app-name-api

# Git remote ekle
heroku git:remote -a your-app-name-api
```

### **C. Database - ClearDB MySQL (Ücretsiz)**

```bash
# ClearDB addon ekle
heroku addons:create cleardb:ignite

# Connection string al
heroku config:get CLEARDB_DATABASE_URL
```

### **D. Environment Variables**

```bash
# Heroku config vars
heroku config:set ASPNETCORE_ENVIRONMENT=Production
heroku config:set ConnectionStrings__DefaultConnection="your-cleardb-connection-string"
heroku config:set JwtSettings__SecretKey="your-super-secure-production-key-32-chars"
```

### **E. Deploy**

```bash
# Build ve deploy
git add .
git commit -m "Deploy to Heroku"
git push heroku main

# Migration çalıştır
heroku run dotnet ef database update
```

---

## 🗄️ **3. Database Seçenekleri**

### **A. ClearDB (Heroku addon)**
- **Ücretsiz**: 5MB
- **Kolay entegrasyon**
- **Otomatik backup**

### **B. PlanetScale (Önerilen)**
- **Ücretsiz**: 5GB
- **Serverless MySQL**
- **Branch'ler ile schema versioning**

```bash
# PlanetScale connection string
mysql://username:password@host/database?sslaccept=strict
```

### **C. Railway**
- **Ücretsiz**: 1GB
- **PostgreSQL/MySQL**
- **Kolay setup**

---

## ⚙️ **4. Konfigürasyon Değişiklikleri**

### **A. CORS Ayarları**

`Program.cs` dosyasında:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "https://your-netlify-site.netlify.app",
            "https://yourdomain.com",
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

### **B. appsettings.Production.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-host;Database=your-db-name;User=your-user;Password=your-password;SslMode=Required;"
  },
  "JwtSettings": {
    "SecretKey": "your-production-secret-key-32-characters-minimum",
    "Issuer": "DocumentManagementAPI",
    "Audience": "DocumentManagementClient",
    "ExpiryMinutes": 1440
  },
  "AllowedHosts": "*"
}
```

### **C. Frontend Environment**

`.env.production`:
```
VITE_API_BASE_URL=https://your-backend-api.herokuapp.com/api
```

---

## 🚀 **5. Deployment Workflow**

### **Frontend (Otomatik)**
1. **GitHub'a push** yap
2. **Netlify otomatik build** yapar
3. **Deploy** eder

### **Backend (Manuel)**
```bash
cd DocumentManagementAPI
git add .
git commit -m "Update API"
git push heroku main
```

---

## 💰 **6. Maliyet Analizi**

### **Ücretsiz Tier**
- **Netlify**: Ücretsiz (100GB bandwidth)
- **Heroku**: Ücretsiz (550 saat/ay)
- **ClearDB**: Ücretsiz (5MB)
- **Domain**: ~$10-15/yıl

### **Paid Tier (Önerilen)**
- **Netlify Pro**: $19/ay
- **Heroku Hobby**: $7/ay
- **PlanetScale**: $29/ay
- **Toplam**: ~$55/ay

---

## 🔧 **7. Alternatif Backend Seçenekleri**

### **A. Railway**
```bash
# Railway CLI
npm install -g @railway/cli
railway login
railway init
railway up
```

### **B. Render**
- **GitHub entegrasyonu**
- **Otomatik deploy**
- **Ücretsiz SSL**

### **C. DigitalOcean App Platform**
- **$5/ay** başlangıç
- **Managed database**
- **Auto-scaling**

---

## 🎯 **8. Deployment Checklist**

### **Frontend**
- [ ] Netlify'da site oluşturuldu
- [ ] Environment variables ayarlandı
- [ ] Custom domain bağlandı (opsiyonel)
- [ ] HTTPS aktif

### **Backend**
- [ ] Heroku app oluşturuldu
- [ ] Database addon eklendi
- [ ] Environment variables ayarlandı
- [ ] Migration çalıştırıldı
- [ ] CORS ayarları güncellendi

### **Test**
- [ ] Frontend erişilebilir
- [ ] API endpoints çalışıyor
- [ ] Authentication flow test edildi
- [ ] File upload/download test edildi

---

## 🚨 **Önemli Notlar**

1. **Heroku Sleep Mode**: Ücretsiz tier'da 30 dakika inaktivite sonrası uyur
2. **File Storage**: Heroku'da dosyalar kalıcı değil, AWS S3 kullan
3. **Database Limits**: Ücretsiz tier'larda boyut sınırları var
4. **CORS**: Production domain'lerini mutlaka ekle

---

Bu rehberle projenizi Netlify + Heroku kombinasyonu ile canlıya alabilirsiniz! 🎉