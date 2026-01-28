// =====================================================
// JarApi - Frontend Test Script
// این فایل شامل توابع آماده برای تست API است
// =====================================================

const API_BASE_URL = 'http://172.16.68.238:5257';
let authToken = null;
let currentUser = null;

// =====================================================
// Helper Functions
// =====================================================

function setToken(token) {
  authToken = token;
  localStorage.setItem('authToken', token);
  console.log('✅ Token saved:', token.substring(0, 20) + '...');
}

function getToken() {
  if (!authToken) {
    authToken = localStorage.getItem('authToken');
  }
  return authToken;
}

function clearAuth() {
  authToken = null;
  currentUser = null;
  localStorage.removeItem('authToken');
  localStorage.removeItem('currentUser');
  console.log('✅ Auth cleared');
}

async function apiRequest(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers
  };

  if (options.auth !== false && getToken()) {
    headers['Authorization'] = `Bearer ${getToken()}`;
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers
    });

    const data = await response.json();
    
    if (!response.ok) {
      console.error('❌ Error:', data);
      return { success: false, error: data, status: response.status };
    }

    console.log('✅ Success:', data);
    return { success: true, data, status: response.status };
  } catch (error) {
    console.error('❌ Network Error:', error);
    return { success: false, error: error.message };
  }
}

// =====================================================
// Authentication APIs
// =====================================================

async function register(userData) {
  console.log('📝 Registering user:', userData.personnelCode);
  const result = await apiRequest('/api/auth/register', {
    method: 'POST',
    auth: false,
    body: JSON.stringify(userData)
  });

  if (result.success && result.data.token) {
    setToken(result.data.token);
    currentUser = result.data.userInfo;
    localStorage.setItem('currentUser', JSON.stringify(currentUser));
  }

  return result;
}

async function login(personnelCode, password) {
  console.log('🔐 Logging in:', personnelCode);
  const result = await apiRequest('/api/auth/login', {
    method: 'POST',
    auth: false,
    body: JSON.stringify({ personnelCode, password })
  });

  if (result.success && result.data.token) {
    setToken(result.data.token);
    currentUser = result.data.userInfo;
    localStorage.setItem('currentUser', JSON.stringify(currentUser));
  }

  return result;
}

async function getUserPermissions() {
  console.log('👤 Getting user permissions...');
  return await apiRequest('/api/auth/user-permissions');
}

async function forgotPassword(personnelCode, insuranceCode, mobileNumber) {
  console.log('🔑 Forgot password for:', personnelCode);
  return await apiRequest('/api/auth/forgot-password', {
    method: 'POST',
    auth: false,
    body: JSON.stringify({ personnelCode, insuranceCode, mobileNumber })
  });
}

async function resetPassword(userId, newPassword) {
  console.log('🔄 Resetting password for userId:', userId);
  return await apiRequest('/api/auth/reset-password', {
    method: 'POST',
    auth: false,
    body: JSON.stringify({ userId, newPassword })
  });
}

// =====================================================
// User Management APIs
// =====================================================

async function getUsers(filters = {}) {
  console.log('📋 Getting users with filters:', filters);
  const params = new URLSearchParams(filters);
  return await apiRequest(`/api/auth/users?${params}`);
}

async function updateUser(userId, userData) {
  console.log('✏️ Updating user:', userId);
  return await apiRequest(`/api/auth/users/${userId}`, {
    method: 'PUT',
    body: JSON.stringify(userData)
  });
}

// =====================================================
// Education Degree APIs
// =====================================================

async function getEducationDegrees(filters = {}) {
  console.log('🎓 Getting education degrees...');
  const params = new URLSearchParams(filters);
  return await apiRequest(`/api/educationdegree?${params}`);
}

async function getEducationDegree(id) {
  console.log('🎓 Getting education degree:', id);
  return await apiRequest(`/api/educationdegree/${id}`);
}

