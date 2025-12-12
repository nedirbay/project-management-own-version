# 🚀 Project Manager - Backend Implementation Guide

## 📋 Overview

Bu doküman, Project Manager uygulamasının ASP.NET Core backend implementasyonu için kapsamlı bir kılavuzdur.

**Backend Teknolojisi:** ASP.NET Core 8.0 Web API  
**Veritabanı:** SQL Server / PostgreSQL  
**Authentication:** JWT (JSON Web Tokens)

---

## 📚 Dokümantasyon Dosyaları

- **backend.txt** - Detaylı backend spesifikasyonu
  - Tüm Entity modelleri
  - Tüm DTOs
  - Tüm API Endpoints
  - Business Rules
  - Authorization kuralları
  - Örnek Request/Response'lar
  - Implementation sırası

---

## 🎯 Hızlı Başlangıç

### 1. Proje Oluşturma

```bash
# Web API projesi oluştur
dotnet new webapi -n ProjectManager.API

# Solution oluştur
dotnet new sln -n ProjectManager

# Projeyi solution'a ekle
dotnet sln add ProjectManager.API/ProjectManager.API.csproj
```

### 2. Gerekli NuGet Paketleri

```bash
cd ProjectManager.API

# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Authentication & JWT
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package BCrypt.Net-Next

# AutoMapper
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Validation
dotnet add package FluentValidation
dotnet add package FluentValidation.AspNetCore

# Swagger
dotnet add package Swashbuckle.AspNetCore

# Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

---

## 📁 Proje Yapısı

```
ProjectManager.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── WorkspacesController.cs
│   ├── ProjectsController.cs
│   ├── TasksController.cs
│   └── ReportsController.cs
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Workspace.cs
│   │   ├── Project.cs
│   │   ├── Task.cs
│   │   └── DailyReport.cs
│   └── DTOs/
│       ├── UserDtos.cs
│       ├── WorkspaceDtos.cs
│       ├── ProjectDtos.cs
│       ├── TaskDtos.cs
│       └── ReportDtos.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DbInitializer.cs
│   └── Migrations/
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Helpers/
│   ├── AutoMapperProfile.cs
│   ├── JwtHelper.cs
│   └── FileUploadHelper.cs
├── Middlewares/
│   ├── ErrorHandlingMiddleware.cs
│   └── LoggingMiddleware.cs
├── Validators/
│   ├── UserValidator.cs
│   └── ProjectValidator.cs
├── Program.cs
└── appsettings.json
```

---

## 🗄️ Veritabanı Modelleri

### Temel Entities

1. **User** - Kullanıcılar (Admin, WorkspaceAdmin, Member)
2. **UserSettings** - Kullanıcı ayarları (tema, dil, bildirimler)
3. **Workspace** - Çalışma alanları
4. **WorkspaceMember** - Workspace üyelikleri (Many-to-Many)
5. **Project** - Projeler
6. **ProjectMember** - Proje üyelikleri (Many-to-Many)
7. **Task** - Görevler
8. **TaskAssignee** - Görev atamaları (Many-to-Many)
9. **SubTask** - Alt görevler
10. **TaskComment** - Görev yorumları
11. **TaskAttachment** - Görev ekleri
12. **DailyReport** - Günlük raporlar

> Detaylı model tanımları için `backend.txt` dosyasına bakınız.

---

## 🔐 Authentication & Authorization

### JWT Token Yapısı

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "username": "johndoe",
  "role": "Admin",
  "exp": 1234567890
}
```

### Rol Tabanlı Yetkilendirme

- **Admin**: Tüm işlemler
- **WorkspaceAdmin**: Kendi workspace'lerinde tüm işlemler
- **Member**: Atandığı projeler ve görevler

### Authorization Attribute'ları

```csharp
[Authorize] // Tüm authenticated kullanıcılar
[Authorize(Roles = "Admin")] // Sadece Admin
[Authorize(Roles = "Admin,WorkspaceAdmin")] // Admin veya WorkspaceAdmin
```

