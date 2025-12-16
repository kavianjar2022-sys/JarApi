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
public class UnitController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UnitController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UnitDto>>> GetAllUnits([FromQuery] int? companyId = null)
    {
        var query = _context.Units
            .Include(u => u.Company)
            .Include(u => u.ParentUnit)
            .Include(u => u.SubUnits)
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(u => u.CompanyId == companyId.Value);

        var units = await query
            .Where(u => u.ParentUnitId == null) // فقط واحدهای اصلی
            .OrderBy(u => u.Name)
            .ToListAsync();

        return Ok(units.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UnitDto>> GetUnit(int id)
    {
        var unit = await _context.Units
            .Include(u => u.Company)
            .Include(u => u.ParentUnit)
            .Include(u => u.SubUnits)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (unit == null)
            return NotFound(new { message = "واحد یافت نشد" });

        return Ok(MapToDto(unit));
    }

    [HttpPost]
    public async Task<ActionResult<UnitDto>> CreateUnit(CreateUnitDto dto)
    {
        var company = await _context.Companies.FindAsync(dto.CompanyId);
        if (company == null)
            return NotFound(new { message = "شرکت یافت نشد" });

        if (dto.ParentUnitId.HasValue)
        {
            var parentUnit = await _context.Units.FindAsync(dto.ParentUnitId.Value);
            if (parentUnit == null)
                return NotFound(new { message = "واحد والد یافت نشد" });
            if (parentUnit.CompanyId != dto.CompanyId)
                return BadRequest(new { message = "واحد والد باید در همان شرکت باشد" });
        }

        var unit = new Unit
        {
            Name = dto.Name,
            Code = dto.Code,
            CompanyId = dto.CompanyId,
            ParentUnitId = dto.ParentUnitId,
            IsActive = dto.IsActive
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        var created = await _context.Units
            .Include(u => u.Company)
            .Include(u => u.ParentUnit)
            .FirstAsync(u => u.Id == unit.Id);

        return CreatedAtAction(nameof(GetUnit), new { id = unit.Id }, MapToDto(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UnitDto>> UpdateUnit(int id, UpdateUnitDto dto)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
            return NotFound(new { message = "واحد یافت نشد" });

        if (dto.Name != null) unit.Name = dto.Name;
        if (dto.Code != null) unit.Code = dto.Code;
        if (dto.CompanyId.HasValue) unit.CompanyId = dto.CompanyId.Value;
        if (dto.ParentUnitId.HasValue) unit.ParentUnitId = dto.ParentUnitId;
        if (dto.IsActive.HasValue) unit.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        var updated = await _context.Units
            .Include(u => u.Company)
            .Include(u => u.ParentUnit)
            .FirstAsync(u => u.Id == id);

        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUnit(int id)
    {
        var unit = await _context.Units
            .Include(u => u.SubUnits)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (unit == null)
            return NotFound(new { message = "واحد یافت نشد" });

        if (unit.SubUnits.Any())
            return BadRequest(new { message = "ابتدا زیرواحدها را حذف کنید" });

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();

        return Ok(new { message = "واحد با موفقیت حذف شد" });
    }

    private static UnitDto MapToDto(Unit unit)
    {
        return new UnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Code = unit.Code,
            CompanyId = unit.CompanyId,
            CompanyName = unit.Company?.Name,
            ParentUnitId = unit.ParentUnitId,
            ParentUnitName = unit.ParentUnit?.Name,
            IsActive = unit.IsActive,
            SubUnits = unit.SubUnits?.Select(MapToDto).ToList()
        };
    }
}
