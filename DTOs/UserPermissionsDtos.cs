namespace JarApi.DTOs;

// DTO برای اطلاعات کامل کاربر بعد از لاگین
public class UserPermissionsDto
{
    public string PersonnelCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    // آیا کاربر دسترسی سراسری (دیدن همه شرکت‌ها) دارد
    public bool IsGlobalAccess { get; set; }
    // لیست شرکت‌هایی که کاربر در آن‌ها سمت دارد
    public List<CompanyDto> Companies { get; set; } = new();
    // لیست واحدهایی که کاربر در آن‌ها سمت دارد
    public List<UnitInfoDto> Units { get; set; } = new();
    public List<UserMenuDto> Menus { get; set; } = new();
    public List<UserWidgetDto> Widgets { get; set; } = new();
}

public class UserMenuDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentMenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<UserMenuDto>? SubMenus { get; set; }
}

public class UserWidgetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string WidgetType { get; set; } = string.Empty;
    public string? Configuration { get; set; }
    public int DisplayOrder { get; set; }
    public bool CanView { get; set; }
    public bool CanConfigure { get; set; }
}

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}
