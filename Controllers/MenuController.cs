using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/MyMenu")]
public class MenuController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<object>> GetAllMenus(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool includeSubMenus = true, // این پارامتر تعیین میکنه که آیا زیرمنوها لود بشن یا نه
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        // 💡 قدم اول: query رو روی تمام منوها (نه فقط ParentMenuId == null) شروع می‌کنیم
        var baseQuery = _context.Menus.AsQueryable();

        // 💡 تغییرات جستجو در اینجا! (جستجو روی تمام فیلدهای رشته‌ای و قابل تبدیل به رشته، هم در منوها و هم در زیرمنوها)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            baseQuery = baseQuery.Where(m =>
                m.Name.ToLower().Contains(searchTerm) ||
                m.Title.ToLower().Contains(searchTerm) ||
                (m.Icon != null && m.Icon.ToLower().Contains(searchTerm)) ||
                (m.Url != null && m.Url.ToLower().Contains(searchTerm)) ||
                m.DisplayOrder.ToString().Contains(searchTerm) ||
                m.IsActive.ToString().ToLower().Contains(searchTerm) ||
                // جستجو در زیرمنوها (برای هر زیرمنو هم همین فیلدها رو چک می‌کنیم)
                m.SubMenus.Any(sm =>
                    sm.Name.ToLower().Contains(searchTerm) ||
                    sm.Title.ToLower().Contains(searchTerm) ||
                    (sm.Icon != null && sm.Icon.ToLower().Contains(searchTerm)) ||
                    (sm.Url != null && sm.Url.ToLower().Contains(searchTerm)) ||
                    sm.DisplayOrder.ToString().Contains(searchTerm) ||
                    sm.IsActive.ToString().ToLower().Contains(searchTerm)
                    // اگه زیرمنوها هم خودشون SubMenus دارن و می‌خوایم توی اون سطح هم سرچ کنیم، باید Recursive اینجا رو هم عمیق‌تر کنیم.
                    // اما بهینه‌تره که فقط دو سطح (منو و زیرمنو) رو در جستجو در نظر بگیریم تا Query خیلی سنگین نشه.
                )
            );
        }

        // فیلتر بر اساس وضعیت فعال/غیرفعال (برای منوهای اصلی که نتیجه جستجو هستن)
        if (isActive.HasValue)
            baseQuery = baseQuery.Where(m => m.IsActive == isActive.Value);

        // حالا فقط منوهایی که والد ندارند یا منوهایی که در جستجو پیدا شده‌اند و والد دارند اما می‌خواهیم نمایش دهیم را فیلتر می‌کنیم
        // این بخش نیاز به دقت بیشتری داره، چون اگه یک زیرمنو پیدا بشه، باید والد اصلی اون رو هم توی نتایج بیاریم.
        // بهترین راه اینه که ابتدا تمام IDهای منوهایی که با جستجو مطابقت دارند (هم اصلی هم زیرمنو) رو پیدا کنیم.
        // سپس منوهای اصلی (ParentMenuId == null) که شامل این IDها یا IDهای فرزندانشان می‌شوند را بازیابی کنیم.

        // گرفتن IDهای تمام منوهایی که با سرچ مطابقت دارند (هم اصلی هم زیرمنو)
        var matchingMenuIds = await baseQuery.Select(m => m.Id).ToListAsync();

        // حالا کوئری اصلی رو برای گرفتن منوهای ریشه (ParentMenuId == null) آماده می‌کنیم
        // و مطمئن میشیم که یا خود منوی ریشه با سرچ مطابقت داره یا یکی از زیرمنوهاش (به هر عمقی)
        var finalQuery = _context.Menus
            .Where(m => m.ParentMenuId == null && (
                matchingMenuIds.Contains(m.Id) || // خود منوی ریشه با سرچ مطابقت دارد
                _context.Menus.Any(sub => sub.ParentMenuId == m.Id && matchingMenuIds.Contains(sub.Id)) // یکی از فرزندان مستقیم آن مطابقت دارد
                // برای جستجوی عمیق‌تر در زیرمنوها به صورت Recursive باید از CTE یا روش‌های دیگر استفاده کرد.
                // در حال حاضر این جستجو یک سطح زیرمنو را پوشش می‌دهد.
            ))
            .AsQueryable();

        // فیلتر بر اساس وضعیت فعال/غیرفعال (مجدداً برای منوهای اصلی)
        if (isActive.HasValue)
            finalQuery = finalQuery.Where(m => m.IsActive == isActive.Value);

        // تعداد کل نتایج اصلی
        var totalCount = await finalQuery.CountAsync();

        // صفحه‌بندی و لود زیرمنوها
        if (includeSubMenus)
        {
            // این Include ها برای لود کردن تمام زیرمنوهاست تا ساختار درختی کامل بشه
            finalQuery = finalQuery
                .Include(m => m.SubMenus)
                .ThenInclude(sm => sm.SubMenus) // اگر زیرمنوها هم زیرمنو دارند
                .ThenInclude(ssm => ssm.SubMenus); // و باز هم زیرمنو دارند (این قسمت رو بسته به عمق نیازت اضافه کن)
        }

        var menus = await finalQuery
            .OrderBy(m => m.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            menus = menus.Select(MapToDto).ToList()
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<MenuDto>> GetMenu(Guid id)
    {
        var menu = await _context.Menus
            .Include(m => m.SubMenus)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu == null)
            return NotFound(new { message = "منو یافت نشد" });

        return Ok(MapToDto(menu));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MenuDto>> CreateMenu(CreateMenuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "اطلاعات وارد شده معتبر نیست", errors = ModelState });

        if (dto.ParentMenuId.HasValue)
        {
            var parentExists = await _context.Menus.AnyAsync(m => m.Id == dto.ParentMenuId.Value);
            if (!parentExists)
                return NotFound(new { success = false, message = "منوی والد یافت نشد" });
        }

        var menu = new Menu
        {
            Name = dto.Name,
            Title = dto.Title,
            Icon = dto.Icon,
            Url = dto.Url,
            DisplayOrder = dto.DisplayOrder,
            ParentMenuId = dto.ParentMenuId,
            IsActive = dto.IsActive
        };

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMenu), new { id = menu.Id }, MapToDto(menu));
    }

    [HttpPut("UpdateMenu/{id}")]
    [Authorize]
    public async Task<ActionResult<MenuDto>> UpdateMenu(Guid id, UpdateMenuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "اطلاعات وارد شده معتبر نیست", errors = ModelState });

        var menu = await _context.Menus.FindAsync(id);
        if (menu == null)
            return NotFound(new { success = false, message = "منو یافت نشد" });

        if (dto.ParentMenuId.HasValue)
        {
            var parentExists = await _context.Menus.AnyAsync(m => m.Id == dto.ParentMenuId.Value);
            if (!parentExists)
                return NotFound(new { success = false, message = "منوی والد یافت نشد" });
        }

        if (dto.Name != null) menu.Name = dto.Name;
        if (dto.Title != null) menu.Title = dto.Title;
        if (dto.Icon != null) menu.Icon = dto.Icon;
        if (dto.Url != null) menu.Url = dto.Url;
        if (dto.DisplayOrder.HasValue) menu.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.ParentMenuId.HasValue) menu.ParentMenuId = dto.ParentMenuId;
        if (dto.IsActive.HasValue) menu.IsActive = dto.IsActive.Value;
        await _context.SaveChangesAsync();

        return Ok(MapToDto(menu));
    }
    [HttpDelete("DeleteMenu/{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteMenu(Guid id)
    {
        var menu = await _context.Menus
            .Include(m => m.SubMenus)
            .Include(m => m.RoleMenus)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (menu == null)
            return NotFound(new { success = false, message = "منو یافت نشد" });

       if (menu.SubMenus?.Any() == true)
    return BadRequest(new { success = false, message = "ابتدا زیرمنوها را حذف کنید" });

if (menu.RoleMenus?.Any() == true)
    return BadRequest(new { success = false, message = "این منو به نقش‌ما اختصاص داده شده و قابل حذف نیست" });

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "منو با موفقیت حذف شد" });
    }

    private static MenuDto MapToDto(Menu menu)
    {
        return new MenuDto
        {
            Id = menu.Id,
            Name = menu.Name,
            Title = menu.Title,
            Icon = menu.Icon,
            Url = menu.Url,
            DisplayOrder = menu.DisplayOrder,
            ParentMenuId = menu.ParentMenuId,
            IsActive = menu.IsActive,
            SubMenus = menu.SubMenus?.Select(MapToDto).ToList()
        };
    }
}
