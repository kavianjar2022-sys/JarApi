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
public class MenuController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<MenuDto>>> GetAllMenus()
    {
        var menus = await _context.Menus
            .Where(m => m.ParentMenuId == null) // فقط منوهای اصلی
            .Include(m => m.SubMenus)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        return Ok(menus.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MenuDto>> GetMenu(int id)
    {
        var menu = await _context.Menus
            .Include(m => m.SubMenus)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu == null)
            return NotFound(new { message = "منو یافت نشد" });

        return Ok(MapToDto(menu));
    }

    [HttpPost]
    public async Task<ActionResult<MenuDto>> CreateMenu(CreateMenuDto dto)
    {
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

    [HttpPut("{id}")]
    public async Task<ActionResult<MenuDto>> UpdateMenu(int id, UpdateMenuDto dto)
    {
        var menu = await _context.Menus.FindAsync(id);
        if (menu == null)
            return NotFound(new { message = "منو یافت نشد" });

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

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMenu(int id)
    {
        var menu = await _context.Menus.FindAsync(id);
        if (menu == null)
            return NotFound(new { message = "منو یافت نشد" });

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync();

        return Ok(new { message = "منو با موفقیت حذف شد" });
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