async function createEducationDegree(data) {
  console.log('➕ Creating education degree:', data.name);
  return await apiRequest('/api/educationdegree', {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

async function updateEducationDegree(id, data) {
  console.log('✏️ Updating education degree:', id);
  return await apiRequest(`/api/educationdegree/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

async function deleteEducationDegree(id) {
  console.log('🗑️ Deleting education degree:', id);
  return await apiRequest(`/api/educationdegree/${id}`, {
    method: 'DELETE'
  });
}

// =====================================================
// Company APIs
// =====================================================

async function getCompanies(filters = {}) {
  console.log('🏢 Getting companies...');
  const params = new URLSearchParams(filters);
  return await apiRequest(`/api/company?${params}`);
}

async function getCompany(id) {
  console.log('🏢 Getting company:', id);
  return await apiRequest(`/api/company/${id}`);
}

async function createCompany(data) {
  console.log('➕ Creating company:', data.name);
  return await apiRequest('/api/company', {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

async function updateCompany(id, data) {
  console.log('✏️ Updating company:', id);
  return await apiRequest(`/api/company/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data)
  });
}

async function deleteCompany(id) {
  console.log('🗑️ Deleting company:', id);
  return await apiRequest(`/api/company/${id}`, {
    method: 'DELETE'
  });
}

// =====================================================
// Test Scenarios
// =====================================================

async function testFullFlow() {
  console.log('\n🚀 Starting Full Test Flow...\n');

  // 1. Register a new user
  const userData = {
    personnelCode: `TEST${Date.now()}`,
    password: 'Test@123',
    firstName: 'تست',
    lastName: 'کاربر',
    gender: 0,
    mobileNumber: '09123456789',
    insuranceCode: '1234567890'
  };

  console.log('\n--- Step 1: Register ---');
  const registerResult = await register(userData);
  if (!registerResult.success) {
    console.error('Registration failed!');
    return;
  }

  // 2. Get user permissions
  console.log('\n--- Step 2: Get User Permissions ---');
  await getUserPermissions();

  // 3. Get education degrees
  console.log('\n--- Step 3: Get Education Degrees ---');
  await getEducationDegrees();

  // 4. Create education degree
  console.log('\n--- Step 4: Create Education Degree ---');
  const degreeResult = await createEducationDegree({
    name: `مدرک تست ${Date.now()}`,
    isActive: true
  });

  // 5. Get users list
  console.log('\n--- Step 5: Get Users List ---');
  await getUsers({ page: 1, pageSize: 10 });

  // 6. Update user with education degree
  if (degreeResult.success && currentUser) {
    console.log('\n--- Step 6: Update User with Education Degree ---');
    await updateUser(currentUser.userId, {
      firstName: currentUser.firstName,
      lastName: currentUser.lastName,
      gender: currentUser.gender,
      educationDegreeId: degreeResult.data.id
    });
  }

  // 7. Get companies
  console.log('\n--- Step 7: Get Companies ---');
  await getCompanies({ page: 1, pageSize: 10 });

  console.log('\n✅ Full test flow completed!\n');
}

async function testQuickLogin(personnelCode = '12345', password = 'Pass@123') {
  console.log('\n🚀 Quick Login Test...\n');
  const result = await login(personnelCode, password);
  
  if (result.success) {
    await getUserPermissions();
  }
  
  return result;
}

// =====================================================
// Export for use
// =====================================================

window.JarApiTest = {
  // Auth
  register,
  login,
  getUserPermissions,
  forgotPassword,
  resetPassword,
  clearAuth,
  
  // Users
  getUsers,
  updateUser,
  
  // Education Degrees
  getEducationDegrees,
  getEducationDegree,
  createEducationDegree,
  updateEducationDegree,
  deleteEducationDegree,
  
  // Companies
  getCompanies,
  getCompany,
  createCompany,
  updateCompany,
  deleteCompany,
  
  // Test scenarios
  testFullFlow,
  testQuickLogin,
  
  // Helpers
  setToken,
  getToken,
  
  // State
  get currentUser() { return currentUser; },
  get token() { return authToken; }
};

console.log(`
╔═══════════════════════════════════════════╗
║   JarApi Test Script Loaded!              ║
╠═══════════════════════════════════════════╣
║                                           ║
║  Usage:                                   ║
║  ------                                   ║
║                                           ║
║  // Full test flow                        ║
║  JarApiTest.testFullFlow()                ║
║                                           ║
║  // Quick login                           ║
║  JarApiTest.testQuickLogin('12345', 'pw') ║
║                                           ║
║  // Individual APIs                       ║
║  JarApiTest.login('code', 'pass')         ║
║  JarApiTest.getUsers({page: 1})           ║
║  JarApiTest.getEducationDegrees()         ║
║                                           ║
║  // Check current state                   ║
║  JarApiTest.currentUser                   ║
║  JarApiTest.token                         ║
║                                           ║
╚═══════════════════════════════════════════╝
`);
