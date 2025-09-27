# Document Management System

Modern belge yönetim sistemi - .NET 8 Web API + React + TypeScript

## 🚀 Hızlı Başlangıç

### Gereksinimler
- .NET 8 SDK
- Node.js 18+
- MySQL 8.0+

### 1. Bağımlılıkları Yükle
```bash
# Frontend bağımlılıkları
npm install

# Backend bağımlılıkları
cd DocumentManagementAPI
dotnet restore
cd ..
```

### 2. Database Kurulumu
```bash
# MySQL'de database oluştur
CREATE DATABASE DocumentManagementDB;

# Migration çalıştır
cd DocumentManagementAPI
dotnet ef database update
cd ..
```

### 3. Projeyi Çalıştır

#### Seçenek 1: NPM Script ile (Önerilen)
```bash
npm run dev:both
```

#### Seçenek 2: Script Dosyası ile
```bash
# Windows
start-dev.bat

# Linux/Mac
chmod +x start-dev.sh
./start-dev.sh
```

#### Seçenek 3: Docker ile
```bash
docker-compose -f docker-compose.dev.yml up
```

#### Seçenek 4: Manuel
```bash
# Terminal 1 - Backend
cd DocumentManagementAPI
dotnet run

# Terminal 2 - Frontend
npm run dev
```

## 📡 Erişim Adresleri

- **Frontend**: http://localhost:5173
- **Backend API**: https://localhost:7001
- **Swagger UI**: https://localhost:7001
- **MySQL**: localhost:3306

## 👤 Test Kullanıcıları

| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| superadmin | 12345 | Super Admin |
| bugibo_admin | 12345 | Company Admin |
| burak | 12345 | User |

## 🛠️ Geliştirme

### Backend (.NET 8)
```bash
cd DocumentManagementAPI
dotnet watch run  # Hot reload ile çalıştır
```

### Frontend (React + Vite)
```bash
npm run dev  # Hot reload ile çalıştır
```

## 📦 Build

### Production Build
```bash
# Frontend
npm run build

# Backend
cd DocumentManagementAPI
dotnet publish -c Release
```

## 🐳 Docker

### Development
```bash
docker-compose -f docker-compose.dev.yml up
```

### Production
```bash
docker-compose -f docker-compose.prod.yml up
```

## Deployment notes for SPA routing

If you deploy the frontend as a static Single Page App (SPA) you must ensure unknown routes are rewritten to `index.html` so client-side routing (React Router) works. This repo includes:

- `public/_redirects` for Netlify and Render
- `public/static.json` for Render
- `public/.htaccess` for Apache/cPanel hosting
- `static.json` in the project root for Render static sites

Without these, direct navigation to routes like `/login` or `/dashboard` returns 404 from the static file server.

You can verify the SPA fallback locally with:

```bash
npm run test:spa
```

This command builds the project, serves the production bundle with history fallback enabled, and asserts that `/`, `/login`, and `/dashboard` all return the SPA shell instead of a 404.
