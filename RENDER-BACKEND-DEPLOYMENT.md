# 🚀 Render'da Backend Deployment Rehberi

Bu rehber, Document Management API'yi Render platformunda tamamen ücretsiz olarak nasıl canlıya alacağınızı adım adım anlatır.

## 📋 Render Avantajları

- ✅ **750 saat/ay ücretsiz** hosting
- ✅ **Otomatik SSL** certificate
- ✅ **Custom domain** desteği
- ✅ **GitHub entegrasyonu**
- ✅ **Docker** desteği
- ✅ **Environment variables**
- ✅ **Otomatik deploy** (Git push ile)

---

## 🎯 **1. Render Hesabı Oluştur**

### **A. Kayıt Ol**
1. **https://render.com** adresine git
2. **"Get Started for Free"** tıkla
3. **GitHub ile giriş** yap (önerilen)
4. **Email doğrulama** yap

### **B. Dashboard'a Giriş**
- Render Dashboard açılacak
- **"New +"** butonunu göreceksin

---

## 🖥️ **2. Web Service Oluştur**

### **A. Service Tipi Seç**
1. **"New +"** → **"Web Service"** tıkla
2. **"Build and deploy from a Git repository"** seç
3. **"Next"** tıkla

### **B. Repository Bağla**
1. **GitHub repository**'ni seç
2. Repository'nin **public** olması gerekiyor
3. **"Connect"** tıkla

### **C. Service Ayarları**
```
Name: uploaddocumentbe
Region: Oregon (US West)
Branch: main
Root Directory: DocumentManagementAPI
Runtime: Docker
```

### **D. Build Ayarları**
```
Build Command: (boş bırak - Docker kullanıyoruz)
Start Command: (boş bırak - Dockerfile'da tanımlı)
```

---

## ⚙️ **3. Environment Variables Ekle**

### **A. Environment Variables Sekmesi**
Service ayarlarında **"Environment"** sekmesine git

### **B. Gerekli Variables**
```
ASPNETCORE_ENVIRONMENT = Production

ConnectionStrings__DefaultConnection = Server=sql.freedb.tech;Database=sql7800199;User=sql7800199;Password=xa3L1w7xpG;Port=3306;SslMode=Required;

JwtSettings__SecretKey = FreePlatform-SecureKey-32Chars-Min-Change-This-Production-Key-2024

JwtSettings__Issuer = DocumentManagementAPI

JwtSettings__Audience = DocumentManagementClient

JwtSettings__ExpiryMinutes = 1440
```

### **C. Variable Ekleme**
1. **"Add Environment Variable"** tıkla
2. **Key** ve **Value** gir
3. Her variable için tekrarla
4. **"Save Changes"** tıkla

---

## 🐳 **4. Dockerfile Kontrolü**

Render otomatik olarak `DocumentManagementAPI/Dockerfile` dosyasını kullanacak:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DocumentManagementAPI.csproj", "."]
RUN dotnet restore "DocumentManagementAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "DocumentManagementAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DocumentManagementAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for uploads and logs
RUN mkdir -p uploads logs

# Render uses PORT environment variable (default 10000)
CMD ASPNETCORE_URLS=http://*:${PORT:-10000} dotnet DocumentManagementAPI.dll
```

---

## 🚀 **5. Deploy İşlemi**

### **A. İlk Deploy**
1. **"Create Web Service"** tıkla
2. Render otomatik build başlatır
3. **Build logs** takip et
4. **5-10 dakika** sürebilir

### **B. Build Süreci**
```
1. Repository clone
2. Docker image build
3. Dependencies install
4. Application publish
5. Container start
6. Health check
```

### **C. Deploy Durumu**
- **Building**: Build devam ediyor
- **Live**: Deploy başarılı
- **Failed**: Hata var, logs kontrol et

---

## 🔗 **6. Service URL'i Al**

### **A. Dashboard'da URL**
Deploy başarılı olduktan sonra:
```
https://your-service-name.onrender.com
```

### **B. API Test**
```bash
# Health check
curl https://your-service-name.onrender.com/health

