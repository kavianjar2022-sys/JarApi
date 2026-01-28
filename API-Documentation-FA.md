# 📘 مستندات کامل API - سیستم مدیریت دسترسی JarApi

**نسخه:** 2.0  
**تاریخ:** 1404/10/17  
**Base URL:** `http://localhost:5257` (Development)

---

## 🔐 احراز هویت (Authentication)

### ثبت‌نام کاربر جدید

**Endpoint:** `POST /api/auth/register`  
**نیاز به احراز هویت:** ❌ خیر

**Request Body:**

```json
{
  "personnelCode": "1001",
  "password": "Pass@123",
  "firstName": "علی",
  "lastName": "احمدی",
  "faceCode": "FC001",
  "birthDate": "1370-05-15",
  "hireDate": "1400-01-01",
  "mobileNumber": "09123456789",
  "insuranceCode": "1234567890",
  "homePhoneNumber": "02112345678"
}
```

**پاسخ موفق (200):**

```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "ثبت‌نام با موفقیت انجام شد",
  "userInfo": {
    "userId": "uuid-here",
    "personnelCode": "1001",
    "firstName": "علی",
    "lastName": "احمدی",
    "faceCode": "FC001",
    "birthDate": "1370-05-15",
    "hireDate": "1400-01-01",
    "mobileNumber": "09123456789",
    "insuranceCode": "1234567890",
    "homePhoneNumber": "02112345678"
  }
}
```

**پاسخ خطا (400):**

```json
{
  "success": false,
  "message": "کاربری با این کد پرسنلی وجود دارد"
}
```

### ورود کاربر

**Endpoint:** `POST /api/auth/login`  
**نیاز به احراز هویت:** ❌ خیر

**Request Body:**

```json
{
  "personnelCode": "1001",
  "password": "Pass@123"
}
```

**پاسخ موفق (200):**

```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "ورود با موفقیت انجام شد",
  "userInfo": {
    "userId": "uuid-here",
    "personnelCode": "1001",
    "firstName": "علی",
    "lastName": "احمدی",
    "faceCode": "FC001",
    "birthDate": "1370-05-15",
    "hireDate": "1400-01-01",
    "mobileNumber": "09123456789",
    "insuranceCode": "1234567890",
    "homePhoneNumber": "02112345678"
  }
}
```

### فراموشی رمز عبور (گام ۱: تایید هویت)

**Endpoint:** `POST /api/auth/forgot-password`  
**نیاز به احراز هویت:** ❌ خیر

**Request Body:**

```json
{
  "personnelCode": "1001",
  "insuranceCode": "1234567890",
  "mobileNumber": "09123456789"
}
```

**توضیح:** `insuranceCode` همان کد ملی است.

**پاسخ موفق (200):**

```json
{
  "success": true,
  "userId": "uuid-here",
  "message": "تایید شد. اکنون می‌توانید رمز عبور را تغییر دهید."
}
```

### تنظیم مجدد رمز عبور (گام ۲)

**Endpoint:** `POST /api/auth/reset-password`  
**نیاز به احراز هویت:** ❌ خیر

**Request Body:**

```json
{
  "userId": "uuid-from-step-1",
  "newPassword": "NewPass@123"
}
```

**پاسخ موفق (200):**

```json
{
  "success": true,
  "message": "رمز عبور با موفقیت تغییر کرد"
}
```

---

## 👤 مدیریت کاربران

### دریافت دسترسی‌های کاربر

**Endpoint:** `GET /api/auth/user-permissions`  
**نیاز به احراز هویت:** ✅ بله  
**Header:** `Authorization: Bearer {token}`

**پاسخ موفق (200):**

```json
{
  "userId": "uuid-here",
  "personnelCode": "1001",
  "firstName": "علی",
  "lastName": "احمدی",
  "gender": 0,
  "educationDegree": {
    "id": "degree-uuid",
    "name": "کارشناسی",
    "isActive": true
  },
  "jobPosition": {
    "id": "position-uuid",
    "title": "کارشناس ارشد",
    "code": "SE",
    "description": "کارشناس ارشد نرم‌افزار",
    "level": 3,
    "parentPositionId": "parent-uuid",
    "isActive": true
  },
  "currentShift": {
    "id": "shift-uuid",
    "name": "اداری",
    "description": "شیفت اداری 8 تا 4",
    "type": 0,
    "fixedStartTime": "08:00:00",
    "fixedEndTime": "16:00:00",
    "fixedBreakMinutes": 60,
    "assignmentStartDate": "2025-01-01T00:00:00",
    "assignmentEndDate": "2025-12-31T00:00:00"
  },
  "roles": ["مدیر", "کارشناس"],
  "isGlobalAccess": false,
  "companies": [
    {
      "id": 1,
      "name": "شرکت الف",
      "code": "CO001",
      "address": "تهران، خیابان ولیعصر",
      "isActive": true
    }
  ],
  "units": [
    {
      "id": 5,
      "name": "واحد فروش",
      "code": "U005",
      "companyId": 1,
      "companyName": "شرکت الف"
    }
  ],
  "unit": {
    "id": 5,
    "name": "واحد فروش",
    "code": "U005",
    "companyId": 1,
    "companyName": "شرکت الف"
  },
  "menus": [
    {
      "id": 1,
      "name": "dashboard",
      "title": "داشبورد",
      "icon": "dashboard",
      "url": "/dashboard",
      "displayOrder": 1,
      "parentMenuId": null,
      "canView": true,
      "canCreate": false,
      "canEdit": false,
      "canDelete": false,
      "subMenus": []
    }
  ],
  "widgets": [
    {
      "id": 1,
      "name": "sales-chart",
      "title": "نمودار فروش",
      "description": "نمودار فروش ماهانه",
      "icon": "chart",
      "widgetType": "chart",
      "configuration": "{}",
      "displayOrder": 1,
      "canView": true,
      "canConfigure": false
    }
  ]
}
```