---

## 🔌 API Endpoints Özeti

### Authentication
```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
GET    /api/auth/me
```

### Users
```
GET    /api/users
POST   /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
DELETE /api/users/{id}
POST   /api/users/{id}/avatar
POST   /api/users/{id}/change-password
```

### Workspaces
```
GET    /api/workspaces
POST   /api/workspaces
GET    /api/workspaces/{id}
PUT    /api/workspaces/{id}
DELETE /api/workspaces/{id}
POST   /api/workspaces/{id}/members
DELETE /api/workspaces/{id}/members/{userId}
```

### Projects
```
GET    /api/projects
POST   /api/projects
GET    /api/projects/{id}
PUT    /api/projects/{id}
DELETE /api/projects/{id}
PATCH  /api/projects/{id}/progress
```

### Tasks
```
GET    /api/tasks
POST   /api/tasks
GET    /api/tasks/{id}
PUT    /api/tasks/{id}
DELETE /api/tasks/{id}
PATCH  /api/tasks/{id}/status
PATCH  /api/tasks/{id}/order
POST   /api/tasks/{id}/subtasks
POST   /api/tasks/{id}/comments
```

### Daily Reports
```
GET    /api/reports
POST   /api/reports
GET    /api/reports/{id}
PUT    /api/reports/{id}
DELETE /api/reports/{id}
GET    /api/reports/my
```

> Tüm endpoint detayları için `backend.txt` dosyasına bakınız.

---

## ⚙️ Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectManager;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  },
  "JWT": {
    "SecretKey": "your-super-secret-key-min-32-chars",
    "Issuer": "ProjectManagerAPI",
    "Audience": "ProjectManagerClient",
    "ExpirationMinutes": 1440
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  },
  "FileUpload": {
    "MaxSizeInMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"],
    "StoragePath": "wwwroot/uploads"
  }
}
```

---

## 🚀 Implementation Sırası

### Phase 1: Core Setup (1-2 gün)
- ✅ Proje yapısı oluştur
- ✅ NuGet paketlerini yükle
- ✅ DbContext setup
- ✅ Entity modelleri oluştur
- ✅ Migration oluştur ve çalıştır

### Phase 2: Authentication (2-3 gün)
- ✅ JWT helper oluştur
- ✅ AuthController implement et
- ✅ Login/Register endpoints
- ✅ Password hashing
- ✅ Token generation

### Phase 3: User Management (2 gün)
- ✅ UserController implement et
- ✅ UserSettings endpoint'leri
- ✅ Avatar upload
- ✅ Password change

### Phase 4: Workspace & Projects (3-4 gün)
- ✅ WorkspaceController implement et
- ✅ ProjectController implement et
- ✅ Member management
- ✅ Authorization policies

### Phase 5: Tasks & Kanban (3-4 gün)
- ✅ TaskController implement et
- ✅ SubTask endpoints
- ✅ Task status update
- ✅ Kanban order management

### Phase 6: Daily Reports (2 gün)
- ✅ ReportController implement et
- ✅ Filtreleme özellikleri
- ✅ Rol bazlı erişim

### Phase 7: Testing & Polish (2-3 gün)
- ✅ Unit testler
- ✅ Integration testler
- ✅ API dokümantasyonu
- ✅ Performance optimization

**Toplam Süre:** ~3-4 hafta (tek kişi için)

---

## 🧪 Testing

### Unit Tests
```bash
# Test projesi oluştur
dotnet new xunit -n ProjectManager.Tests

# Test paketleri
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### API Testing
- **Swagger UI**: `https://localhost:7001/swagger`
- **Postman**: Collection export edilebilir
- **Thunder Client**: VS Code extension

---

## 📊 Database Migration

```bash
# Migration oluştur
dotnet ef migrations add InitialCreate

# Database güncelle
dotnet ef database update

# Migration geri al
dotnet ef migrations remove
```

---