# Swagger UI
https://your-service-name.onrender.com
```

---

## 🗄️ **7. Database Migration**

### **A. Render Shell Kullan**
1. Service dashboard'da **"Shell"** sekmesi
2. **"Launch Shell"** tıkla
3. Migration komutunu çalıştır:

```bash
dotnet ef database update
```

### **B. Alternatif: Local Migration**
```bash
# Local'de connection string ile
dotnet ef database update --connection "Server=sql.freedb.tech;Database=sql7800199;User=sql7800199;Password=xa3L1w7xpG;Port=3306;SslMode=Required;"
```

---

## 🔄 **8. Otomatik Deploy Ayarla**

### **A. Auto-Deploy**
- **GitHub'a push** yaptığında otomatik deploy olur
- **"Auto-Deploy"** varsayılan olarak aktif

### **B. Manual Deploy**
- Service dashboard'da **"Manual Deploy"** → **"Deploy latest commit"**

---

## 🌐 **9. Custom Domain (Opsiyonel)**

### **A. Domain Ekle**
1. **"Settings"** → **"Custom Domains"**
2. **"Add Custom Domain"** tıkla
3. Domain'ini gir: `api.yourdomain.com`

### **B. DNS Ayarları**
```
CNAME api your-service-name.onrender.com
```

### **C. SSL Certificate**
- Render otomatik SSL certificate sağlar
- **Let's Encrypt** kullanır

---

## 📊 **10. Monitoring ve Logs**

### **A. Logs Görüntüle**
- Service dashboard'da **"Logs"** sekmesi
- **Real-time** log takibi
- **Error** ve **warning** mesajları

### **B. Metrics**
- **CPU** ve **Memory** kullanımı
- **Request** sayısı
- **Response time**

### **C. Health Checks**
```
Health Check Path: /health
Timeout: 30 seconds
```

---

## 💰 **11. Ücretsiz Tier Limitleri**

### **A. Limitler**
- **750 saat/ay** çalışma süresi
- **512MB RAM**
- **0.1 CPU**
- **Disk**: Geçici (restart'ta silinir)

### **B. Sleep Mode**
- **15 dakika** inaktivite sonrası uyur
- **İlk request** 30 saniye gecikebilir

### **C. Bandwidth**
- **100GB/ay** ücretsiz
- Sonrası **$0.10/GB**

---

## 🚨 **12. Troubleshooting**

### **A. Build Hatası**
```bash
# Logs kontrol et
# Common issues:
# - Dockerfile path yanlış
# - Dependencies eksik
# - Port configuration hatası
```

### **B. Runtime Hatası**
```bash
# Environment variables kontrol et
# Database connection test et
# Logs'da error mesajları ara
```

### **C. Database Connection**
```bash
# FreeSQLDatabase erişilebilir mi test et
mysql -h sql.freedb.tech -P 3306 -u sql7800199 -p
```

---

## ✅ **13. Deployment Checklist**

### **Pre-Deploy**
- [ ] GitHub repository hazır
- [ ] Dockerfile doğru konumda
- [ ] Environment variables listesi hazır
- [ ] Database bilgileri doğru

### **Deploy**
- [ ] Render service oluşturuldu
- [ ] Environment variables eklendi
- [ ] Build başarılı
- [ ] Service live durumda

### **Post-Deploy**
- [ ] API health check geçiyor
- [ ] Database migration çalıştırıldı
- [ ] Swagger UI erişilebilir
- [ ] Authentication test edildi

### **Frontend Integration**
- [ ] Frontend'de API URL güncellendi
- [ ] CORS ayarları doğru
- [ ] End-to-end test yapıldı

---

## 🎯 **Sonraki Adımlar**

1. **Frontend'i Netlify'e deploy et**
2. **API URL'ini frontend'de güncelle**
3. **End-to-end test yap**
4. **Custom domain ekle** (opsiyonel)
5. **Monitoring setup yap**

---

Bu rehberle Render'da backend'inizi başarıyla canlıya alabilirsiniz! 🚀

**API URL'iniz**: `https://your-service-name.onrender.com`