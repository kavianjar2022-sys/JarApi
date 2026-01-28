# راهنمای امنیتی (Security Guide)

## نکات امنیتی مهم (Important Security Notes)

### ⚠️ اقدامات ضروری قبل از استقرار (Required Actions Before Deployment)

1. **تنظیم کلید JWT قوی**
   - کلید JWT باید حداقل 32 کاراکتر باشد
   - از کاراکترهای تصادفی و پیچیده استفاده کنید
   - هرگز کلید پیش‌فرض را استفاده نکنید
   - برای تولید کلید قوی می‌توانید از این دستور استفاده کنید:
   ```bash
   # PowerShell
   -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
   
   # Linux/Mac
   openssl rand -base64 48
   ```

2. **امن‌سازی Connection String دیتابیس**
   - هرگز رمز عبور دیتابیس را در فایل‌های تنظیمات commit نکنید
   - از User Secrets در محیط Development استفاده کنید
   - در Production از Environment Variables یا Azure Key Vault استفاده کنید

3. **تنظیم HTTPS**
   - در Production حتماً از HTTPS استفاده کنید
   - گواهی SSL معتبر داشته باشید
   - HTTP Strict Transport Security (HSTS) را فعال کنید

4. **محدودیت درخواست (Rate Limiting)**
   - برای جلوگیری از حملات DDoS و Brute Force
   - محدودیت تعداد درخواست در بازه زمانی مشخص

## استفاده از User Secrets (برای Development)

```bash
# تنظیم Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=JarApiDb;User Id=YourUser;Password=YourPassword;TrustServerCertificate=True"

# تنظیم JWT Secret Key
dotnet user-secrets set "JwtSettings:SecretKey" "YourVeryStrongAndRandomSecretKeyHere"
```

## استفاده از Environment Variables (برای Production)

### Windows
```powershell
$env:ConnectionStrings__DefaultConnection="Server=.;Database=JarApiDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
$env:JwtSettings__SecretKey="YourVeryStrongAndRandomSecretKeyHere"
```

### Linux/Mac
```bash
export ConnectionStrings__DefaultConnection="Server=.;Database=JarApiDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
export JwtSettings__SecretKey="YourVeryStrongAndRandomSecretKeyHere"
```

## بهترین شیوه‌های امنیتی (Security Best Practices)

### 1. مدیریت رمز عبور
- ✅ حداقل طول رمز عبور: 6 کاراکتر (توصیه می‌شود 8 یا بیشتر)
- ✅ نیاز به حروف بزرگ و کوچک
- ✅ نیاز به عدد
- ⚠️ توصیه: نیاز به کاراکترهای خاص را نیز فعال کنید

### 2. توکن JWT
- ✅ زمان انقضا: 60 دقیقه (قابل تنظیم)
- ⚠️ برای API‌های حساس، زمان کمتری (15-30 دقیقه) استفاده کنید
- 🔄 Refresh Token را پیاده‌سازی کنید

### 3. CORS
- ✅ فقط originهای مورد اعتماد را اجازه دهید
- ⚠️ در Production، لیست originها را به‌دقت مشخص کنید
- ❌ از `AllowAnyOrigin` در Production استفاده نکنید

### 4. Logging
- ✅ همه خطاها را log کنید
- ❌ هرگز رمز عبور یا اطلاعات حساس را log نکنید
- ✅ IP آدرس و timestamp را ثبت کنید

### 5. Input Validation
- ✅ همه ورودی‌ها را validate کنید
- ✅ از Data Annotations استفاده کنید
- ✅ از Fluent Validation برای validation پیچیده استفاده کنید

## چک‌لیست امنیتی قبل از Deploy

- [ ] کلید JWT قوی و منحصر به فرد تنظیم شده است
- [ ] رمز عبور دیتابیس از فایل‌های config حذف شده است
- [ ] HTTPS فعال است
- [ ] CORS به‌درستی پیکربندی شده است
- [ ] Rate Limiting اضافه شده است
- [ ] Logging به‌درستی کار می‌کند
- [ ] همه endpoint‌های حساس با `[Authorize]` محافظت شده‌اند
- [ ] Error Messages اطلاعات حساس را فاش نمی‌کنند
- [ ] Database Migrations اجرا شده است
- [ ] Health Check endpoint کار می‌کند

## گزارش مشکلات امنیتی (Reporting Security Issues)

اگر مشکل امنیتی پیدا کردید، لطفاً به‌صورت خصوصی گزارش دهید:
- از طریق GitHub Security Advisory
- یا ایمیل به تیم توسعه

## منابع بیشتر (Additional Resources)

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
