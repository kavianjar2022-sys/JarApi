namespace JarApi.Models;

public class UserRoleCompany
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole Role { get; set; } = null!;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // زمان اختصاص سمت (اختیاری)
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
