using System.ComponentModel.DataAnnotations;

namespace JarApi.DTOs;

public class UnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? ParentUnitId { get; set; }
    public string? ParentUnitName { get; set; }
    public bool IsActive { get; set; }
    public List<UnitDto>? SubUnits { get; set; }
}

public class CreateUnitDto
{
    [Required(ErrorMessage = "نام واحد الزامی است")]
    public string Name { get; set; } = string.Empty;
    
    public string? Code { get; set; }
    
    [Required(ErrorMessage = "شناسه شرکت الزامی است")]
    public int CompanyId { get; set; }
    
    public int? ParentUnitId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUnitDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int? CompanyId { get; set; }
    public int? ParentUnitId { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignRoleToUserInUnitDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه واحد الزامی است")]
    public int UnitId { get; set; }
}

public class RemoveRoleFromUserInUnitDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه واحد الزامی است")]
    public int UnitId { get; set; }
}

public class UnitInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
}
