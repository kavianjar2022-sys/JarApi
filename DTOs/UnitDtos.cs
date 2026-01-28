using System.ComponentModel.DataAnnotations;

namespace JarApi.DTOs;

public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public Guid? ParentUnitId { get; set; }
    public string? ParentUnitName { get; set; }
    public bool IsActive { get; set; }
    public List<UnitDto>? SubUnits { get; set; }
}

public class CreateUnitDto
{
    [Required(ErrorMessage = "نام واحد الزامی است")]
    [MaxLength(200, ErrorMessage = "نام واحد نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50, ErrorMessage = "کد واحد نباید بیشتر از ۵۰ کاراکتر باشد")]
    public string? Code { get; set; }
    
    [Required(ErrorMessage = "شناسه شرکت الزامی است")]
    public Guid CompanyId { get; set; }
    
    public Guid? ParentUnitId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUnitDto
{
    [MaxLength(200, ErrorMessage = "نام واحد نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string? Name { get; set; }
    
    [MaxLength(50, ErrorMessage = "کد واحد نباید بیشتر از ۵۰ کاراکتر باشد")]
    public string? Code { get; set; }
    
    public Guid? CompanyId { get; set; }
    public Guid? ParentUnitId { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignRoleToUserInUnitDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه واحد الزامی است")]
    public Guid UnitId { get; set; }
}

public class RemoveRoleFromUserInUnitDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "شناسه واحد الزامی است")]
    public Guid UnitId { get; set; }
}

public class UnitInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
}
