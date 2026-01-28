using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace JarApi.Middleware;

/// <summary>
/// Global exception handler برای مدیریت خطاهای غیرمنتظره
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "خطای غیرمنتظره رخ داد: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "خطای سرور",
            Detail = httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? exception.Message
                : "خطای غیرمنتظره‌ای رخ داد. لطفاً با پشتیبانی تماس بگیرید"
        };

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = problemDetails.Detail,
            errors = httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? new { exception = exception.ToString() }
                : null
        }, cancellationToken);

        return true;
    }
}
