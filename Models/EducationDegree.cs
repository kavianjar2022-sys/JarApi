namespace JarApi.Models;

/// <summary>
/// مدل مدرک تحصیلی
/// </summary>
public class EducationDegree
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// نام مدرک تحصیلی (مثلاً: دیپلم، کارشناسی، کارشناسی ارشد، دکترا)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    // ارتباط با کاربران
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