**توضیحات فیلدها:**

- `gender`: جنسیت کاربر (0=مرد، 1=زن)
- `educationDegree`: مدرک تحصیلی کاربر (null اگر تعیین نشده باشد)
- `jobPosition`: سمت شغلی کاربر (null اگر تعیین نشده باشد)
- `currentShift`: شیفت فعال فعلی کاربر (null اگر شیفتی نداشته باشد)
- `unit`: واحد اصلی کاربر (اولین واحد از لیست units)

### لیست کاربران (با فیلتر و جستجو)

**Endpoint:** `GET /api/auth/users`  
**نیاز به احراز هویت:** ✅ بله  
**Query Parameters:**

- `search` (اختیاری): جستجو در نام، نام خانوادگی، کد پرسنلی
- `personnelCode` (اختیاری): فیلتر بر اساس کد پرسنلی
- `mobileNumber` (اختیاری): فیلتر بر اساس شماره موبایل
- `insuranceCode` (اختیاری): فیلتر بر اساس کد ملی
- `gender` (اختیاری): فیلتر بر اساس جنسیت (0=مرد، 1=زن)
- `educationDegreeId` (اختیاری): فیلتر بر اساس مدرک تحصیلی
- `page` (پیش‌فرض: 1): شماره صفحه
- `pageSize` (پیش‌فرض: 20): تعداد در هر صفحه

**مثال:** `GET /api/auth/users?search=علی&gender=0&page=1&pageSize=10`

**پاسخ موفق (200):**

```json
{
  "totalCount": 45,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5,
  "users": [
    {
      "userId": "uuid-here",
      "personnelCode": "1001",
      "firstName": "علی",
      "lastName": "احمدی",
      "gender": 0,
      "educationDegreeId": "degree-uuid",
      "educationDegreeName": "کارشناسی",
      "mobileNumber": "09123456789",
      "faceCode": "FC001",
      "birthDate": "1370-05-15",
      "hireDate": "1400-01-01",
      "insuranceCode": "1234567890",
      "homePhoneNumber": "02112345678"
    }
  ]
}
```

### ویرایش کاربر

**Endpoint:** `PUT /api/auth/users/{userId}`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "firstName": "علی",
  "lastName": "احمدی",
  "faceCode": "FC001",
  "birthDate": "1370-05-15",
  "hireDate": "1400-01-01",
  "mobileNumber": "09123456789",
  "insuranceCode": "1234567890",
  "homePhoneNumber": "02112345678",
  "gender": 0,
  "educationDegreeId": "degree-uuid"
}
```

**پاسخ موفق (200):**

```json
{
  "success": true,
  "message": "اطلاعات کاربر با موفقیت به‌روزرسانی شد",
  "user": {
    "userId": "uuid-here",
    "personnelCode": "1001",
    "firstName": "علی",
    "lastName": "احمدی",
    "gender": 0,
    "educationDegreeId": "degree-uuid",
    ...
  }
}
```

---

## � مدیریت مدارک تحصیلی (Education Degrees)

### دریافت لیست مدارک تحصیلی

**Endpoint:** `GET /api/educationdegree`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
[
  {
    "id": "degree-uuid",
    "name": "کارشناسی",
    "isActive": true
  },
  {
    "id": "degree-uuid-2",
    "name": "کارشناسی ارشد",
    "isActive": true
  }
]
```

### دریافت جزئیات مدرک تحصیلی

**Endpoint:** `GET /api/educationdegree/{id}`  
**نیاز به احراز هویت:** ✅ بله

### ایجاد مدرک تحصیلی

**Endpoint:** `POST /api/educationdegree`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "name": "دکترا",
  "isActive": true
}
```

**پاسخ موفق (201):**

```json
{
  "id": "new-degree-uuid",
  "name": "دکترا",
  "isActive": true
}
```

### ویرایش مدرک تحصیلی

**Endpoint:** `PUT /api/educationdegree/{id}`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "name": "دکتری تخصصی",
  "isActive": true
}
```

### حذف مدرک تحصیلی

**Endpoint:** `DELETE /api/educationdegree/{id}`  
**نیاز به احراز هویت:** ✅ بله

**نکته:** اگر به کاربری اختصاص داده شده باشد، قابل حذف نیست.

---

## 💼 مدیریت سمت‌های شغلی (Job Positions)

### دریافت لیست سمت‌های شغلی

**Endpoint:** `GET /api/jobposition`  
**نیاز به احراز هویت:** ✅ بله  
**Query Parameters:**

- `search` (اختیاری): جستجو در عنوان یا کد
- `level` (اختیاری): فیلتر بر اساس سطح
- `isActive` (اختیاری): فیلتر بر اساس وضعیت فعال/غیرفعال
- `page` (پیش‌فرض: 1): شماره صفحه
- `pageSize` (پیش‌فرض: 20): تعداد در هر صفحه

**مثال:** `GET /api/jobposition?level=2&isActive=true`

**پاسخ موفق (200):**

