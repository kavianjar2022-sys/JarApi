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
builder.Services.AddHealthChecks();

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

// Health check endpoint
app.MapHealthChecks("/health");

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
