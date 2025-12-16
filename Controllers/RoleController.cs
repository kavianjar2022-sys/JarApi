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
[Authorize]
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
    public async Task<ActionResult> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return Ok(roles.Select(r => new { r.Id, r.Name, r.IsGlobalAccess }));
    }

    [HttpPost]
    public async Task<ActionResult> CreateRole(CreateRoleDto dto)
    {
        var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
        if (roleExists)
            return BadRequest(new { message = "این نقش قبلاً ایجاد شده است" });

        var role = new ApplicationRole { Name = dto.RoleName, NormalizedName = dto.RoleName.ToUpperInvariant(), IsGlobalAccess = false };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { message = "خطا در ایجاد نقش", errors = result.Errors });

        return Ok(new { message = "نقش با موفقیت ایجاد شد" });
    }

    // تخصیص نقش به کاربر
    [HttpPost("assign-role-to-user")]
    public async Task<ActionResult> AssignRoleToUser(AssignRoleToUserDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);

        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
        if (!roleExists)
            return NotFound(new { message = "نقش یافت نشد" });

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return BadRequest(new { message = "خطا در تخصیص نقش", errors = result.Errors });

        return Ok(new { message = $"نقش {dto.RoleName} به کاربر {dto.PersonnelCode} اختصاص داده شد" });
    }

    // تخصیص نقش به کاربر در سطح یک شرکت
    [HttpPost("assign-role-to-user-in-company")]
    public async Task<ActionResult> AssignRoleToUserInCompany(AssignRoleToUserInCompanyDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { message = "کاربر یافت نشد" });

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null) return NotFound(new { message = "نقش یافت نشد" });

        var company = await _context.Companies.FindAsync(dto.CompanyId);
        if (company == null) return NotFound(new { message = "شرکت یافت نشد" });

        var existing = await _context.UserRoleCompanies.FindAsync(user.Id, role.Id, dto.CompanyId);
        if (existing != null)
            return BadRequest(new { message = "این تخصیص قبلاً وجود دارد" });

        _context.UserRoleCompanies.Add(new UserRoleCompany
        {
            UserId = user.Id,
            RoleId = role.Id,
            CompanyId = dto.CompanyId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "نقش به کاربر در شرکت اختصاص داده شد" });
    }

    // حذف نقش از کاربر در سطح یک شرکت
    [HttpPost("remove-role-from-user-in-company")]
    public async Task<ActionResult> RemoveRoleFromUserInCompany(RemoveRoleFromUserInCompanyDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { message = "کاربر یافت نشد" });

        var existing = await _context.UserRoleCompanies.FindAsync(user.Id, dto.RoleId, dto.CompanyId);
        if (existing == null)
            return NotFound(new { message = "تخصیص یافت نشد" });

        _context.UserRoleCompanies.Remove(existing);
        await _context.SaveChangesAsync();

        return Ok(new { message = "نقش از کاربر در شرکت حذف شد" });
    }

    // تخصیص نقش به کاربر در سطح یک واحد
    [HttpPost("assign-role-to-user-in-unit")]
    public async Task<ActionResult> AssignRoleToUserInUnit(AssignRoleToUserInUnitDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { message = "کاربر یافت نشد" });

        var role = await _roleManager.FindByIdAsync(dto.RoleId);
        if (role == null) return NotFound(new { message = "نقش یافت نشد" });

        var unit = await _context.Units.FindAsync(dto.UnitId);
        if (unit == null) return NotFound(new { message = "واحد یافت نشد" });

        var existing = await _context.UserRoleUnits.FindAsync(user.Id, role.Id, dto.UnitId);
        if (existing != null)
            return BadRequest(new { message = "این تخصیص قبلاً وجود دارد" });

        _context.UserRoleUnits.Add(new UserRoleUnit
        {
            UserId = user.Id,
            RoleId = role.Id,
            UnitId = dto.UnitId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "نقش به کاربر در واحد اختصاص داده شد" });
    }

    // حذف نقش از کاربر در سطح یک واحد
    [HttpPost("remove-role-from-user-in-unit")]
    public async Task<ActionResult> RemoveRoleFromUserInUnit(RemoveRoleFromUserInUnitDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);
        if (user == null) return NotFound(new { message = "کاربر یافت نشد" });

        var existing = await _context.UserRoleUnits.FindAsync(user.Id, dto.RoleId, dto.UnitId);
        if (existing == null)
            return NotFound(new { message = "تخصیص یافت نشد" });

        _context.UserRoleUnits.Remove(existing);
        await _context.SaveChangesAsync();

        return Ok(new { message = "نقش از کاربر در واحد حذف شد" });
    }

    // حذف نقش از کاربر
    [HttpPost("remove-role-from-user")]
    public async Task<ActionResult> RemoveRoleFromUser(AssignRoleToUserDto dto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PersonnelCode == dto.PersonnelCode);

        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        var result = await _userManager.RemoveFromRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
            return BadRequest(new { message = "خطا در حذف نقش", errors = result.Errors });

        return Ok(new { message = $"نقش {dto.RoleName} از کاربر {dto.PersonnelCode} حذف شد" });
    }

    // تخصیص منوها به نقش
    [HttpPost("assign-menus")]
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

    // تخصیص ویجت‌ها به نقش
    [HttpPost("assign-widgets")]
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

    // دریافت منوهای یک نقش
    [HttpGet("{roleId}/menus")]
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
}