```json
{
  "totalCount": 15,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1,
  "items": [
    {
      "id": "position-uuid",
      "title": "مدیر عامل",
      "code": "CEO",
      "description": "بالاترین سطح مدیریتی",
      "level": 1,
      "parentPositionId": null,
      "parentPositionTitle": null,
      "isActive": true
    },
    {
      "id": "position-uuid-2",
      "title": "مدیر فنی",
      "code": "CTO",
      "description": "مدیریت بخش فناوری",
      "level": 2,
      "parentPositionId": "position-uuid",
      "parentPositionTitle": "مدیر عامل",
      "isActive": true
    }
  ]
}
```

### دریافت جزئیات سمت شغلی

**Endpoint:** `GET /api/jobposition/{id}`  
**نیاز به احراز هویت:** ✅ بله

### ایجاد سمت شغلی

**Endpoint:** `POST /api/jobposition`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "title": "کارشناس ارشد",
  "code": "SE",
  "description": "کارشناس ارشد نرم‌افزار",
  "level": 3,
  "parentPositionId": "parent-position-uuid",
  "isActive": true
}
```

**نکته:** سطح باید بزرگتر از سطح سمت والد باشد.

### ویرایش سمت شغلی

**Endpoint:** `PUT /api/jobposition/{id}`  
**نیاز به احراز هویت:** ✅ بله

### حذف سمت شغلی

**Endpoint:** `DELETE /api/jobposition/{id}`  
**نیاز به احراز هویت:** ✅ بله

**نکته:** اگر کاربری این سمت را داشته باشد یا زیرمجموعه داشته باشد، قابل حذف نیست.

### دریافت درخت سلسله مراتب سمت‌ها

**Endpoint:** `GET /api/jobposition/hierarchy`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
[
  {
    "id": "position-uuid",
    "title": "مدیر عامل",
    "code": "CEO",
    "level": 1,
    "isActive": true,
    "employeeCount": 1,
    "subPositions": [
      {
        "id": "position-uuid-2",
        "title": "مدیر فنی",
        "code": "CTO",
        "level": 2,
        "isActive": true,
        "employeeCount": 3,
        "subPositions": [
          {
            "id": "position-uuid-3",
            "title": "سرپرست برنامه‌نویسی",
            "code": "DEV-LEAD",
            "level": 3,
            "isActive": true,
            "employeeCount": 5,
            "subPositions": []
          }
        ]
      }
    ]
  }
]
```

---

## 🏛️ سلسله مراتب سازمانی (Organizational Hierarchy)

### دریافت نمودار سازمانی (Org Chart)

**Endpoint:** `GET /api/organization/org-chart`  
**نیاز به احراز هویت:** ✅ بله  
**Query Parameters:**

- `rootUserId` (اختیاری): برای دریافت نمودار از یک کاربر خاص

**مثال:** `GET /api/organization/org-chart?rootUserId=user-uuid`

**پاسخ موفق (200):**

```json
[
  {
    "userId": "user-uuid",
    "personnelCode": "1001",
    "firstName": "محمد",
    "lastName": "احمدی",
    "jobPositionTitle": "مدیر عامل",
    "jobPositionLevel": 1,
    "unitName": "مدیریت",
    "managerId": null,
    "directReports": [
      {
        "userId": "user-uuid-2",
        "personnelCode": "1002",
        "firstName": "علی",
        "lastName": "رضایی",
        "jobPositionTitle": "مدیر فنی",
        "jobPositionLevel": 2,
        "managerId": "user-uuid",
        "directReports": []
      }
    ]
  }
]
```

### دریافت مدیر کاربر

**Endpoint:** `GET /api/organization/user/{userId}/manager`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
{
  "userId": "manager-uuid",
  "personnelCode": "1001",
  "firstName": "محمد",
  "lastName": "احمدی",
  "jobPositionTitle": "مدیر عامل",
  "unitName": "مدیریت"
}
```

### دریافت زنجیره مدیریت (Chain of Command)

**Endpoint:** `GET /api/organization/user/{userId}/chain-of-command`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
{
  "chain": [
    {
      "userId": "manager1-uuid",
      "personnelCode": "1002",
      "firstName": "علی",
      "lastName": "رضایی",
      "jobPositionTitle": "مدیر فنی",
      "unitName": "فناوری"
    },
    {
      "userId": "manager2-uuid",
      "personnelCode": "1001",
      "firstName": "محمد",
      "lastName": "احمدی",
      "jobPositionTitle": "مدیر عامل",
      "unitName": "مدیریت"
    }
  ],
  "levels": 2
}
```

### دریافت زیردستان مستقیم

**Endpoint:** `GET /api/organization/user/{userId}/direct-reports`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
[
  {
    "userId": "report-uuid",
    "personnelCode": "1003",
    "firstName": "حسین",
    "lastName": "کریمی",
    "jobPositionTitle": "کارشناس ارشد",
    "unitName": "فناوری",
    "directReportsCount": 2
  }
]
```

### دریافت تمام زیردستان

**Endpoint:** `GET /api/organization/user/{userId}/all-subordinates`  
**نیاز به احراز هویت:** ✅ بله

### تخصیص/تغییر مدیر کاربر

**Endpoint:** `PUT /api/organization/user/{userId}/assign-manager`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "userId": "user-uuid",
  "managerId": "manager-uuid"
}
```

**نکته:** برای حذف مدیر، `managerId` را `null` قرار دهید.

### دریافت آمار تیم

