# JarApi

A simple ASP.NET Core Web API targeting .NET 9.0 with ASP.NET Core Identity authentication using JWT tokens. The authentication system uses Personnel Code instead of email and includes custom user fields.

## Features

- JWT Bearer Authentication
- Custom Identity with Personnel Code-based login
- Custom user fields: FaceCode, BirthDate, HireDate, FirstName, LastName, MobileNumber, InsuranceCode, HomePhoneNumber

## Prerequisites

- .NET SDK 9.0 or later
- SQL Server (LocalDB or full instance)

## Configuration

### Database Connection

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=JarApiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### JWT Settings

Configure JWT settings in `appsettings.json`:

```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyForJwtTokenGenerationMinimum32Characters!",
  "Issuer": "JarApi",
  "Audience": "JarApiClients",
  "ExpiryMinutes": 60
}
```

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
