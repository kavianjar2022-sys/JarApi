using JarApi.Models;

namespace JarApi.DTOs;

// DTO برای اطلاعات کامل کاربر بعد از لاگین
public class UserPermissionsDto
{
    public string UserId { get; set; } = string.Empty;
    public string PersonnelCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // جنسیت کاربر - می‌تواند null باشد اگر اطلاعاتی موجود نباشد
    public Gender? Gender { get; set; }
    // مدرک تحصیلی کاربر - می‌تواند null باشد اگر تعیین نشده باشد
    public EducationDegreeDto? EducationDegree { get; set; }
    // سمت شغلی کاربر - می‌تواند null باشد اگر تعیین نشده باشد
    public JobPositionDto? JobPosition { get; set; }
    public List<string> Roles { get; set; } = new();
    // آیا کاربر دسترسی سراسری (دیدن همه شرکت‌ها) دارد
    public bool IsGlobalAccess { get; set; }
    // لیست شرکت‌هایی که کاربر در آن‌ها سمت دارد
    public List<CompanyDto> Companies { get; set; } = new();
    // لیست واحدهایی که کاربر در آن‌ها سمت دارد
    public List<UnitInfoDto> Units { get; set; } = new();
    // واحد اصلی/انتخاب‌شده کاربر؛ اگر هیچ واحدی نداشته باشد null خواهد بود
    public UnitInfoDto? Unit { get; set; }
    // شیفت فعال کاربر - اگر هیچ شیفتی تعیین نشده باشد null خواهد بود
    public ShiftInfoDto? CurrentShift { get; set; }
    public List<UserMenuDto> Menus { get; set; } = new();
    public List<UserWidgetDto> Widgets { get; set; } = new();
}

public class UserMenuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentMenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<UserMenuDto>? SubMenus { get; set; }
}

public class UserWidgetDto
{
    public Guid Id { get; set; }
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