**Endpoint:** `GET /api/organization/user/{userId}/team-statistics`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
{
  "directReportsCount": 3,
  "totalSubordinatesCount": 15,
  "maxDepth": 3,
  "byJobPosition": {
    "کارشناس": 8,
    "کارشناس ارشد": 5,
    "سرپرست": 2
  },
  "byUnit": {
    "فناوری": 10,
    "منابع انسانی": 3,
    "مالی": 2
  }
}
```

---

## �🎭 مدیریت نقش‌ها (Roles)

### دریافت لیست نقش‌ها

**Endpoint:** `GET /api/role`  
**نیاز به احراز هویت:** ✅ بله  
**Query Parameters:**

- `companyId` (اختیاری): فیلتر نقش‌های یک شرکت + نقش‌های سراسری

**مثال:** `GET /api/role?companyId=5`

**پاسخ موفق (200):**

```json
[
  {
    "id": "role-uuid",
    "name": "مدیر",
    "isGlobalAccess": true,
    "companyId": null,
    "companyName": null
  },
  {
    "id": "role-uuid-2",
    "name": "کارشناس فروش",
    "isGlobalAccess": false,
    "companyId": 5,
    "companyName": "شرکت الف"
  }
]
```

### ایجاد نقش جدید

**Endpoint:** `POST /api/role`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "roleName": "مدیر فروش",
  "isGlobalAccess": false,
  "companyId": 5
}
```

**توضیح:**

- `companyId = null` → نقش سراسری (قابل استفاده در همه شرکت‌ها)
- `companyId = 5` → نقش مخصوص شرکت ۵

**پاسخ موفق (200):**

```json
{
  "success": true,
  "message": "نقش با موفقیت ایجاد شد",
  "roleId": "role-uuid",
  "companyId": 5
}
```

### ویرایش نقش

**Endpoint:** `PUT /api/role/{roleId}`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "roleName": "مدیر فروش ارشد",
  "isGlobalAccess": false,
  "companyId": 5
}
```

### حذف نقش

**Endpoint:** `DELETE /api/role/{roleId}`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
{
  "success": true,
  "message": "نقش با موفقیت حذف شد"
}
```

**پاسخ خطا (400):**

```json
{
  "success": false,
  "message": "این نقش به کاربران اختصاص داده شده و قابل حذف نیست"
}
```

### تخصیص نقش سراسری به کاربر

**Endpoint:** `POST /api/role/assign-role-to-user`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "personnelCode": "1001",
  "roleId": "role-uuid"
}
```

**نکته:** فقط نقش‌های سراسری (companyId = null) قابل تخصیص مستقیم هستند.

### تخصیص نقش در شرکت

**Endpoint:** `POST /api/role/assign-role-to-user-in-company`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "personnelCode": "1001",
  "roleId": "role-uuid",
  "companyId": 5
}
```

**توضیح:** نقش باید به همان شرکت تعلق داشته باشد یا سراسری باشد.

### تخصیص نقش در واحد

**Endpoint:** `POST /api/role/assign-role-to-user-in-unit`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "personnelCode": "1001",
  "roleId": "role-uuid",
  "unitId": 10
}
```

### حذف نقش سراسری از کاربر

**Endpoint:** `POST /api/role/remove-role-from-user`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "personnelCode": "1001",
  "roleId": "role-uuid"
}
```

### حذف نقش از کاربر در شرکت

**Endpoint:** `POST /api/role/remove-role-from-user-in-company`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "personnelCode": "1001",
  "roleId": "role-uuid",
  "companyId": 5
}
```

### حذف نقش از کاربر در واحد

**Endpoint:** `POST /api/role/remove-role-from-user-in-unit`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "personnelCode": "1001",
  "roleId": "role-uuid",
  "unitId": 10
}
```

### تخصیص منوها به نقش

**Endpoint:** `POST /api/role/assign-menus`  
**نیاز به احراز هویت:** ✅ بله

**توضیح:** این API تمام دسترسی‌های قبلی منوها را حذف کرده و دسترسی‌های جدید را اعمال می‌کند.

**Request Body:**

```json
{
  "roleId": "role-uuid",
  "menus": [
    {
      "menuId": "menu-uuid-1",
      "canView": true,
      "canCreate": false,
      "canEdit": false,
      "canDelete": false
    },
    {
      "menuId": "menu-uuid-2",
      "canView": true,
      "canCreate": true,
      "canEdit": true,
      "canDelete": false
    }
  ]
}
```

### ویرایش دسترسی یک منوی خاص

**Endpoint:** `PUT /api/role/update-menu-permission`  
**نیاز به احراز هویت:** ✅ بله

**توضیح:** برای ویرایش دسترسی‌های یک منوی خاص بدون تأثیر بر سایر منوها.

**Request Body:**

```json
{
  "roleId": "role-uuid",
  "menuId": "menu-uuid",
  "canView": true,
  "canCreate": true,
  "canEdit": true,
  "canDelete": false
}
```

**پاسخ موفق (200):**

```json
{
  "message": "دسترسی‌های منو با موفقیت به‌روزرسانی شد"
}
```

### تخصیص ویجت‌ها به نقش

**Endpoint:** `POST /api/role/assign-widgets`  
**نیاز به احراز هویت:** ✅ بله

**توضیح:** این API تمام دسترسی‌های قبلی ویجت‌ها را حذف کرده و دسترسی‌های جدید را اعمال می‌کند.

**Request Body:**

```json
{
  "roleId": "role-uuid",
  "widgets": [
    {
      "widgetId": "widget-uuid",
      "canView": true,
      "canConfigure": false
    }
  ]
}
```

### ویرایش دسترسی یک ویجت خاص

**Endpoint:** `PUT /api/role/update-widget-permission`  
**نیاز به احراز هویت:** ✅ بله

