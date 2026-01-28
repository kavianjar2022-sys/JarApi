# 🚀 راهنمای سریع برای تیم Frontend

## شروع سریع

### 1. اجرای API
```bash
cd JarApi
dotnet run
```

API روی `http://localhost:5257` اجرا می‌شود.

### 2. تست API

دو روش برای تست:

#### روش 1: Swagger UI (توصیه می‌شود)
مرورگر خود را باز کنید و به آدرس زیر بروید:
```
http://localhost:5257
```

#### روش 2: فایل HTML تست
فایل `test-api.html` را در مرورگر باز کنید:
```bash
# در ویندوز
start test-api.html

# یا مستقیماً فایل را در مرورگر باز کنید
```

---

## 📋 نکات مهم برای Frontend

### 1. Base URL
```javascript
const API_BASE_URL = 'http://localhost:5257';
```

### 2. Authentication
تمام endpoint‌ها (به جز login و register) نیاز به توکن دارند:

```javascript
headers: {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
}
```

### 3. نحوه ذخیره Token
```javascript
// بعد از login/register موفق
localStorage.setItem('authToken', response.token);
localStorage.setItem('user', JSON.stringify(response.userInfo));
```

### 4. نمونه کد Login
```javascript
async function login(personnelCode, password) {
  try {
    const response = await fetch('http://localhost:5257/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ personnelCode, password })
    });

    const data = await response.json();

    if (data.success) {
      localStorage.setItem('authToken', data.token);
      localStorage.setItem('user', JSON.stringify(data.userInfo));
      return data;
    } else {
      throw new Error(data.message);
    }
  } catch (error) {
    console.error('Login failed:', error);
    throw error;
  }
}
```

### 5. نمونه کد برای API با Authentication
```javascript
async function getUsers(page = 1, pageSize = 20) {
  const token = localStorage.getItem('authToken');
  
  if (!token) {
    throw new Error('لطفاً ابتدا وارد شوید');
  }

  try {
    const response = await fetch(
      `http://localhost:5257/api/auth/users?page=${page}&pageSize=${pageSize}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );

    const data = await response.json();

    if (response.ok) {
      return data;
    } else {
      throw new Error(data.message);
    }
  } catch (error) {
    console.error('Failed to fetch users:', error);
    throw error;
  }
}
```

---

## 🎯 Endpoint های اصلی

### احراز هویت
- `POST /api/auth/register` - ثبت‌نام
- `POST /api/auth/login` - ورود
- `GET /api/auth/user-permissions` - دریافت دسترسی‌های کاربر (نیاز به token)

### مدیریت کاربران
- `GET /api/auth/users` - لیست کاربران (pagination + filters)
- `PUT /api/auth/users/{userId}` - ویرایش کاربر

### مدارک تحصیلی
- `GET /api/educationdegree` - لیست مدارک
- `POST /api/educationdegree` - ایجاد مدرک
- `PUT /api/educationdegree/{id}` - ویرایش
- `DELETE /api/educationdegree/{id}` - حذف

### شرکت‌ها
- `GET /api/company` - لیست شرکت‌ها
- `POST /api/company` - ایجاد شرکت
- `PUT /api/company/{id}` - ویرایش
- `DELETE /api/company/{id}` - حذف

### واحدها
- `GET /api/unit` - لیست واحدها
- `POST /api/unit` - ایجاد واحد
- `PUT /api/unit/{id}` - ویرایش
- `DELETE /api/unit/{id}` - حذف

مستندات کامل: `API-Documentation-FA.md`

---

## 📦 ساختار Response

### موفق
```json
{
  "success": true,
  "message": "عملیات موفق",
  "data": { ... }
}
```

### خطا
```json
{
  "success": false,
  "message": "پیام خطا",
  "errors": {
    "field": ["error message"]
  }
}
```

---

## 🔧 مدیریت خطاها

```javascript
async function handleApiCall(apiFunction) {
  try {
    const result = await apiFunction();
    
    if (result.success) {
      return result.data;
    } else {
      // نمایش پیام خطا به کاربر
      showError(result.message);
      return null;
    }
  } catch (error) {
    if (error.status === 401) {
      // Unauthorized - redirect to login
      redirectToLogin();
    } else if (error.status === 403) {
      // Forbidden - show access denied
      showError('شما دسترسی لازم را ندارید');
    } else {
      // سایر خطاها
      showError('خطایی رخ داد. لطفاً دوباره تلاش کنید');
    }
    return null;
  }
}
```

---

## 🌐 CORS

CORS برای آدرس‌های زیر فعال است:
- `http://localhost:5173` (Vite default)
- `http://127.0.0.1:5173`
- `http://localhost:3000` (React/Next default)
- `http://127.0.0.1:3000`

---

## 📱 نمونه استفاده در React

```jsx
import { useState, useEffect } from 'react';

function UsersList() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchUsers();
  }, []);

  async function fetchUsers() {
    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('http://localhost:5257/api/auth/users?page=1&pageSize=20', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      const data = await response.json();
      
      if (response.ok) {
        setUsers(data.users);
      } else {
        console.error('Error:', data.message);
      }
    } catch (error) {
      console.error('Failed to fetch:', error);
    } finally {
      setLoading(false);
    }
  }

  if (loading) return <div>در حال بارگذاری...</div>;

  return (
    <div>
      {users.map(user => (
        <div key={user.userId}>
          {user.firstName} {user.lastName} ({user.personnelCode})
        </div>
      ))}
    </div>
  );
}
```

---

## ⚡ Performance Tips

1. **Cache کردن لیست‌های ثابت** (مثل مدارک تحصیلی)
2. **استفاده از Pagination** برای لیست‌های بزرگ
3. **Debounce** برای جستجو
4. **Token Refresh** را پیاده‌سازی کنید (token بعد از 60 دقیقه expire می‌شود)

---

## 🐛 Debug Tips

### Check API Status
```javascript
fetch('http://localhost:5257/health')
  .then(res => res.json())
  .then(data => console.log('API is running'))
  .catch(() => console.error('API is not running'));
```

### Check Token
```javascript
const token = localStorage.getItem('authToken');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log('Token expires at:', new Date(payload.exp * 1000));
}
```

---

## 📞 پشتیبانی

در صورت بروز مشکل:
1. فایل `test-api.html` را باز کنید و تست‌های اولیه را اجرا کنید
2. Swagger UI را بررسی کنید: `http://localhost:5257`
3. Console مرورگر را برای error های CORS بررسی کنید
4. مطمئن شوید API در حال اجراست

---

## ✅ Checklist قبل از شروع

- [ ] API در حال اجراست (`dotnet run`)
- [ ] Swagger UI باز می‌شود (`http://localhost:5257`)
- [ ] توانستید با Swagger یک user ثبت‌نام کنید
- [ ] توانستید با Swagger login کنید و token دریافت کنید
- [ ] فایل `test-api.html` کار می‌کند
- [ ] CORS error نمی‌گیرید

---

**موفق باشید! 🚀**
