namespace JarApi.Models;

public class Menu
{
    public Guid Id { get; set; }
    
    // نام منو
    public string Name { get; set; } = string.Empty;
    
    // عنوان فارسی منو
    public string Title { get; set; } = string.Empty;
    
    // آیکون منو
    public string? Icon { get; set; }
    
    // مسیر URL
    public string? Url { get; set; }
    
    // ترتیب نمایش
    public int DisplayOrder { get; set; }
    
    // منوی والد (برای زیرمنوها)
    public Guid? ParentMenuId { get; set; }
    public Menu? ParentMenu { get; set; }
    
    // زیرمنوها
    public ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
    
    // فعال/غیرفعال
    public bool IsActive { get; set; } = true;
    
    // رابطه با نقش‌ها
    public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