**توضیح:** برای ویرایش دسترسی‌های یک ویجت خاص بدون تأثیر بر سایر ویجت‌ها.

**Request Body:**

```json
{
  "roleId": "role-uuid",
  "widgetId": "widget-uuid",
  "canView": true,
  "canConfigure": true
}
```

**پاسخ موفق (200):**

```json
{
  "message": "دسترسی‌های ویجت با موفقیت به‌روزرسانی شد"
}
```

### دریافت منوهای نقش

**Endpoint:** `GET /api/role/{roleId}/menus`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
[
  {
    "menuId": 1,
    "name": "dashboard",
    "title": "داشبورد",
    "canView": true,
    "canCreate": false,
    "canEdit": false,
    "canDelete": false
  }
]
```

### دریافت ویجت‌های نقش

**Endpoint:** `GET /api/role/{roleId}/widgets`  
**نیاز به احراز هویت:** ✅ بله

### دریافت کاربران نقش

**Endpoint:** `GET /api/role/{roleId}/users`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
{
  "success": true,
  "roleName": "مدیر",
  "globalUsers": [
    {
      "userId": "uuid",
      "personnelCode": "1001",
      "firstName": "علی",
      "lastName": "احمدی",
      "assignmentType": "Global"
    }
  ],
  "companyUsers": [
    {
      "userId": "uuid",
      "personnelCode": "1002",
      "firstName": "رضا",
      "lastName": "رضایی",
      "assignmentType": "Company",
      "companyId": 5,
      "companyName": "شرکت الف",
      "assignedAt": "2024-01-15T10:30:00"
    }
  ],
  "unitUsers": [
    {
      "userId": "uuid",
      "personnelCode": "1003",
      "firstName": "محمد",
      "lastName": "محمدی",
      "assignmentType": "Unit",
      "unitId": 10,
      "unitName": "واحد فروش",
      "companyId": 5,
      "companyName": "شرکت الف",
      "assignedAt": "2024-02-20T14:00:00"
    }
  ]
}
```

---

## 🏢 مدیریت شرکت‌ها (Companies)

### دریافت لیست شرکت‌ها

**Endpoint:** `GET /api/company`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
[
  {
    "id": 1,
    "name": "شرکت الف",
    "code": "CO001",
    "address": "تهران، خیابان ولیعصر",
    "isActive": true
  }
]
```

### دریافت جزئیات شرکت

**Endpoint:** `GET /api/company/{id}`  
**نیاز به احراز هویت:** ✅ بله

### ایجاد شرکت

**Endpoint:** `POST /api/company`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "name": "شرکت الف",
  "code": "CO001",
  "address": "تهران، خیابان ولیعصر",
  "isActive": true
}
```

### ویرایش شرکت

**Endpoint:** `PUT /api/company/{id}`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:** (همه فیلدها اختیاری)

```json
{
  "name": "شرکت الف - ویرایش شده",
  "code": "CO001",
  "address": "تهران، میدان ونک",
  "isActive": true
}
```

### حذف شرکت

**Endpoint:** `DELETE /api/company/{id}`  
**نیاز به احراز هویت:** ✅ بله

**نکته:** اگر شرکت دارای واحد یا کاربر مرتبط باشد، قابل حذف نیست.

---

## 🏗️ مدیریت واحدها (Units)

### دریافت لیست واحدها

**Endpoint:** `GET /api/unit`  
**نیاز به احراز هویت:** ✅ بله  
**Query Parameters:**

- `companyId` (اختیاری): فیلتر واحدهای یک شرکت

**مثال:** `GET /api/unit?companyId=5`

**پاسخ موفق (200):** (فقط واحدهای ریشه با زیرواحدهای آن‌ها)

```json
[
  {
    "id": 1,
    "name": "مدیریت",
    "code": "U001",
    "companyId": 5,
    "companyName": "شرکت الف",
    "parentUnitId": null,
    "parentUnitName": null,
    "isActive": true,
    "subUnits": [
      {
        "id": 2,
        "name": "واحد فروش",
        "code": "U002",
        "companyId": 5,
        "companyName": "شرکت الف",
        "parentUnitId": 1,
        "parentUnitName": "مدیریت",
        "isActive": true,
        "subUnits": []
      }
    ]
  }
]
```

### دریافت جزئیات واحد

**Endpoint:** `GET /api/unit/{id}`  
**نیاز به احراز هویت:** ✅ بله

### ایجاد واحد

**Endpoint:** `POST /api/unit`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "name": "واحد فروش",
  "code": "U002",
  "companyId": 5,
  "parentUnitId": 1,
  "isActive": true
}
```

**نکته:** واحد والد باید در همان شرکت باشد.

### ویرایش واحد

**Endpoint:** `PUT /api/unit/{id}`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:** (همه فیلدها اختیاری)

```json
{
  "name": "واحد فروش ویژه",
  "code": "U002",
  "isActive": true
}
```

**نکته:** اگر CompanyId تغییر کند، ParentUnitId به null تنظیم می‌شود.

### حذف واحد

**Endpoint:** `DELETE /api/unit/{id}`  
**نیاز به احراز هویت:** ✅ بله

**نکته:** اگر واحد دارای زیرواحد یا کاربر مرتبط باشد، قابل حذف نیست.

---

## 📋 مدیریت منوها (Menus)

### دریافت لیست منوها

**Endpoint:** `GET /api/menu`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):** (فقط منوهای اصلی با زیرمنوها)

```json
[
  {
    "id": 1,
    "name": "dashboard",
    "title": "داشبورد",
    "icon": "dashboard",
    "url": "/dashboard",
    "displayOrder": 1,
    "parentMenuId": null,
    "isActive": true,
    "subMenus": [
      {
        "id": 2,
        "name": "reports",
        "title": "گزارشات",
        "icon": "report",
        "url": "/dashboard/reports",
        "displayOrder": 1,
        "parentMenuId": 1,
        "isActive": true,
        "subMenus": []
      }
    ]
  }
]
```

### دریافت جزئیات منو

**Endpoint:** `GET /api/menu/{id}`  
**نیاز به احراز هویت:** ✅ بله

### ایجاد منو

**Endpoint:** `POST /api/menu`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "name": "dashboard",
  "title": "داشبورد",
  "icon": "dashboard",
  "url": "/dashboard",
  "displayOrder": 1,
  "parentMenuId": null,
  "isActive": true
}
```

