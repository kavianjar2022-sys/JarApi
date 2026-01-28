using System.ComponentModel.DataAnnotations;

namespace JarApi.Models;

public class JobPosition
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    // سطح سازمانی (1 = بالاترین سطح مثل مدیر عامل، 2 = مدیر، 3 = سرپرست، و ...)
    [Range(1, 100)]
    public int Level { get; set; }

    // سمت والد (سمت بالاتر در سلسله مراتب)
    public Guid? ParentPositionId { get; set; }
    public JobPosition? ParentPosition { get; set; }

    public bool IsActive { get; set; } = true;

    // سمت‌های زیرمجموعه
    public ICollection<JobPosition> SubPositions { get; set; } = new List<JobPosition>();

    // کاربرانی که این سمت را دارند
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