## 🔒 Security Best Practices

1. ✅ HTTPS kullan
2. ✅ JWT token'ları güvenli sakla
3. ✅ Password'ları hash'le (BCrypt)
4. ✅ Input validation yap
5. ✅ CORS düzgün yapılandır
6. ✅ Rate limiting ekle
7. ✅ SQL Injection'dan korun (EF Core otomatik korur)
8. ✅ Sensitive data'yı loglama
9. ✅ API versioning kullan
10. ✅ Regular security audit'ler yap

---

## 🐛 Error Handling

### Global Error Handler
```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();
```

### Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "Email",
        "message": "Email is already in use"
      }
    ]
  }
}
```

---

## 📈 Performance Optimization

1. **Caching**: Redis veya Memory Cache
2. **Pagination**: Tüm liste endpoint'lerinde
3. **Async/Await**: Tüm I/O operasyonlarında
4. **Database Indexing**: Sık sorgulanan kolonlarda
5. **Lazy Loading**: İlişkili entity'lerde dikkatli kullan
6. **Query Optimization**: Select projections kullan
7. **Response Compression**: Gzip
8. **Connection Pooling**: Default olarak aktif

---

## 📝 Logging

### Serilog Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Log Levels
- **Information**: User actions, API calls
- **Warning**: Business rule violations
- **Error**: Exceptions, validation errors
- **Critical**: System failures, security breaches

---

## 🚀 Deployment

### Development
```bash
dotnet run
```

### Production
```bash
# Publish
dotnet publish -c Release -o ./publish

# Run
cd publish
dotnet ProjectManager.API.dll
```

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "ProjectManager.API.dll"]
```

---

## 🔗 Frontend Entegrasyonu

### Axios Configuration
```javascript
const API_BASE_URL = 'https://localhost:7001/api';

axios.defaults.baseURL = API_BASE_URL;
axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
```

### CORS Configuration
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        builder => builder
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
```

---

## 📚 Kaynaklar

### Dokümantasyon
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT.io](https://jwt.io)

### Paketler
- [AutoMapper](https://automapper.org)
- [FluentValidation](https://fluentvalidation.net)
- [Serilog](https://serilog.net)

---

## ✅ Checklist

### Pre-Development
- [ ] backend.txt detaylı incelendi
- [ ] Veritabanı modelleri anlaşıldı
- [ ] API endpoint'leri belirlendi
- [ ] Authorization kuralları netleşti

### Development
- [ ] Proje oluşturuldu
- [ ] NuGet paketleri yüklendi
- [ ] DbContext yapılandırıldı
- [ ] Migrations çalıştırıldı
- [ ] Authentication implement edildi
- [ ] Controllers oluşturuldu
- [ ] Business rules uygulandı
- [ ] Validation eklendi

### Testing
- [ ] Unit testler yazıldı
- [ ] API testleri yapıldı
- [ ] Postman collection hazırlandı
- [ ] Frontend ile test edildi

### Deployment
- [ ] Production settings yapılandırıldı
- [ ] HTTPS enabled
- [ ] CORS düzgün ayarlandı
- [ ] Database connection güvenli
- [ ] Logging aktif
- [ ] Error handling test edildi

---

## 🆘 Sorun Giderme

### Migration Hataları
```bash
# Drop database
dotnet ef database drop

# Remove migrations
dotnet ef migrations remove

# Yeniden oluştur
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### JWT Hataları
- Secret key en az 32 karakter olmalı
- Token expiration ayarlarını kontrol et
- Bearer token formatını kontrol et

### CORS Hataları
- AllowedOrigins'i kontrol et
- AllowCredentials ile WithOrigins birlikte kullan
- Preflight request'leri kontrol et

---

## 👥 İletişim & Destek

- **Dokümantasyon**: backend.txt
- **Frontend Repo**: ../project-managament
- **Issues**: GitHub Issues

---

**Başarılar! 🚀**

**Version:** 1.0  
**Last Updated:** 2024