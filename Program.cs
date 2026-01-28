using JarApi.Data;
using JarApi.Models;
using JarApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Rate Limiting - باید قبل از سایر سرویس‌ها تنظیم شود
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = false; // از کد پرسنلی استفاده می‌کنیم
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Validate JWT SecretKey is configured
if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey is not configured or is too short. " +
        "Please set 'JwtSettings:SecretKey' in User Secrets (Development) or Environment Variables (Production). " +
        "The key must be at least 32 characters long. " +
        "Example: dotnet user-secrets set \"JwtSettings:SecretKey\" \"YourStrongRandomKey32CharsOrMore\"");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };

    // Debug events (Development only)
    if (builder.Environment.IsDevelopment())
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[Jwt] OnAuthenticationFailed - Exception: {context.Exception?.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"[Jwt] OnTokenValidated - Principal: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    }
});

builder.Services.AddAuthorization();

// CORS for frontend (Vite dev server on 5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFromFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddMvc();
// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JarApi - احراز هویت با کد پرسنلی",
        Version = "v1.3",
        Description = "سیستم احراز هویت با ASP.NET Core Identity و JWT Token"
    });

    // تنظیمات JWT در Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "توکن JWT را وارد کنید. مثال: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // No explicit servers - allow Swagger to work with any URL
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        tags: new[] { "db", "sql" });


// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Use exception handler
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for easy API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JarApi v1");
    c.RoutePrefix = string.Empty; // Swagger در روت برنامه
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve static files (for test-api.html, test-api.js, etc.)
app.UseStaticFiles();

// Use Rate Limiting
app.UseIpRateLimiting();

// Apply CORS before auth to affect 401/403 as well
app.UseCors("AllowFromFrontend");


// Routing
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Test page shortcut: http://localhost:5257/test-api
app.MapGet("/test-api", async (HttpContext context) =>
{
    var filePath = Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "test-api.html");
    if (!File.Exists(filePath))
        return Results.NotFound(new { success = false, message = "test-api.html یافت نشد" });

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(filePath);
    return Results.Empty;
}).AllowAnonymous();

app.MapControllers();

// Health check endpoint with detailed response
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                exception = e.Value.Exception?.Message,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.Run();

// برای دسترسی در Integration Tests
public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
