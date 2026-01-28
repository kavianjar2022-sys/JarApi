namespace JarApi.Models;

public class Widget
{
    public Guid Id { get; set; }
    
    // نام ویجت
    public string Name { get; set; } = string.Empty;
    
    // عنوان فارسی ویجت
    public string Title { get; set; } = string.Empty;
    
    // توضیحات
    public string? Description { get; set; }
    
    // آیکون ویجت
    public string? Icon { get; set; }
    
    // نوع ویجت (Chart, Card, Table, etc.)
    public string WidgetType { get; set; } = string.Empty;
    
    // تنظیمات JSON ویجت
    public string? Configuration { get; set; }
    
    // ترتیب نمایش
    public int DisplayOrder { get; set; }
    
    // فعال/غیرفعال
    public bool IsActive { get; set; } = true;
    
    // رابطه با نقش‌ها
    public ICollection<RoleWidget> RoleWidgets { get; set; } = new List<RoleWidget>();
}