### ویرایش منو

**Endpoint:** `PUT /api/menu/{id}`  
**نیاز به احراز هویت:** ✅ بله

### حذف منو

**Endpoint:** `DELETE /api/menu/{id}`  
**نیاز به احراز هویت:** ✅ بله

**نکته:** اگر منو دارای زیرمنو یا دسترسی نقش باشد، قابل حذف نیست.

---

## 🧩 مدیریت ویجت‌ها (Widgets)

### دریافت لیست ویجت‌ها

**Endpoint:** `GET /api/widget`  
**نیاز به احراز هویت:** ✅ بله

**پاسخ موفق (200):**

```json
[
  {
    "id": 1,
    "name": "sales-chart",
    "title": "نمودار فروش",
    "description": "نمودار فروش ماهانه",
    "icon": "chart",
    "widgetType": "chart",
    "configuration": "{}",
    "displayOrder": 1,
    "isActive": true
  }
]
```

### دریافت جزئیات ویجت

**Endpoint:** `GET /api/widget/{id}`  
**نیاز به احراز هویت:** ✅ بله

### ایجاد ویجت

**Endpoint:** `POST /api/widget`  
**نیاز به احراز هویت:** ✅ بله

**Request Body:**

```json
{
  "name": "sales-chart",
  "title": "نمودار فروش",
  "description": "نمودار فروش ماهانه",
  "icon": "chart",
  "widgetType": "chart",
  "configuration": "{}",
  "displayOrder": 1,
  "isActive": true
}
```

### ویرایش ویجت

**Endpoint:** `PUT /api/widget/{id}`  
**نیاز به احراز هویت:** ✅ بله

### حذف ویجت

**Endpoint:** `DELETE /api/widget/{id}`  
**نیاز به احراز هویت:** ✅ بله

**نکته:** اگر ویجت به نقش‌ها اختصاص داده شده باشد، قابل حذف نیست.

---

## 🛡️ نکات امنیتی و فنی

- **توکن:** همه مسیرها (به جز ثبت‌نام، لاگین و فراموشی رمز عبور) نیاز به JWT دارند؛ هدر: `Authorization: Bearer <token>`
- **انقضا:** توکن‌ها به‌صورت پیش‌فرض پس از 60 دقیقه منقضی می‌شوند (قابل تنظیم در appsettings.json)
- **CORS:** دسترسی برای `http://localhost:5173` و `http://127.0.0.1:5173` فعال است
- **Base URL:** محیط توسعه `http://localhost:5257`
- **ساختار پاسخ:** تمام پاسخ‌ها دارای فیلدهای `success`, `message` و داده‌های ساختارمند هستند
- **خطاهای رایج:**
  - 400: داده‌های ورودی نامعتبر یا تضاد (مثلاً حذف شرکت دارای وابستگی)
  - 401: توکن نامعتبر/منقضی شده یا ارسال نشده
  - 403: دسترسی ناکافی برای عملیات موردنظر
  - 404: منبع یافت نشد
  - 500: خطای داخلی سرور

---

## 🧭 راهنمای یکپارچه‌سازی فرانت‌اند

### شروع کار
- **ورود/ثبت‌نام:** پس از موفقیت، توکن JWT را ذخیره و در همه درخواست‌های محافظت‌شده ارسال کنید
- **بارگذاری دسترسی‌ها:** در شروع اپلیکیشن، از `GET /api/auth/user-permissions` برای دریافت اطلاعات کامل کاربر استفاده کنید

### اطلاعات کاربر
- **جنسیت و مدرک تحصیلی:** از فیلدهای `gender` و `educationDegree` استفاده کنید
- **سمت شغلی:** `jobPosition` شامل عنوان، کد، سطح و سلسله مراتب است
- **شیفت فعلی:** `currentShift` نمایش دهنده شیفت فعال کاربر در زمان حاضر است
- **واحد اصلی:** `unit` اولین واحد از لیست `units` است

### رندر منوها و ویجت‌ها
- **منوها:** فقط منوهایی را نمایش دهید که `canView = true` هستند
- **دسترسی‌ها:** از `canCreate/canEdit/canDelete` برای نمایش دکمه‌های مربوطه استفاده کنید
- **ویجت‌ها:** بر اساس `canView` و `canConfigure` رفتار UI را تنظیم کنید

### سلسله مراتب سازمانی
- **نمودار سازمانی:** از `GET /api/organization/org-chart` برای نمایش چارت سازمانی استفاده کنید
- **زنجیره مدیریت:** برای نمایش مسیر تایید درخواست‌ها از `chain-of-command` استفاده کنید
- **مدیریت تیم:** از `direct-reports` و `team-statistics` برای نمایش اطلاعات تیم استفاده کنید

### فیلترها و جستجو
- **فیلتر شرکت/واحد:** رابط کاربری را با انتخاب شرکت/واحد به‌روزرسانی کنید
- **صفحه‌بندی:** از `page` و `pageSize` استفاده کرده و `totalPages` را لحاظ کنید
- **جستجو:** از پارامتر `search` برای جستجوی عمومی استفاده کنید

### مدیریت شیفت‌ها
- **برنامه روزانه:** از `GET /api/shift/schedule` برای نمایش برنامه کاری کاربر استفاده کنید
- **کاربران حاضر:** از `GET /api/shift/on-duty` برای نمایش کاربران حاضر در شیفت استفاده کنید
- **شیفت فعلی:** در `user-permissions` فیلد `currentShift` را برای نمایش شیفت جاری استفاده کنید

---

## 📊 خلاصه Endpoints

### احراز هویت و کاربران
- `POST /api/auth/register` - ثبت‌نام
- `POST /api/auth/login` - ورود
- `POST /api/auth/forgot-password` - فراموشی رمز
- `POST /api/auth/reset-password` - تنظیم مجدد رمز
- `GET /api/auth/user-permissions` - دسترسی‌های کاربر
- `GET /api/auth/users` - لیست کاربران
- `PUT /api/auth/users/{id}` - ویرایش کاربر

### مدارک تحصیلی
- `GET /api/educationdegree` - لیست
- `GET /api/educationdegree/{id}` - جزئیات
- `POST /api/educationdegree` - ایجاد
- `PUT /api/educationdegree/{id}` - ویرایش
- `DELETE /api/educationdegree/{id}` - حذف

### سمت‌های شغلی
- `GET /api/jobposition` - لیست
- `GET /api/jobposition/{id}` - جزئیات
- `POST /api/jobposition` - ایجاد
- `PUT /api/jobposition/{id}` - ویرایش
- `DELETE /api/jobposition/{id}` - حذف
- `GET /api/jobposition/hierarchy` - درخت سلسله مراتب

### سلسله مراتب سازمانی
- `GET /api/organization/org-chart` - نمودار سازمانی
- `GET /api/organization/user/{id}/manager` - مدیر کاربر
- `GET /api/organization/user/{id}/chain-of-command` - زنجیره مدیریت
- `GET /api/organization/user/{id}/direct-reports` - زیردستان مستقیم
- `GET /api/organization/user/{id}/all-subordinates` - تمام زیردستان
- `PUT /api/organization/user/{id}/assign-manager` - تخصیص مدیر
- `GET /api/organization/user/{id}/team-statistics` - آمار تیم

### نقش‌ها
- `GET /api/role` - لیست نقش‌ها
- `POST /api/role` - ایجاد نقش
- `PUT /api/role/{id}` - ویرایش
- `DELETE /api/role/{id}` - حذف
- `POST /api/role/assign-*` - تخصیص نقش به کاربر
- `POST /api/role/remove-*` - حذف نقش از کاربر
- `POST /api/role/assign-menus` - تخصیص منوها (جایگزینی کامل)
- `PUT /api/role/update-menu-permission` - ویرایش دسترسی یک منو
- `POST /api/role/assign-widgets` - تخصیص ویجت‌ها (جایگزینی کامل)
- `PUT /api/role/update-widget-permission` - ویرایش دسترسی یک ویجت
- `GET /api/role/{id}/users` - کاربران نقش

### شرکت‌ها
- `GET /api/company` - لیست
- `GET /api/company/{id}` - جزئیات
- `POST /api/company` - ایجاد
- `PUT /api/company/{id}` - ویرایش
- `DELETE /api/company/{id}` - حذف

### واحدها
- `GET /api/unit` - لیست
- `GET /api/unit/{id}` - جزئیات
- `POST /api/unit` - ایجاد
- `PUT /api/unit/{id}` - ویرایش
- `DELETE /api/unit/{id}` - حذف

### منوها
- `GET /api/menu` - لیست
- `GET /api/menu/{id}` - جزئیات
- `POST /api/menu` - ایجاد
- `PUT /api/menu/{id}` - ویرایش
- `DELETE /api/menu/{id}` - حذف

### ویجت‌ها
- `GET /api/widget` - لیست
- `GET /api/widget/{id}` - جزئیات
- `POST /api/widget` - ایجاد
- `PUT /api/widget/{id}` - ویرایش
- `DELETE /api/widget/{id}` - حذف

### شیفت‌ها
- `GET /api/shift/definitions` - لیست شیفت‌ها
- `GET /api/shift/definitions/{id}` - جزئیات
- `POST /api/shift/definitions` - ایجاد
- `PUT /api/shift/definitions/{id}` - ویرایش
- `DELETE /api/shift/definitions/{id}` - حذف
- `POST /api/shift/assignments` - تخصیص شیفت
- `GET /api/shift/assignments` - لیست تخصیص‌ها
- `DELETE /api/shift/assignments/{id}` - حذف تخصیص
- `GET /api/shift/schedule` - برنامه روزانه
- `GET /api/shift/on-duty` - کاربران حاضر

---

## 📝 یادداشت‌های نسخه 2.0

