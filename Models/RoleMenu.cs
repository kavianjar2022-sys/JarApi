using Microsoft.AspNetCore.Identity;

namespace JarApi.Models;

// جدول رابط بین نقش‌ها و منوها
public class RoleMenu
{
    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole Role { get; set; } = null!;
    
    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;
    
    // دسترسی‌های اضافی (اختیاری)
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}
