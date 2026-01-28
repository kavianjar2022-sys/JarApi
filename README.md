# JarApi

A simple ASP.NET Core Web API targeting .NET 9.0 with ASP.NET Core Identity authentication using JWT tokens. The authentication system uses Personnel Code instead of email and includes custom user fields.

## ⚠️ Security Notice

**CRITICAL:** This application requires configuration before it can run:

1. **JWT SecretKey is REQUIRED** - The application will fail to start without it
2. **Never commit passwords or secrets** to source control
3. **Use User Secrets** for development and **Environment Variables** for production
4. See [SECURITY.md](SECURITY.md) for detailed security guidelines

### Quick Start Configuration

```bash
# Set JWT Secret Key (REQUIRED - minimum 32 characters)
dotnet user-secrets set "JwtSettings:SecretKey" "$(openssl rand -base64 48)"

# Optional: Set custom database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=JarApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

## Features

- JWT Bearer Authentication
- Custom Identity with Personnel Code-based login
- Custom user fields: FaceCode, BirthDate, HireDate, FirstName, LastName, MobileNumber, InsuranceCode, HomePhoneNumber

## Prerequisites

- .NET SDK 9.0 or later
- SQL Server (LocalDB or full instance)

## Configuration

### Security-First Configuration

**For Development:** Use User Secrets (recommended)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=JarApiDb;User Id=YourUser;Password=YourPassword;TrustServerCertificate=True"
dotnet user-secrets set "JwtSettings:SecretKey" "YourVeryStrongRandomSecretKey32CharactersOrMore"
```

**For Production:** Use Environment Variables
```bash
# See SECURITY.md for detailed instructions
export ConnectionStrings__DefaultConnection="..."
export JwtSettings__SecretKey="..."
```

### Database Connection

The default `appsettings.json` uses Windows Authentication (Trusted_Connection):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=JarApiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### JWT Settings

⚠️ **JWT SecretKey is now REQUIRED and validated at startup**

The `appsettings.json` file intentionally has an empty SecretKey. You **MUST** configure it using User Secrets or Environment Variables:

```bash
# Development (User Secrets) - REQUIRED
dotnet user-secrets set "JwtSettings:SecretKey" "YourVeryStrongRandomSecretKeyMustBe32CharactersOrMore"

# Production (Environment Variable)
export JwtSettings__SecretKey="YourVeryStrongRandomSecretKeyMustBe32CharactersOrMore"
```

The application will throw an exception on startup if:
- SecretKey is not set
- SecretKey is less than 32 characters

## Database Setup

```powershell
# Apply migrations to create database
dotnet ef database update
```

If you don't have SQL Server configured, you can:

1. Install SQL Server LocalDB
2. Or modify the connection string to point to your SQL Server instance

## Build & Run

```powershell
# Build
dotnet build

# Run
dotnet run
```

## Frontend Handoff

- Base URL (Development): `http://localhost:5257`
- Full API documentation: see [API-Documentation-FA.md](API-Documentation-FA.md)
- CORS is enabled for `http://localhost:5173` and `http://127.0.0.1:5173`

Use the JWT token returned from login in the `Authorization` header as `Bearer <token>` for protected endpoints.

## API Endpoints

### Authentication

**Register a new user**

```
POST /api/auth/register
Content-Type: application/json

{
  "personnelCode": "EMP001",
  "password": "Password123",
  "firstName": "علی",
  "lastName": "احمدی",
  "faceCode": "FACE001",
  "birthDate": "1990-01-01",
  "hireDate": "2020-01-01",
  "mobileNumber": "09123456789",
  "insuranceCode": "INS001",
  "homePhoneNumber": "02112345678"
}
```

**Login**

```
POST /api/auth/login
Content-Type: application/json

{
  "personnelCode": "EMP001",
  "password": "Password123"
}
```

Response includes JWT token:

```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "ورود با موفقیت انجام شد",
  "userInfo": {
    "personnelCode": "EMP001",
    "firstName": "علی",
    "lastName": "احمدی",
    ...
  }
}
```

### Protected Endpoints

Use the JWT token in the Authorization header:

```
Authorization: Bearer {your-token-here}
```

### Sample Endpoint

```
GET /weatherforecast
```

Returns sample weather forecast data (this endpoint is public in the current implementation).

## Project Structure

- `Models/` - ApplicationUser and data models
- `Data/` - Entity Framework DbContext
- `Controllers/` - API controllers (AuthController)
- `DTOs/` - Data Transfer Objects for API requests/responses
- `Migrations/` - EF Core database migrations

## Notes

- Personnel Code is used as the primary identifier instead of email
- Password requirements: minimum 6 characters, requires uppercase, lowercase, and digit
- JWT tokens expire after 60 minutes (configurable in appsettings.json)
