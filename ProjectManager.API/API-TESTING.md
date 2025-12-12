# 🧪 API Testing Guide

## 📋 Kurulum ve Çalıştırma

### 1. Gerekli Paketleri Yükle

```bash
cd front/ProjectManager.API

# PostgreSQL ve EF Core
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools

# JWT Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next

# AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Validation
dotnet add package FluentValidation.AspNetCore

# Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console

# JWT Token Handler
dotnet add package System.IdentityModel.Tokens.Jwt

# Restore packages
dotnet restore
```

### 2. PostgreSQL Database Oluştur

```bash
# PostgreSQL'e bağlan
psql -U postgres

# Database oluştur
CREATE DATABASE "ProjectManagerDb";

# Çıkış
\q
```

### 3. Connection String Ayarla

**appsettings.json** dosyasında PostgreSQL connection string'i güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ProjectManagerDb;Username=postgres;Password=SIZIN_SIFRENIZ"
  }
}
```

### 4. Migration Oluştur ve Database'i Güncelle

```bash
# EF Core Tools kur (eğer yoksa)
dotnet tool install --global dotnet-ef

# Migration oluştur
dotnet ef migrations add InitialCreate

# Database'i güncelle (tabloları oluştur)
dotnet ef database update

# Başarılı olduğunda şunları göreceksiniz:
# - Users tablosu
# - UserSettings tablosu
# - Workspaces tablosu
# - Projects tablosu
# - Tasks tablosu
# - DailyReports tablosu
# - ve diğer tüm tablolar
```

### 5. Uygulamayı Çalıştır

```bash
# Development modunda çalıştır
dotnet run

# veya watch mode (otomatik reload)
dotnet watch run
```

**Erişim URL'leri:**
- API Base: `https://localhost:7001`
- Swagger UI: `https://localhost:7001/swagger`
- Health Check: `https://localhost:7001/health`

---

## 🔥 API Endpoint Testleri

### Test için Swagger UI Kullanımı

1. Tarayıcıda açın: `https://localhost:7001/swagger`
2. Tüm endpoint'leri göreceksiniz
3. "Try it out" butonuna tıklayarak test edebilirsiniz

---

### 1. Health Check

```bash
curl -X GET https://localhost:7001/health
```

**Beklenen Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

### 2. Register (Kayıt Ol)

**Endpoint:** `POST /api/auth/register`

```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "email": "admin@projectmanager.com",
    "password": "Admin123!",
    "fullName": "Admin User"
  }'
```

**Başka kullanıcılar:**

```json
// User 2
{
  "username": "john.doe",
  "email": "john@example.com",
  "password": "John123!",
  "fullName": "John Doe"
}

// User 3
{
  "username": "jane.smith",
  "email": "jane@example.com",
  "password": "Jane123!",
  "fullName": "Jane Smith"
}
```

