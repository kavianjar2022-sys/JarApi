using Microsoft.AspNetCore.Identity;

namespace JarApi.Models;

public class ApplicationRole : IdentityRole
{
    // اگر true باشد، این نقش می‌تواند اطلاعات همه شرکت‌ها را ببیند
    public bool IsGlobalAccess { get; set; } = false;
    
    // شرکت مرتبط با این نقش (null = نقش سراسری)
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
}
