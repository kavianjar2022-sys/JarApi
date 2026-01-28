using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class RoleController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleController(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // مدیریت نقش‌ها
    [HttpGet]
    [Authorize]
    public async Task<ActionResult> GetAllRoles(
        [FromQuery] Guid? companyId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isGlobalAccess = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _roleManager.Roles.AsQueryable();
        
        // فیلتر بر اساس شرکت
        if (companyId.HasValue)
        {
            // نقش‌های مربوط به شرکت + نقش‌های سراسری
            query = query.Where(r => r.CompanyId == null || r.CompanyId == companyId.Value);
        }

        // جستجو در نام نقش
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(r => r.Name != null && r.Name.ToLower().Contains(searchTerm));
        }

        // فیلتر بر اساس دسترسی سراسری
        if (isGlobalAccess.HasValue)
            query = query.Where(r => r.IsGlobalAccess == isGlobalAccess.Value);

        // تعداد کل نتایج
        var totalCount = await query.CountAsync();
        
        // صفحه‌بندی
        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new 
            { 
                r.Id, 
                r.Name, 
                r.IsGlobalAccess, 
                r.CompanyId,
                CompanyName = r.Company != null ? r.Company.Name : null
            })
            .ToListAsync();
            
        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            roles
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateRole(CreateRoleDto dto)
    {
        // بررسی وجود شرکت
        if (dto.CompanyId.HasValue)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId.Value);
            if (company == null)
                return NotFound(new { success = false, message = "شرکت یافت نشد" });
        }

        // بررسی تکراری بودن نام نقش در همان شرکت
        var existingRole = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Name == dto.RoleName && r.CompanyId == dto.CompanyId);
        if (existingRole != null)
            return BadRequest(new { success = false, message = "این نقش قبلاً در این شرکت ایجاد شده است" });

        var role = new ApplicationRole 
        { 
            Name = dto.RoleName, 
            NormalizedName = dto.RoleName.ToUpperInvariant(), 
            IsGlobalAccess = dto.IsGlobalAccess,
            CompanyId = dto.CompanyId
        };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "خطا در ایجاد نقش", errors = result.Errors });

        return Ok(new { success = true, message = "نقش با موفقیت ایجاد شد", roleId = role.Id, companyId = role.CompanyId });
    }

    // تخصیص نقش سراسری به کاربر
    [HttpPost("assign-role-to-user")]
    [Authorize]
    public async Task<ActionResult> AssignRoleToUser(AssignRoleToUserDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);

        if (user == null)
            return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { success = false, message = "نقش یافت نشد" });

        // فقط نقش‌های سراسری (بدون CompanyId) قابل تخصیص مستقیم هستند
        if (role.CompanyId.HasValue)
            return BadRequest(new { success = false, message = "این نقش مخصوص یک شرکت است. از assign-role-to-user-in-company استفاده کنید" });

        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "خطا در تخصیص نقش", errors = result.Errors });

        return Ok(new { success = true, message = $"نقش {role.Name} به کاربر {dto.PersonnelCode} اختصاص داده شد" });
    }

    // تخصیص نقش به کاربر در سطح یک شرکت
    [HttpPost("assign-role-to-user-in-company")]
    [Authorize]
    public async Task<ActionResult> AssignRoleToUserInCompany(AssignRoleToUserInCompanyDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null) return NotFound(new { success = false, message = "نقش یافت نشد" });

        var company = await _context.Companies.FindAsync(dto.CompanyId);
        if (company == null) return NotFound(new { success = false, message = "شرکت یافت نشد" });

        // بررسی اینکه نقش به این شرکت تعلق دارد یا سراسری است
        if (role.CompanyId.HasValue && role.CompanyId.Value != dto.CompanyId)
            return BadRequest(new { success = false, message = "این نقش به شرکت دیگری تعلق دارد" });

        var existing = await _context.UserRoleCompanies.FindAsync(user.Id, role.Id, dto.CompanyId);
        if (existing != null)
            return BadRequest(new { success = false, message = "این تخصیص قبلاً وجود دارد" });

        _context.UserRoleCompanies.Add(new UserRoleCompany
        {
            UserId = user.Id,
            RoleId = role.Id,
            CompanyId = dto.CompanyId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "نقش به کاربر در شرکت اختصاص داده شد" });
    }

    // حذف نقش از کاربر در سطح یک شرکت
    [HttpPost("remove-role-from-user-in-company")]
    [Authorize]
    public async Task<ActionResult> RemoveRoleFromUserInCompany(RemoveRoleFromUserInCompanyDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var existing = await _context.UserRoleCompanies.FindAsync(user.Id, dto.RoleId, dto.CompanyId);
        if (existing == null)
            return NotFound(new { success = false, message = "تخصیص یافت نشد" });

        _context.UserRoleCompanies.Remove(existing);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "نقش از کاربر در شرکت حذف شد" });
    }

    // تخصیص نقش به کاربر در سطح یک واحد
    [HttpPost("assign-role-to-user-in-unit")]
    [Authorize]
    public async Task<ActionResult> AssignRoleToUserInUnit(AssignRoleToUserInUnitDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null) return NotFound(new { success = false, message = "نقش یافت نشد" });

        var unit = await _context.Units
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == dto.UnitId);
        if (unit == null) return NotFound(new { success = false, message = "واحد یافت نشد" });

        // بررسی اینکه نقش به شرکت واحد تعلق دارد یا سراسری است
        if (role.CompanyId.HasValue && role.CompanyId.Value != unit.CompanyId)
            return BadRequest(new { success = false, message = "این نقش به شرکت دیگری تعلق دارد" });

        var existing = await _context.UserRoleUnits.FindAsync(user.Id, role.Id, dto.UnitId);
        if (existing != null)
            return BadRequest(new { success = false, message = "این تخصیص قبلاً وجود دارد" });

        _context.UserRoleUnits.Add(new UserRoleUnit
        {
            UserId = user.Id,
            RoleId = role.Id,
            UnitId = dto.UnitId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "نقش به کاربر در واحد اختصاص داده شد" });
    }

    // حذف نقش از کاربر در سطح یک واحد
    [HttpPost("remove-role-from-user-in-unit")]
    [Authorize]
    public async Task<ActionResult> RemoveRoleFromUserInUnit(RemoveRoleFromUserInUnitDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var existing = await _context.UserRoleUnits.FindAsync(user.Id, dto.RoleId, dto.UnitId);
        if (existing == null)
            return NotFound(new { success = false, message = "تخصیص یافت نشد" });

        _context.UserRoleUnits.Remove(existing);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "نقش از کاربر در واحد حذف شد" });
    }

    // حذف نقش سراسری از کاربر
    [HttpPost("remove-role-from-user")]
    [Authorize]
    public async Task<ActionResult> RemoveRoleFromUser(AssignRoleToUserDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);

        if (user == null)
            return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { success = false, message = "نقش یافت نشد" });

        // فقط نقش‌های سراسری قابل حذف مستقیم هستند
        if (role.CompanyId.HasValue)
            return BadRequest(new { success = false, message = "این نقش مخصوص یک شرکت است. از remove-role-from-user-in-company استفاده کنید" });

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "خطا در حذف نقش", errors = result.Errors });

        return Ok(new { success = true, message = $"نقش {role.Name} از کاربر {dto.PersonnelCode} حذف شد" });
    }

    // تخصیص منوها به نقش
    [HttpPost("assign-menus")]
    [Authorize]
    public async Task<ActionResult> AssignMenusToRole(AssignMenusToRoleDto dto)
    {
        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { message = "نقش یافت نشد" });

        // حذف دسترسی‌های قبلی
        var existingMenus = await _context.RoleMenus
            .Where(rm => rm.RoleId == dto.RoleId)
            .ToListAsync();
        _context.RoleMenus.RemoveRange(existingMenus);

        // اضافه کردن دسترسی‌های جدید
        foreach (var menu in dto.Menus)
        {
            _context.RoleMenus.Add(new RoleMenu
            {
                RoleId = dto.RoleId,
                MenuId = menu.MenuId,
                CanView = menu.CanView,
                CanCreate = menu.CanCreate,
                CanEdit = menu.CanEdit,
                CanDelete = menu.CanDelete
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "دسترسی‌های منو با موفقیت تخصیص داده شد" });
    }

    // ویرایش دسترسی‌های یک منوی خاص برای نقش
    [HttpPut("update-menu-permission")]
    [Authorize]
    public async Task<ActionResult> UpdateMenuPermission(UpdateMenuPermissionDto dto)
    {
        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { message = "نقش یافت نشد" });

        var menu = await _context.Menus.FindAsync(dto.MenuId);
        if (menu == null)
            return NotFound(new { message = "منو یافت نشد" });

        var roleMenu = await _context.RoleMenus
            .FirstOrDefaultAsync(rm => rm.RoleId == dto.RoleId && rm.MenuId == dto.MenuId);

        if (roleMenu == null)
            return NotFound(new { message = "این منو به نقش اختصاص داده نشده است" });

        roleMenu.CanView = dto.CanView;
        roleMenu.CanCreate = dto.CanCreate;
        roleMenu.CanEdit = dto.CanEdit;
        roleMenu.CanDelete = dto.CanDelete;

        await _context.SaveChangesAsync();

        return Ok(new { message = "دسترسی‌های منو با موفقیت به‌روزرسانی شد" });
    }

    // تخصیص ویجت‌ها به نقش
    [HttpPost("assign-widgets")]
    [Authorize]
    public async Task<ActionResult> AssignWidgetsToRole(AssignWidgetsToRoleDto dto)
    {
        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { message = "نقش یافت نشد" });

        // حذف دسترسی‌های قبلی
        var existingWidgets = await _context.RoleWidgets
            .Where(rw => rw.RoleId == dto.RoleId)
            .ToListAsync();
        _context.RoleWidgets.RemoveRange(existingWidgets);

        // اضافه کردن دسترسی‌های جدید
        foreach (var widget in dto.Widgets)
        {
            _context.RoleWidgets.Add(new RoleWidget
            {
                RoleId = dto.RoleId,
                WidgetId = widget.WidgetId,
                CanView = widget.CanView,
                CanConfigure = widget.CanConfigure
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "دسترسی‌های ویجت با موفقیت تخصیص داده شد" });
    }

    // ویرایش دسترسی‌های یک ویجت خاص برای نقش
    [HttpPut("update-widget-permission")]
    [Authorize]
    public async Task<ActionResult> UpdateWidgetPermission(UpdateWidgetPermissionDto dto)
    {
        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null)
            return NotFound(new { message = "نقش یافت نشد" });

        var widget = await _context.Widgets.FindAsync(dto.WidgetId);
        if (widget == null)
            return NotFound(new { message = "ویجت یافت نشد" });

        var roleWidget = await _context.RoleWidgets
            .FirstOrDefaultAsync(rw => rw.RoleId == dto.RoleId && rw.WidgetId == dto.WidgetId);

        if (roleWidget == null)
            return NotFound(new { message = "این ویجت به نقش اختصاص داده نشده است" });

        roleWidget.CanView = dto.CanView;
        roleWidget.CanConfigure = dto.CanConfigure;

        await _context.SaveChangesAsync();

        return Ok(new { message = "دسترسی‌های ویجت با موفقیت به‌روزرسانی شد" });
    }

    // دریافت منوهای یک نقش
    [HttpGet("{roleId}/menus")]
    [Authorize]
    public async Task<ActionResult> GetRoleMenus(string roleId)
    {
        var menus = await _context.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Include(rm => rm.Menu)
            .Select(rm => new
            {
                rm.MenuId,
                rm.Menu.Name,
                rm.Menu.Title,
                rm.CanView,
                rm.CanCreate,
                rm.CanEdit,
                rm.CanDelete
            })
            .ToListAsync();

        return Ok(menus);
    }

    // دریافت ویجت‌های یک نقش
    [HttpGet("{roleId}/widgets")]
    [Authorize]
    public async Task<ActionResult> GetRoleWidgets(string roleId)
    {
        var widgets = await _context.RoleWidgets
            .Where(rw => rw.RoleId == roleId)
            .Include(rw => rw.Widget)
            .Select(rw => new
            {
                rw.WidgetId,
                rw.Widget.Name,
                rw.Widget.Title,
                rw.CanView,
                rw.CanConfigure
            })
            .ToListAsync();

        return Ok(widgets);
    }

    /// <summary>
    /// ویرایش نقش
    /// </summary>
    [HttpPut("{roleId}")]
    [Authorize]
    public async Task<ActionResult> UpdateRole(string roleId, UpdateRoleDto dto)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { success = false, message = "نقش یافت نشد" });

        if (!string.IsNullOrWhiteSpace(dto.RoleName))
        {
            var existingRole = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name == dto.RoleName && r.CompanyId == role.CompanyId && r.Id != roleId);
            if (existingRole != null)
                return BadRequest(new { success = false, message = "نقشی با این نام در این شرکت وجود دارد" });
            
            role.Name = dto.RoleName;
            role.NormalizedName = dto.RoleName.ToUpperInvariant();
        }

        if (dto.IsGlobalAccess.HasValue)
            role.IsGlobalAccess = dto.IsGlobalAccess.Value;

        if (dto.CompanyId.HasValue)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId.Value);
            if (company == null)
                return NotFound(new { success = false, message = "شرکت یافت نشد" });
            role.CompanyId = dto.CompanyId.Value;
        }

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "خطا در ویرایش نقش", errors = result.Errors });

        return Ok(new { success = true, message = "نقش با موفقیت ویرایش شد" });
    }

    /// <summary>
    /// حذف نقش
    /// </summary>
    [HttpDelete("{roleId}")]
    [Authorize]
    public async Task<ActionResult> DeleteRole(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { success = false, message = "نقش یافت نشد" });

        // بررسی وجود کاربران با این نقش
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
            return BadRequest(new { success = false, message = "این نقش به کاربران اختصاص داده شده و قابل حذف نیست" });

        // بررسی وجود تخصیص در شرکت‌ها
        var companyAssignments = await _context.UserRoleCompanies
            .Where(urc => urc.RoleId == roleId)
            .AnyAsync();
        if (companyAssignments)
            return BadRequest(new { success = false, message = "این نقش در شرکت‌ها استفاده شده و قابل حذف نیست" });

        // بررسی وجود تخصیص در واحدها
        var unitAssignments = await _context.UserRoleUnits
            .Where(uru => uru.RoleId == roleId)
            .AnyAsync();
        if (unitAssignments)
            return BadRequest(new { success = false, message = "این نقش در واحدها استفاده شده و قابل حذف نیست" });

        // بررسی وجود دسترسی منو
        var menuPermissions = await _context.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .AnyAsync();
        if (menuPermissions)
            return BadRequest(new { success = false, message = "این نقش دارای دسترسی‌های منو است و قابل حذف نیست" });

        // بررسی وجود دسترسی ویجت
        var widgetPermissions = await _context.RoleWidgets
            .Where(rw => rw.RoleId == roleId)
            .AnyAsync();
        if (widgetPermissions)
            return BadRequest(new { success = false, message = "این نقش دارای دسترسی‌های ویجت است و قابل حذف نیست" });

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "خطا در حذف نقش", errors = result.Errors });

        return Ok(new { success = true, message = "نقش با موفقیت حذف شد" });
    }

    /// <summary>
    /// دریافت لیست کاربران یک نقش
    /// </summary>
    [HttpGet("{roleId}/users")]
    [Authorize]
    public async Task<ActionResult> GetRoleUsers(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { success = false, message = "نقش یافت نشد" });

        // کاربران سراسری
        var globalUsers = await _userManager.GetUsersInRoleAsync(role.Name!);

        // کاربران در شرکت‌ها
        var companyUsers = await _context.UserRoleCompanies
            .Where(urc => urc.RoleId == roleId)
            .Include(urc => urc.User)
            .Include(urc => urc.Company)
            .Select(urc => new
            {
                UserId = urc.User.Id,
                urc.User.PersonnelCode,
                urc.User.FirstName,
                urc.User.LastName,
                AssignmentType = "Company",
                CompanyId = urc.Company.Id,
                CompanyName = urc.Company.Name,
                urc.AssignedAt
            })
            .ToListAsync();

        // کاربران در واحدها
        var unitUsers = await _context.UserRoleUnits
            .Where(uru => uru.RoleId == roleId)
            .Include(uru => uru.User)
            .Include(uru => uru.Unit)
            .ThenInclude(u => u.Company)
            .Select(uru => new
            {
                UserId = uru.User.Id,
                uru.User.PersonnelCode,
                uru.User.FirstName,
                uru.User.LastName,
                AssignmentType = "Unit",
                UnitId = uru.Unit.Id,
                UnitName = uru.Unit.Name,
                CompanyId = uru.Unit.Company.Id,
                CompanyName = uru.Unit.Company.Name,
                uru.AssignedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            roleName = role.Name,
            globalUsers = globalUsers.Select(u => new
            {
                UserId = u.Id,
                u.PersonnelCode,
                u.FirstName,
                u.LastName,
                AssignmentType = "Global"
            }),
            companyUsers,
            unitUsers
        });
    }
}
