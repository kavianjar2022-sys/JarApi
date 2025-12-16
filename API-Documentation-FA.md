# مستندات کامل API احراز هویت و مدیریت دسترسی (JarApi)

## ۱. احراز هویت و ثبت‌نام

### ثبت‌نام کاربر

- **Endpoint:** `POST /api/auth/register`
- **Body:**

```json
{
  "personnelCode": "string (یونیک)",
  "password": "string (حداقل 6 کاراکتر)",
  "firstName": "string",
  "lastName": "string",
  "faceCode": "string (اختیاری)",
  "birthDate": "yyyy-MM-dd (اختیاری)",
  "hireDate": "yyyy-MM-dd (اختیاری)",
  "mobileNumber": "string (اختیاری)",
  "nationalCode": "string (اختیاری)",
  "insuranceCode": "string (اختیاری)",
  "homePhoneNumber": "string (اختیاری)"
}
```

- **پاسخ موفق:** توکن JWT و اطلاعات کاربر

### ورود کاربر

- **Endpoint:** `POST /api/auth/login`
- **Body:**

```json
{
  "personnelCode": "string",
  "password": "string"
}
```

- **پاسخ موفق:** توکن JWT و اطلاعات کاربر

### فراموشی رمز عبور (دو مرحله‌ای)

- **مرحله ۱:** بررسی اطلاعات کاربر
  - **Endpoint:** `POST /api/auth/forgot-password`
  - **Body:**
    ```json
    {
      "personnelCode": "string",
      "nationalCode": "string",
      "mobileNumber": "string"
    }
    ```
  - **پاسخ موفق:**
    ```json
    {
      "success": true,
      "userId": "string",
      "message": "تایید شد. اکنون می‌توانید رمز عبور را تغییر دهید."
    }
    ```
- **مرحله ۲:** تغییر رمز عبور
  - **Endpoint:** `POST /api/auth/reset-password`
  - **Body:**
    ```json
    {
      "userId": "string",
      "newPassword": "string"
    }
    ```
  - **پاسخ موفق:** پیام موفقیت

## ۲. دریافت دسترسی‌ها و اطلاعات کاربر

- **Endpoint:** `GET /api/auth/user-permissions`
- **Header:**
  `Authorization: Bearer <JWT Token>`
- **پاسخ:**

```json
{
  "personnelCode": "string",
  "firstName": "string",
  "lastName": "string",
  "roles": ["string"],
  "isGlobalAccess": true/false,
  "companies": [
    { "id": 1, "name": "string", "code": "string" }
  ],
  "menus": [ ... ],
  "widgets": [ ... ]
}
```

## ۳. مدیریت نقش‌ها (Role)

- **دریافت همه نقش‌ها:** `GET /api/role`
- **ایجاد نقش:** `POST /api/role` (Body: `{ "roleName": "string" }`)
- **تخصیص نقش به کاربر:**
  - سراسری: `POST /api/role/assign-role-to-user` (Body: `{ "personnelCode": "string", "roleName": "string" }`)
  - در شرکت: `POST /api/role/assign-role-to-user-in-company` (Body: `{ "personnelCode": "string", "roleId": "string", "companyId": 1 }`)
- **حذف نقش از کاربر:**
  - سراسری: `POST /api/role/remove-role-from-user` (Body: مانند تخصیص)
  - در شرکت: `POST /api/role/remove-role-from-user-in-company` (Body: مانند تخصیص در شرکت)
- **تخصیص منو/ویجت به نقش:**
  - منو: `POST /api/role/assign-menus` (Body: `{ "roleId": "string", "menus": [ { "menuId": 1, "canView": true, ... } ] }`)
  - ویجت: `POST /api/role/assign-widgets` (Body: `{ "roleId": "string", "widgets": [ { "widgetId": 1, "canView": true, ... } ] }`)
- **دریافت منوها/ویجت‌های نقش:**
  - منو: `GET /api/role/{roleId}/menus`
  - ویجت: `GET /api/role/{roleId}/widgets`

## ۴. مدیریت شرکت‌ها (Company)

- **دریافت همه:** `GET /api/company`
- **ایجاد:** `POST /api/company` (Body: مدل Company)
- **ویرایش:** `PUT /api/company/{id}` (Body: مدل Company)
- **حذف:** `DELETE /api/company/{id}`

## ۵. مدیریت منوها (Menu)

- **دریافت همه:** `GET /api/menu`
- **ایجاد:** `POST /api/menu` (Body: مدل CreateMenuDto)
- **ویرایش:** `PUT /api/menu/{id}` (Body: مدل UpdateMenuDto)
- **حذف:** `DELETE /api/menu/{id}`

## ۶. مدیریت ویجت‌ها (Widget)

- **دریافت همه:** `GET /api/widget`
- **ایجاد:** `POST /api/widget` (Body: مدل CreateWidgetDto)
- **ویرایش:** `PUT /api/widget/{id}` (Body: مدل UpdateWidgetDto)
- **حذف:** `DELETE /api/widget/{id}`

## ۷. نکات امنیتی و فنی

- همه مسیرها (به جز ثبت‌نام، لاگین و فراموشی رمز عبور) نیاز به توکن JWT دارند.
- توکن JWT باید در هدر Authorization با فرمت `Bearer <token>` ارسال شود.
- اطلاعات نقش‌ها، شرکت‌ها، منوها و ویجت‌ها بر اساس دسترسی کاربر بازگردانده می‌شود.
- تمام پیام‌های خطا و موفقیت به زبان فارسی و ساختارمند هستند.

---

این فایل را می‌توانید به تیم فرانت‌اند تحویل دهید. اگر نمونه‌ای از هر مدل یا درخواست خاصی نیاز دارید، اعلام کنید تا اضافه کنم.
