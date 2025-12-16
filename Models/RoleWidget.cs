using Microsoft.AspNetCore.Identity;

namespace JarApi.Models;

// جدول رابط بین نقش‌ها و ویجت‌ها
public class RoleWidget
{
    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole Role { get; set; } = null!;
    
    public int WidgetId { get; set; }
    public Widget Widget { get; set; } = null!;
    
    // دسترسی‌های اضافی (اختیاری)
    public bool CanView { get; set; } = true;
    public bool CanConfigure { get; set; } = false;
}
