using Microsoft.AspNetCore.Identity;

namespace JarApi.Models;

public class ApplicationRole : IdentityRole
{
    // اگر true باشد، این نقش می‌تواند اطلاعات همه شرکت‌ها را ببیند
    public bool IsGlobalAccess { get; set; } = false;
}
