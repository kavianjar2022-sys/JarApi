using System.ComponentModel.DataAnnotations;

namespace JarApi.DTOs;

public class JobPositionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int Level { get; set; }
    public Guid? ParentPositionId { get; set; }
    public string? ParentPositionTitle { get; set; }
    public bool IsActive { get; set; }
}

public class CreateJobPositionDto
{
    [Required(ErrorMessage = "عنوان شغلی الزامی است")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "سطح سازمانی الزامی است")]
    [Range(1, 100, ErrorMessage = "سطح باید بین 1 تا 100 باشد")]
    public int Level { get; set; }

    public Guid? ParentPositionId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateJobPositionDto : CreateJobPositionDto { }

public class JobPositionHierarchyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public List<JobPositionHierarchyDto> SubPositions { get; set; } = new();
}
