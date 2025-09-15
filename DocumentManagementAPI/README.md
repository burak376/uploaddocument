# Document Management API

Bu proje, belge yönetim sistemi için .NET 8 Web API backend'idir.

## Özellikler

- JWT tabanlı kimlik doğrulama
- Rol tabanlı yetkilendirme (SuperAdmin, CompanyAdmin, User)
- Firma yönetimi
- Kullanıcı yönetimi
- Belge türü yönetimi
- Belge yükleme ve indirme
- MySQL veritabanı desteği
- Entity Framework Core
- Serilog ile loglama
- Swagger/OpenAPI dokümantasyonu

## Gereksinimler

- .NET 8 SDK
- MySQL Server 8.0+
- Visual Studio 2022 veya VS Code

## Kurulum

### 1. Projeyi klonlayın
```bash
git clone <repository-url>
cd DocumentManagementAPI
```

### 2. NuGet paketlerini yükleyin
```bash
dotnet restore
```

### 3. MySQL veritabanını hazırlayın

MySQL Server'ı çalıştırın ve aşağıdaki komutu çalıştırarak veritabanını oluşturun:

```sql
CREATE DATABASE DocumentManagementDB;
```

### 4. Bağlantı dizesini güncelleyin

`appsettings.Development.json` dosyasındaki bağlantı dizesini kendi MySQL ayarlarınıza göre güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DocumentManagementDB;User=root;Password=YourPassword;"
  }
}
```

### 5. Veritabanı migration'larını çalıştırın

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6. Uygulamayı çalıştırın

```bash
dotnet run
```

API şu adreste çalışacaktır: `https://localhost:7001` veya `http://localhost:5001`

Swagger UI: `https://localhost:7001` (Development ortamında)

## Docker ile Çalıştırma (Opsiyonel)

### docker-compose.yml dosyası oluşturun:

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: documentmanagement_mysql
    environment:
      MYSQL_ROOT_PASSWORD: 12345
      MYSQL_DATABASE: DocumentManagementDB
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  api:
    build: .
    container_name: documentmanagement_api
    ports:
      - "5001:80"
    depends_on:
      - mysql
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=mysql;Database=DocumentManagementDB;User=root;Password=12345;

volumes:
  mysql_data:
```

### Dockerfile oluşturun:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

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

### Docker Compose ile çalıştırın:

```bash
docker-compose up -d
```

## Test Kullanıcıları

Sistem başlatıldığında aşağıdaki test kullanıcıları otomatik olarak oluşturulur:

| Kullanıcı Adı | Şifre | Rol | Firma |
|---------------|-------|-----|-------|
| superadmin | 12345 | SuperAdmin | - |
| bugibo_admin | 12345 | CompanyAdmin | Bugibo Yazılım |
| burak | 12345 | User | Bugibo Yazılım |

## API Endpoints

### Authentication
- `POST /api/auth/login` - Kullanıcı girişi

### Companies (SuperAdmin)
- `GET /api/companies` - Tüm firmaları listele
- `GET /api/companies/{id}` - Firma detayı
- `POST /api/companies` - Yeni firma oluştur
- `PUT /api/companies/{id}` - Firma güncelle
- `DELETE /api/companies/{id}` - Firma sil

### Users (SuperAdmin, CompanyAdmin)
- `GET /api/users` - Kullanıcıları listele
- `GET /api/users/{id}` - Kullanıcı detayı
- `POST /api/users` - Yeni kullanıcı oluştur
- `PUT /api/users/{id}` - Kullanıcı güncelle
- `DELETE /api/users/{id}` - Kullanıcı sil
- `POST /api/users/{id}/change-password` - Şifre değiştir

### Document Types (SuperAdmin, CompanyAdmin)
- `GET /api/documenttypes` - Belge türlerini listele
- `GET /api/documenttypes/{id}` - Belge türü detayı
- `POST /api/documenttypes` - Yeni belge türü oluştur
- `PUT /api/documenttypes/{id}` - Belge türü güncelle
- `DELETE /api/documenttypes/{id}` - Belge türü sil

### Documents (Tüm roller)
- `GET /api/documents` - Belgeleri listele
- `GET /api/documents/{id}` - Belge detayı
- `POST /api/documents/upload` - Belge yükle
- `GET /api/documents/{id}/download` - Belge indir
- `DELETE /api/documents/{id}` - Belge sil
- `POST /api/documents/search` - Belge ara

## Dosya Depolama

Yüklenen dosyalar şu klasör yapısında saklanır:
```
/uploads/{companyId}/{ownerKey}/{documentTypeKey}/{yyyy}/{MM}/{guid}_{filename}
```

## Loglama

Loglar `logs/` klasöründe günlük olarak saklanır. Serilog kullanılarak hem konsola hem de dosyaya log yazılır.

## Güvenlik

- JWT token tabanlı kimlik doğrulama
- Rol tabanlı yetkilendirme
- BCrypt ile şifre hashleme
- CORS politikaları
- Dosya uzantısı ve boyut kontrolü

## Geliştirme

### Migration oluşturma:
```bash
dotnet ef migrations add MigrationName
```

### Migration uygulama:
```bash
dotnet ef database update
```

### Veritabanını sıfırlama:
```bash
dotnet ef database drop
dotnet ef database update
```

## Sorun Giderme

1. **MySQL bağlantı hatası**: Bağlantı dizesini ve MySQL servisinin çalıştığını kontrol edin
2. **Migration hataları**: Veritabanını drop edip yeniden oluşturun
3. **JWT token hataları**: SecretKey'in en az 32 karakter olduğundan emin olun
4. **CORS hataları**: Frontend URL'ini CORS politikasına ekleyin

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır.