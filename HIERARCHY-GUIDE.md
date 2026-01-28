# راهنمای استفاده از سیستم سلسله مراتب

## سلسله مراتب شغلی (Job Position)

### مدیریت سمت‌های شغلی

#### دریافت لیست سمت‌ها
```http
GET /api/JobPosition?page=1&pageSize=20
GET /api/JobPosition?search=مدیر
GET /api/JobPosition?level=2
GET /api/JobPosition?isActive=true
```

#### دریافت یک سمت شغلی
```http
GET /api/JobPosition/{id}
```

#### ایجاد سمت شغلی جدید
```http
POST /api/JobPosition
Content-Type: application/json

{
  "title": "مدیر عامل",
  "code": "CEO",
  "description": "بالاترین سطح مدیریتی",
  "level": 1,
  "parentPositionId": null,
  "isActive": true
}
```

#### ویرایش سمت شغلی
```http
PUT /api/JobPosition/{id}
Content-Type: application/json

{
  "title": "مدیر فنی",
  "code": "CTO",
  "description": "مدیریت بخش فناوری",
  "level": 2,
  "parentPositionId": "{ceo-id}",
  "isActive": true
}
```

#### حذف سمت شغلی
```http
DELETE /api/JobPosition/{id}
```

#### دریافت درخت سلسله مراتب سمت‌ها
```http
GET /api/JobPosition/hierarchy
```

**پاسخ نمونه:**
```json
[
  {
    "id": "guid",
    "title": "مدیر عامل",
    "code": "CEO",
    "level": 1,
    "isActive": true,
    "employeeCount": 1,
    "subPositions": [
      {
        "id": "guid",
        "title": "مدیر فنی",
        "code": "CTO",
        "level": 2,
        "isActive": true,
        "employeeCount": 3,
        "subPositions": [...]
      }
    ]
  }
]
```

---

## سلسله مراتب سازمانی (Organizational Hierarchy)

### نمودار سازمانی (Org Chart)

#### دریافت کل نمودار سازمانی
```http
GET /api/Organization/org-chart
```

#### دریافت نمودار از یک کاربر خاص
```http
GET /api/Organization/org-chart?rootUserId={userId}
```

**پاسخ نمونه:**
```json
[
  {
    "userId": "string",
    "personnelCode": "1001",
    "firstName": "محمد",
    "lastName": "احمدی",
    "jobPositionTitle": "مدیر عامل",
    "jobPositionLevel": 1,
    "unitName": "مدیریت",
    "managerId": null,
    "directReports": [
      {
        "userId": "string",
        "personnelCode": "1002",
        "firstName": "علی",
        "lastName": "رضایی",
        "jobPositionTitle": "مدیر فنی",
        "jobPositionLevel": 2,
        "managerId": "string",
        "directReports": [...]
      }
    ]
  }
]
```

### مدیر کاربر

#### دریافت مدیر مستقیم
```http
GET /api/Organization/user/{userId}/manager
```

**پاسخ نمونه:**
```json
{
  "userId": "string",
  "personnelCode": "1001",
  "firstName": "محمد",
  "lastName": "احمدی",
  "jobPositionTitle": "مدیر عامل",
  "unitName": "مدیریت"
}
```

#### دریافت زنجیره مدیریت (تا بالاترین سطح)
```http
GET /api/Organization/user/{userId}/chain-of-command
```

**پاسخ نمونه:**
```json
{
  "chain": [
    {
      "userId": "string",
      "personnelCode": "1002",
      "firstName": "علی",
      "lastName": "رضایی",
      "jobPositionTitle": "مدیر فنی",
      "unitName": "فناوری"
    },
    {
      "userId": "string",
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

### زیردستان

#### دریافت زیردستان مستقیم
```http
GET /api/Organization/user/{userId}/direct-reports
```

#### دریافت تمام زیردستان (در همه سطوح)
```http
GET /api/Organization/user/{userId}/all-subordinates
```

**پاسخ نمونه:**
```json
[
  {
    "userId": "string",
    "personnelCode": "1003",
    "firstName": "حسین",
    "lastName": "کریمی",
    "jobPositionTitle": "کارشناس ارشد",
    "unitName": "فناوری",
    "directReportsCount": 2
  }
]
```

### تخصیص مدیر

#### تعیین/تغییر مدیر کاربر
```http
PUT /api/Organization/user/{userId}/assign-manager
Content-Type: application/json

{
  "userId": "{user-id}",
  "managerId": "{manager-id}"
}
```

برای حذف مدیر، `managerId` را `null` قرار دهید:
```json
{
  "userId": "{user-id}",
  "managerId": null
}
```

### آمار تیم

#### دریافت آمار تیم یک مدیر
```http
GET /api/Organization/user/{userId}/team-statistics
```

**پاسخ نمونه:**
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

## نمونه سناریوهای استفاده

### 1. ایجاد ساختار سمت‌های شغلی

```javascript
// مدیر عامل
const ceo = await createJobPosition({
  title: "مدیر عامل",
  code: "CEO",
  level: 1
});

// مدیر فنی
const cto = await createJobPosition({
  title: "مدیر فنی",
  code: "CTO",
  level: 2,
  parentPositionId: ceo.id
});

// سرپرست برنامه‌نویسی
const devLead = await createJobPosition({
  title: "سرپرست برنامه‌نویسی",
  code: "DEV-LEAD",
  level: 3,
  parentPositionId: cto.id
});
```

### 2. ساخت ساختار سازمانی

```javascript
// ویرایش کاربران و تعیین سمت و مدیر
await updateUser(userId1, {
  jobPositionId: ceo.id,
  managerId: null  // مدیر عامل، مدیر ندارد
});

await updateUser(userId2, {
  jobPositionId: cto.id,
  managerId: userId1  // مدیر فنی، زیر مدیر عامل
});

await updateUser(userId3, {
  jobPositionId: devLead.id,
  managerId: userId2  // سرپرست، زیر مدیر فنی
});
```

### 3. نمایش چارت سازمانی در فرانت

```javascript
// دریافت نمودار کامل
const orgChart = await fetch('/api/Organization/org-chart');

// نمایش به صورت درختی
function renderOrgChart(node) {
  return `
    <div class="org-node">
      <div class="employee">
        ${node.firstName} ${node.lastName}
        <br>
        <small>${node.jobPositionTitle}</small>
      </div>
      <div class="subordinates">
        ${node.directReports.map(renderOrgChart).join('')}
      </div>
    </div>
  `;
}
```

### 4. سیستم تایید درخواست‌ها

```javascript
// دریافت زنجیره مدیریت برای تایید درخواست
const chain = await fetch(`/api/Organization/user/${userId}/chain-of-command`);

// ارسال درخواست به مدیر مستقیم
const manager = chain.chain[0];
await sendApprovalRequest(manager.userId, requestData);
```

---

## نکات مهم

### محدودیت‌ها و اعتبارسنجی‌ها

1. **سمت شغلی:**
   - عنوان سمت باید یکتا باشد
   - کد سمت (در صورت وجود) باید یکتا باشد
   - سطح سمت باید بزرگتر از سطح سمت والد باشد
   - نمی‌توان سمت را زیرمجموعه خودش قرار داد
   - سمت‌هایی که کاربر یا زیرمجموعه دارند قابل حذف نیستند

2. **سلسله مراتب سازمانی:**
   - کاربر نمی‌تواند مدیر خودش باشد
   - نمی‌توان کاربر را مدیر زیردست خودش قرار داد (جلوگیری از حلقه)
   - سیستم تا 20 سطح عمق را پشتیبانی می‌کند

### بهینه‌سازی عملکرد

- برای نمودار سازمانی بزرگ، از فیلتر `rootUserId` استفاده کنید
- از pagination در لیست‌ها استفاده کنید
- برای نمایش سریع، فقط زیردستان مستقیم را بارگذاری کنید

### امنیت

- تمام APIها نیاز به Authentication دارند
- می‌توانید Authorization بر اساس نقش اضافه کنید
- تنها مدیران منابع انسانی باید بتوانند سلسله مراتب را تغییر دهند
