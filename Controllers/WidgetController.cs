using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WidgetController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WidgetController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<object>> GetAllWidgets(
        [FromQuery] string? search = null,
        [FromQuery] string? widgetType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.Widgets.AsQueryable();

        // جستجو در نام، عنوان و توضیحات
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(searchTerm) ||
                w.Title.ToLower().Contains(searchTerm) ||
                (w.Description != null && w.Description.ToLower().Contains(searchTerm))
            );
        }

        // فیلتر بر اساس نوع ویجت
        if (!string.IsNullOrWhiteSpace(widgetType))
            query = query.Where(w => w.WidgetType.ToLower().Contains(widgetType.Trim().ToLower()));

        // فیلتر بر اساس وضعیت فعال/غیرفعال
        if (isActive.HasValue)
            query = query.Where(w => w.IsActive == isActive.Value);

        // تعداد کل نتایج
        var totalCount = await query.CountAsync();

        // صفحه‌بندی
        var widgets = await query
            .OrderBy(w => w.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            widgets = widgets.Select(MapToDto).ToList()
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<WidgetDto>> GetWidget(Guid id)
    {
        var widget = await _context.Widgets.FindAsync(id);

        if (widget == null)
            return NotFound(new { message = "ویجت یافت نشد" });

        return Ok(MapToDto(widget));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WidgetDto>> CreateWidget(CreateWidgetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "اطلاعات وارد شده معتبر نیست", errors = ModelState });

        var widget = new Widget
        {
            Name = dto.Name,
            Title = dto.Title,
            Description = dto.Description,
            Icon = dto.Icon,
            WidgetType = dto.WidgetType,
            Configuration = dto.Configuration,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.Widgets.Add(widget);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWidget), new { id = widget.Id }, MapToDto(widget));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<WidgetDto>> UpdateWidgetWidgetCon(Guid id, UpdateWidgetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "اطلاعات وارد شده معتبر نیست", errors = ModelState });

        var widget = await _context.Widgets.FindAsync(id);
        if (widget == null)
            return NotFound(new { success = false, message = "ویجت یافت نشد" });

        if (dto.Name != null) widget.Name = dto.Name;
        if (dto.Title != null) widget.Title = dto.Title;
        if (dto.Description != null) widget.Description = dto.Description;
        if (dto.Icon != null) widget.Icon = dto.Icon;
        if (dto.WidgetType != null) widget.WidgetType = dto.WidgetType;
        if (dto.Configuration != null) widget.Configuration = dto.Configuration;
        if (dto.DisplayOrder.HasValue) widget.DisplayOrder = dto.DisplayOrder.Value;
        if (dto.IsActive.HasValue) widget.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(widget));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteWidget(Guid id)
    {
        var widget = await _context.Widgets
            .Include(w => w.RoleWidgets)
            .FirstOrDefaultAsync(w => w.Id == id);
            
        if (widget == null)
            return NotFound(new { success = false, message = "ویجت یافت نشد" });

        if (widget.RoleWidgets.Any())
            return BadRequest(new { success = false, message = "این ویجت به نقش‌ها اختصاص داده شده و قابل حذف نیست" });

        _context.Widgets.Remove(widget);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "ویجت با موفقیت حذف شد" });
    }

    private static WidgetDto MapToDto(Widget widget)
    {
        return new WidgetDto
        {
            Id = widget.Id,
            Name = widget.Name,
            Title = widget.Title,
            Description = widget.Description,
            Icon = widget.Icon,
            WidgetType = widget.WidgetType,
            Configuration = widget.Configuration,
            DisplayOrder = widget.DisplayOrder,
            IsActive = widget.IsActive
        };
    }
}
