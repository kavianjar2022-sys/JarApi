using System.ComponentModel.DataAnnotations;

namespace JarApi.DTOs;

// DTOs برای Menu
public class MenuDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentMenuId { get; set; }
    public bool IsActive { get; set; }
    public List<MenuDto>? SubMenus { get; set; }
}

public class CreateMenuDto
{
    [Required(ErrorMessage = "نام منو الزامی است")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "عنوان منو الزامی است")]
    public string Title { get; set; } = string.Empty;

    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentMenuId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateMenuDto
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int? DisplayOrder { get; set; }
    public int? ParentMenuId { get; set; }
    public bool? IsActive { get; set; }
}

// DTOs برای Widget
public class WidgetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string WidgetType { get; set; } = string.Empty;
    public string? Configuration { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateWidgetDto
{
    [Required(ErrorMessage = "نام ویجت الزامی است")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "عنوان ویجت الزامی است")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Icon { get; set; }

    [Required(ErrorMessage = "نوع ویجت الزامی است")]
    public string WidgetType { get; set; } = string.Empty;

    public string? Configuration { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateWidgetDto
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? WidgetType { get; set; }
    public string? Configuration { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

// DTOs برای تخصیص دسترسی‌ها به نقش‌ها
public class AssignMenusToRoleDto
{
    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "لیست منوها الزامی است")]
    public List<MenuPermissionDto> Menus { get; set; } = new();
}

public class MenuPermissionDto
{
    public int MenuId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}

public class AssignWidgetsToRoleDto
{
    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "لیست ویجت‌ها الزامی است")]
    public List<WidgetPermissionDto> Widgets { get; set; } = new();
}

public class WidgetPermissionDto
{
    public int WidgetId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanConfigure { get; set; } = false;
}

// DTO برای ایجاد نقش جدید
public class CreateRoleDto
{
    [Required(ErrorMessage = "نام نقش الزامی است")]
    public string RoleName { get; set; } = string.Empty;
}

// DTO برای تخصیص نقش به کاربر
public class AssignRoleToUserDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نقش الزامی است")]
    public string RoleName { get; set; } = string.Empty;
}

public class AssignRoleToUserInCompanyDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "شناسه شرکت الزامی است")]
    public int CompanyId { get; set; }
}

public class RemoveRoleFromUserInCompanyDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "شناسه نقش الزامی است")]
    public string RoleId { get; set; } = string.Empty;

    [Required(ErrorMessage = "شناسه شرکت الزامی است")]
    public int CompanyId { get; set; }
}
