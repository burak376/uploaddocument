# Belge & Görev Takip Platformu

Çok kiracılı (multi-tenant) şirket yapısını destekleyen .NET 8 + React 18 çözümü. Bu repo, uçtan uca mimariyi, EF Core veri modelini, planlanmış hatırlatma işlerini, e-posta şablonlarını ve React yönlendirme/ekran iskeletlerini içerir.

## 1. Mimari Genel Bakış
```
┌────────────┐        ┌────────────────────┐        ┌──────────────┐
│ React 18   │  HTTPS │ ASP.NET Core 8 API │  EF    │ MySQL 8      │
│ + Vite     │ <─────▶│ Minimal APIs       │◀──────▶│ (tenant izol)│
│ TanStack   │        │ JWT + RBAC         │        └──────────────┘
│ MUI        │        │ Serilog + Audit    │
└─────┬──────┘        │ Hangfire Reminder  │        ┌──────────────┐
      │                │ SMTP (MailKit)     │  SMTP  │ MailHog (dev)│
      │                └────────────────────┘ ◀─────▶└──────────────┘
      │ REST/JSON                                        ▲
      ▼                                                  │
┌──────────────┐                                 Planlı e-postalar
│ Docker Compose│ (api + web + mysql + mailhog + hangfire worker)
└──────────────┘
```

## 2. Proje Yapısı
```
backend/
  DocumentTasking.sln
  src/DocumentTasking.Api/
    Program.cs, Entities/, Data/, Features/, Jobs/, Infrastructure/
    Migrations/ (InitialCreate)
  tests/DocumentTasking.Tests/
    Services/TaskServiceTests.cs
frontend/
  (root)
  src/
    App.tsx, main.tsx, providers/, components/layout/
    features/ (tasks, documents, history)
    pages/ (Login, CompanySelection, TaskList, TaskCreate, TaskDetail, ...)
    tests/DocumentUploadForm.test.tsx
Docker Compose: docker-compose.yml (api, web, mysql, mailhog)
```

## 3. Kurulum
### Gereksinimler
- .NET SDK 8.0+
- Node.js 20+
- Docker & Docker Compose (opsiyonel)

### Bağımlılıkları yükle
```bash
# Frontend
npm install

# Backend
cd backend
dotnet restore
```

### Veritabanı & Migrasyon
```bash
cd backend
# Appsettings içindeki bağlantı mysql:3306 varsayılanıdır.
dotnet ef database update --project src/DocumentTasking.Api/DocumentTasking.Api.csproj
cd ..
```

### Geliştirme Ortamı
```bash
# Terminal 1 - API
cd backend/src/DocumentTasking.Api
dotnet run

# Terminal 2 - Frontend
npm run dev
```

### Docker ile hızlı başlangıç
```bash
docker-compose up --build
```
- API: http://localhost:8080
- Frontend: http://localhost:5173
- MySQL: localhost:3307 (root/Passw0rd!)
- MailHog UI: http://localhost:8025

## 4. Öne Çıkan Backend Bileşenleri
- **Minimal API Uçları** `Features/*Endpoints.cs` altında gruplanmıştır.
- **EF Core 8 Entity’leri** `Domain/Entities` altında. Tüm tablolar `CompanyId` ve `RowVersion` içerir.
- **Tenant izolasyonu** `HttpContextTenantProvider` + `TenantResolutionMiddleware` ile sağlanır; tüm DbSet’lerde global sorgu filtresi vardır.
- **Hatırlatma İşleri** `Jobs/ReminderRunner.cs` Hangfire tarafından `Cron.HourInterval(24)` ile tetiklenir; eksik belgeleri hesaplar ve `EmailQueue` kaydını günceller.
- **E-posta** `Infrastructure/Email` MailKit + Razor templating (`ReminderEmail.cshtml`).
- **Audit Log** `AuditLogger` servisi ve `AuditLoggingMiddleware` ile otomatikleşir.

### Örnek Minimal API
```csharp
app.MapGroup("/api")
   .RequireAuthorization()
   .MapTaskEndpoints();
```
Bkz. `Features/Tasks/TaskEndpoints.cs` – görev oluşturma, listeleme ve durum değiştirme uçları.

### EF Core Entity Örneği
```csharp
public class TaskItem : MultiTenantEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid AssigneeUserId { get; set; }
    public DateTime DueDateUtc { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Open;
    public ICollection<TaskRequiredGroup> RequiredGroups { get; set; } = new();
    public ICollection<TaskDocument> Documents { get; set; } = new();
}
```

## 5. Frontend Özeti
- **React Router 7** ile rota yapısı (`App.tsx`).
- **TanStack Query** data-fetching örnekleri (`useTasks`, `useCreateTask`).
- **MUI** tasarım sistemi; görev listesi `DataGrid`, görev oluşturma formu `DesktopDateTimePicker`.
- **Belge yükleme formu** `DocumentUploadForm` bileşeni ve Vitest testi.

### Route Haritası
```
/login → Kimlik doğrulama
/company → Şirket seçimi
/tasks → Görev listesi (filtre, DataGrid)
/tasks/new → Görev oluşturma (belge grupları)
/tasks/:taskId → Görev detayı, eksik belgeler
/document-setup → Belge tipleri & grupları yönetimi
/admin/users → Kullanıcı ve rol yönetimi
/history → Audit log görüntüleme
/reports → SLA & eksik belge raporları
```

## 6. Testler
```bash
# Backend
cd backend/tests/DocumentTasking.Tests
dotnet test

# Frontend
npm run test -- --run
```

## 7. Docker Compose Hizmetleri
`docker-compose.yml`
- `api`: ASP.NET Core API (port 8080)
- `web`: React Vite dev server (5173)
- `mysql`: MySQL 8 veri tabanı (3307 harici port)
- `mailhog`: SMTP yakalayıcı (8025 UI)
- `.env` dosyası gerektirmez; varsayılan şifreler dosyada.

## 8. Audit ve Loglama
- Serilog hem console hem `logs/audit-.log` dosyasına yazar.
- Audit olay tipleri örnekleri: `TaskCreated`, `TaskStatusChanged`, `DocumentUploaded`.
- `AuditLoggingMiddleware` HTTP isteği tamamlandıktan sonra `HttpContext.Items` içindeki audit girdilerini kalıcı hale getirir.

## 9. Planlanmış Hatırlatmalar
- `ReminderRunner` eksik belge tiplerini hesaplar (`TaskService.GetMissingDocumentTypesAsync`).
- Ayarlanabilir konfigürasyon: `Reminders:IntervalHours`, `Reminders:MaxCount`.
- E-posta şablonu Razor ile çok dilli desteğe hazır (varsayılan `tr-TR`).

## 10. Geliştirici Notları
- Dosya depolama katmanı demo amaçlı `FilePath` olarak bırakılmıştır; üretimde `IFileStorage` implementasyonu ile genişletin.
- Rate limiting, CORS (`Program.cs`) ve JWT doğrulaması iskelet olarak hazır.
- `ApplicationDbContext` MySQL kullanımına göre Pomelo sağlayıcısı ile yapılandırılmıştır.
- Çalışma saatleri UTC olarak saklanır; UI’da `Europe/Istanbul` varsayılanı ile dönüştürülür.

> Bu repo, üretim uygulamasının çekirdek iskeletini sunar. Gerektiğinde domain kurallarını genişletip gerçek kimlik doğrulama, dosya depolama ve altyapı servisleri ile entegre edebilirsiniz.
