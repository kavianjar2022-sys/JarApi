using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WidgetController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WidgetController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<WidgetDto>>> GetAllWidgets()
    {
        var widgets = await _context.Widgets
            .OrderBy(w => w.DisplayOrder)
            .ToListAsync();

        return Ok(widgets.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WidgetDto>> GetWidget(int id)
    {
        var widget = await _context.Widgets.FindAsync(id);

        if (widget == null)
            return NotFound(new { message = "ویجت یافت نشد" });

        return Ok(MapToDto(widget));
    }

    [HttpPost]
    public async Task<ActionResult<WidgetDto>> CreateWidget(CreateWidgetDto dto)
    {
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
    public async Task<ActionResult<WidgetDto>> UpdateWidget(int id, UpdateWidgetDto dto)
    {
        var widget = await _context.Widgets.FindAsync(id);
        if (widget == null)
            return NotFound(new { message = "ویجت یافت نشد" });

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
    public async Task<ActionResult> DeleteWidget(int id)
    {
        var widget = await _context.Widgets.FindAsync(id);
        if (widget == null)
            return NotFound(new { message = "ویجت یافت نشد" });

        _context.Widgets.Remove(widget);
        await _context.SaveChangesAsync();

        return Ok(new { message = "ویجت با موفقیت حذف شد" });
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