### ویژگی‌های جدید:
✅ **مدارک تحصیلی**: مدیریت کامل مدارک تحصیلی کاربران  
✅ **سمت‌های شغلی**: سیستم سلسله مراتب سمت‌ها با سطوح مختلف  
✅ **سلسله مراتب سازمانی**: ساختار مدیر-زیردست و نمودار سازمانی  
✅ **شیفت در user-permissions**: نمایش شیفت فعال در اطلاعات کاربر  
✅ **JobPosition در user-permissions**: نمایش سمت شغلی در اطلاعات کاربر  
✅ **Gender و EducationDegree**: اضافه شدن به لیست و ویرایش کاربران  
✅ **ویرایش دسترسی‌های منو/ویجت**: امکان ویرایش دسترسی‌های خاص بدون جایگزینی کامل  

### تغییرات مهم:
- **user-permissions** حالا شامل `gender`, `educationDegree`, `jobPosition`, `currentShift` و `unit` است
- **لیست کاربران** حالا فیلترهای `gender` و `educationDegreeId` را پشتیبانی می‌کند
- **ویرایش کاربر** حالا `gender` و `educationDegreeId` را پشتیبانی می‌کند
- **ویرایش دسترسی‌ها**: دو endpoint جدید برای ویرایش دسترسی منو و ویجت اضافه شد

### نحوه استفاده از APIهای دسترسی:

**سناریو 1: تخصیص اولیه دسترسی‌ها**
```
POST /api/role/assign-menus - تمام منوهای نقش را جایگزین می‌کند
POST /api/role/assign-widgets - تمام ویجت‌های نقش را جایگزین می‌کند
```

**سناریو 2: ویرایش دسترسی یک منو/ویجت خاص**
```
PUT /api/role/update-menu-permission - فقط یک منو را ویرایش می‌کند
PUT /api/role/update-widget-permission - فقط یک ویجت را ویرایش می‌کند
```

**مثال کاربردی:**
- کاربر به نقش "مدیر" 10 منو اختصاص داده است
- می‌خواهد فقط دسترسی "canEdit" منوی "داشبورد" را تغییر دهد
- به جای ارسال مجدد 10 منو با `assign-menus`، فقط از `update-menu-permission` استفاده می‌کند

---

## 🕒 مدیریت شیفت‌های کاری (Shifts)

### تعریف شیفت جدید

**Endpoint:** `POST /api/shift/definitions`  
**نیاز به احراز هویت:** ✅ بله

**Request Body (شیفت عادی):**

```json
{
  "name": "اداری",
  "description": "ساعت اداری",
  "type": 0,
  "fixedStartTime": "08:00:00",
  "fixedEndTime": "16:00:00",
  "fixedBreakMinutes": 60,
  "workingDaysMask": 0
}
```

**Request Body (شیفت چرخشی 2-2-2 A/B/C + Off):**

```json
{
  "name": "سه شیفت چرخشی",
  "description": "دو روز روزکار، دو روز شبکار، دو روز استراحت",
  "type": 1,
  "segments": [
    {
      "label": "A",
      "durationDays": 2,
      "isOff": false,
      "startTime": "08:00:00",
      "endTime": "16:00:00"
    },
    {
      "label": "B",
      "durationDays": 2,
      "isOff": false,
      "startTime": "16:00:00",
      "endTime": "00:00:00"
    },
    {
      "label": "C",
      "durationDays": 2,
      "isOff": false,
      "startTime": "00:00:00",
      "endTime": "08:00:00"
    },
    { "label": "Off", "durationDays": 2, "isOff": true }
  ]
}
```

### فهرست/جزئیات شیفت‌ها

- `GET /api/shift/definitions`
- `GET /api/shift/definitions/{id}`

### ویرایش/حذف شیفت

- `PUT /api/shift/definitions/{id}`
- `DELETE /api/shift/definitions/{id}` (اگر به کاربر انتساب شده باشد، خطا برمی‌گرداند)

### انتساب شیفت به کاربر

**Endpoint:** `POST /api/shift/assignments`  
**Request Body:**

```json
{
  "personnelCode": "1001",
  "shiftDefinitionId": 3,
  "startDate": "2025-01-01",
  "endDate": "2025-03-31",
  "rotationStartIndex": 0
}
```

### فهرست انتساب‌های کاربر

- `GET /api/shift/assignments?personnelCode=1001`

### حذف انتساب

- `DELETE /api/shift/assignments/{id}`

### دریافت برنامه روزانه کاربر (Schedule)

**Endpoint:** `GET /api/shift/schedule?personnelCode=1001&from=2025-01-01&to=2025-01-10`  
**پاسخ نمونه:**

```json
{
  "success": true,
  "days": [
    {
      "date": "2025-01-01",
      "label": "A",
      "isOff": false,
      "startTime": "08:00:00",
      "endTime": "16:00:00"
    },
    {
      "date": "2025-01-02",
      "label": "A",
      "isOff": false,
      "startTime": "08:00:00",
      "endTime": "16:00:00"
    },
    {
      "date": "2025-01-03",
      "label": "B",
      "isOff": false,
      "startTime": "16:00:00",
      "endTime": "00:00:00"
    }
  ]
}
```

### کاربران حاضر در شیفت (On-Duty)

**Endpoint:** `GET /api/shift/on-duty?dt=2025-01-05T09:30:00`  
اگر `dt` ارسال نشود، زمان فعلی سرور استفاده می‌شود.

**پاسخ نمونه:**

```json
{
  "success": true,
  "count": 2,
  "items": [
    {
      "userId": "uuid-1",
      "personnelCode": "1001",
      "firstName": "علی",
      "lastName": "احمدی",
      "shiftName": "سه‌شیفت A/B/C (2-2-2)",
      "label": "A",
      "startTime": "06:00:00",
      "endTime": "18:00:00"
    }
  ]
}
```