**Beklenen Response (201 Created):**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "admin",
    "email": "admin@projectmanager.com",
    "fullName": "Admin User",
    "role": "Member",
    "avatar": null,
    "phone": null,
    "bio": null,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "errors": null
}
```

---

### 3. Login (Giriş Yap)

**Endpoint:** `POST /api/auth/login`

```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!",
    "rememberMe": true
  }'
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "username": "admin",
      "email": "admin@projectmanager.com",
      "fullName": "Admin User",
      "role": "Member",
      "avatar": null
    },
    "expiresAt": "2024-01-16T10:30:00Z"
  },
  "errors": null
}
```

**ÖNEMLİ:** Token'ı kopyalayın! Sonraki isteklerde kullanacaksınız.

---

### 4. Get Current User (Mevcut Kullanıcı)

**Endpoint:** `GET /api/auth/me`

```bash
curl -X GET https://localhost:7001/api/auth/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "admin",
    "email": "admin@projectmanager.com",
    "fullName": "Admin User",
    "role": "Member",
    "avatar": null,
    "phone": null,
    "bio": null,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "errors": null
}
```

---

### 5. Change Password (Şifre Değiştir)

**Endpoint:** `POST /api/auth/change-password`

```bash
curl -X POST https://localhost:7001/api/auth/change-password \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "Admin123!",
    "newPassword": "NewAdmin123!",
    "confirmPassword": "NewAdmin123!"
  }'
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Password changed successfully",
  "data": null,
  "errors": null
}
```

---

### 6. Upload Avatar (Avatar Yükle)

**Endpoint:** `POST /api/auth/avatar`

```bash
curl -X POST https://localhost:7001/api/auth/avatar \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "avatarBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
  }'
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Avatar uploaded successfully",
  "data": {
    "avatarUrl": "data:image/png;base64,..."
  },
  "errors": null
}
```

---

### 7. Delete Avatar (Avatar Sil)

**Endpoint:** `DELETE /api/auth/avatar`

```bash
curl -X DELETE https://localhost:7001/api/auth/avatar \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Avatar deleted successfully",
  "data": null,
  "errors": null
}
```

---

### 8. Check Username Availability

**Endpoint:** `GET /api/auth/check-username/{username}`

```bash
curl -X GET https://localhost:7001/api/auth/check-username/newuser
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Username is available",
  "data": {
    "isAvailable": true
  },
  "errors": null
}
```

---

### 9. Check Email Availability

**Endpoint:** `GET /api/auth/check-email/{email}`

```bash
curl -X GET https://localhost:7001/api/auth/check-email/new@example.com
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Email is available",
  "data": {
    "isAvailable": true
  },
  "errors": null
}
```

---

### 10. Refresh Token

**Endpoint:** `POST /api/auth/refresh`

```bash
curl -X POST https://localhost:7001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "token": "YOUR_CURRENT_JWT_TOKEN"
  }'
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "token": "NEW_JWT_TOKEN_HERE"
  },
  "errors": null
}
```

---

## 🔒 Authentication Test Senaryoları

### Senaryo 1: Başarılı Kayıt ve Giriş

```bash
# 1. Yeni kullanıcı kaydı
POST /api/auth/register
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test123!",
  "fullName": "Test User"
}

# 2. Giriş yap ve token al
POST /api/auth/login
{
  "username": "testuser",
  "password": "Test123!"
}

# 3. Token ile kullanıcı bilgilerini al
GET /api/auth/me
Headers: Authorization: Bearer {token}
```

### Senaryo 2: Duplicate Username/Email

```bash
# 1. İlk kullanıcı (başarılı)
POST /api/auth/register
{
  "username": "duplicate",
  "email": "duplicate@example.com",
  "password": "Test123!",
  "fullName": "First User"
}

# 2. Aynı username ile tekrar (başarısız - 400)
POST /api/auth/register
{
  "username": "duplicate",
  "email": "different@example.com",
  "password": "Test123!",
  "fullName": "Second User"
}
# Beklenen: "Username already exists"

# 3. Aynı email ile tekrar (başarısız - 400)
POST /api/auth/register
{
  "username": "different",
  "email": "duplicate@example.com",
  "password": "Test123!",
  "fullName": "Third User"
}
# Beklenen: "Email already exists"
```

### Senaryo 3: Zayıf Şifre

```bash
POST /api/auth/register
{
  "username": "weakpass",
  "email": "weak@example.com",
  "password": "123",
  "fullName": "Weak Password User"
}
# Beklenen: 400 - "Password must be at least 8 characters"
```

### Senaryo 4: Invalid Login

```bash
# Yanlış şifre
POST /api/auth/login
{
  "username": "testuser",
  "password": "WrongPassword123!"
}
# Beklenen: 401 - "Invalid username or password"

# Olmayan kullanıcı
POST /api/auth/login
{
  "username": "nonexistent",
  "password": "Test123!"
}
# Beklenen: 401 - "Invalid username or password"
```

### Senaryo 5: Unauthorized Access

```bash
# Token olmadan protected endpoint'e erişim
GET /api/auth/me
# Beklenen: 401 Unauthorized

# Geçersiz token
GET /api/auth/me
Headers: Authorization: Bearer invalid_token
# Beklenen: 401 Unauthorized
```

---

## 📊 Postman Collection

### Collection Ayarı

1. **Postman'de yeni Collection oluştur:** "Project Manager API"
2. **Base URL variable ekle:**
   - Variable: `baseUrl`
   - Value: `https://localhost:7001`
