namespace JarApi.Models;

public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    
    // ارتباط با شرکت
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    // سلسله‌مراتب واحد (واحد اصلی → زیرواحد)
    public int? ParentUnitId { get; set; }
    public Unit? ParentUnit { get; set; }
    public ICollection<Unit> SubUnits { get; set; } = new List<Unit>();
    
    public bool IsActive { get; set; } = true;
    
    // ارتباط با تخصیص نقش کاربر در واحد
    public ICollection<UserRoleUnit> UserRoleUnits { get; set; } = new List<UserRoleUnit>();
}