3. **Token variable ekle:**
   - Variable: `authToken`
   - Value: (login'den sonra otomatik güncellenecek)

### Pre-request Script (Login için)

Login endpoint'inde Test sekmesine ekleyin:

```javascript
// Login başarılı olursa token'ı kaydet
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    if (jsonData.success && jsonData.data.token) {
        pm.collectionVariables.set("authToken", jsonData.data.token);
        console.log("Token saved:", jsonData.data.token);
    }
}
```

### Authorization Header (Protected endpoints için)

Tüm protected endpoint'lerde Authorization sekmesinde:
- Type: Bearer Token
- Token: `{{authToken}}`

---

## ✅ Test Checklist

### Authentication Tests
- [ ] Register yeni kullanıcı
- [ ] Register duplicate username (fail)
- [ ] Register duplicate email (fail)
- [ ] Register zayıf şifre (fail)
- [ ] Login doğru credentials
- [ ] Login yanlış password (fail)
- [ ] Login olmayan user (fail)
- [ ] Get current user (authenticated)
- [ ] Get current user (unauthenticated - fail)
- [ ] Change password başarılı
- [ ] Change password yanlış current password (fail)
- [ ] Upload avatar
- [ ] Delete avatar
- [ ] Check username availability
- [ ] Check email availability
- [ ] Refresh token

### Database Verification
- [ ] Users tablosunda yeni kayıtlar var
- [ ] UserSettings otomatik oluşturuldu
- [ ] Password'ler hash'lenmiş (BCrypt)
- [ ] Timestamps doğru (UTC)

---

## 🐛 Sorun Giderme

### Migration Hatası

```bash
# Hata: "A network-related or instance-specific error occurred"
# Çözüm: PostgreSQL çalışıyor mu kontrol et

# Windows
sc query postgresql-x64-15

# Linux/Mac
sudo systemctl status postgresql
```

### Connection String Hatası

```bash
# Hata: "password authentication failed"
# Çözüm: appsettings.json'da şifreyi kontrol et

# PostgreSQL şifresini değiştirmek için:
psql -U postgres
ALTER USER postgres PASSWORD 'yeni_sifre';
\q
```

### JWT Token Hatası

```bash
# Hata: "IDX10603: The algorithm 'HS256' requires the SecurityKey.KeySize to be greater than '128' bits"
# Çözüm: JWT SecretKey en az 32 karakter olmalı

# appsettings.json'da kontrol et:
"JWT": {
  "SecretKey": "your-super-secret-jwt-key-minimum-32-characters-long"
}
```

### Swagger Hatası

```bash
# Swagger açılmıyor
# Çözüm: Development modunda mı çalıştırıyorsunuz?

# Environment kontrol:
echo $ASPNETCORE_ENVIRONMENT  # Linux/Mac
echo %ASPNETCORE_ENVIRONMENT%  # Windows

# Development modunda çalıştır:
export ASPNETCORE_ENVIRONMENT=Development  # Linux/Mac
set ASPNETCORE_ENVIRONMENT=Development      # Windows
dotnet run
```

---

## 📈 Performans Testleri

### Load Testing (Apache Bench)

```bash
# 100 istek, 10 concurrent
ab -n 100 -c 10 -H "Authorization: Bearer YOUR_TOKEN" https://localhost:7001/api/auth/me

# POST request ile
ab -n 100 -c 10 -p login.json -T application/json https://localhost:7001/api/auth/login
```

### Veritabanı İndeksleri Kontrolü

```sql
-- PostgreSQL'de index'leri kontrol et
SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY tablename, indexname;
```

---

## 🎯 Sonraki Adımlar

✅ **Tamamlandı:**
- Authentication (Register, Login, Password, Avatar)
- JWT Token management
- User management
- Database models (12 entity)
- PostgreSQL integration

🚧 **Devam Eden:**
- Workspace CRUD endpoints
- Project CRUD endpoints
- Task CRUD endpoints
- Daily Report CRUD endpoints
- User Settings endpoints

📋 **Planlanan:**
- Kanban Board endpoints
- Statistics/Dashboard endpoints
- File upload için storage
- Email notifications
- Real-time updates (SignalR)

---

## 💡 İpuçları

1. **Token'ı sakla:** Login yaptıktan sonra token'ı environment variable veya Postman variable olarak kaydet
2. **Swagger kullan:** En hızlı test yöntemi Swagger UI
3. **Database kontrol:** pgAdmin veya psql ile database'i kontrol et
4. **Logs takip et:** `logs/` klasöründeki log dosyalarını incele
5. **HTTPS:** Development'ta self-signed certificate uyarısı normal

---

**Test başarılarınızı bekliyoruz! 🚀**

**Version:** 1.0.0  
**Last Updated:** 2024-01-15