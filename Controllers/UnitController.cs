using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UnitController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UnitController(ApplicationDbContext context)
    {
        _context = context;
    }

[HttpGet]
[Authorize]
public async Task<ActionResult<object>> GetAllUnits(
    [FromQuery] Guid? companyId = null,
    [FromQuery] string? search = null,
    [FromQuery] string? code = null, // این پارامتر رو میتونی نگه داری، یا اگه جستجوی کلی کد رو پوشش میده حذف کنی
    [FromQuery] bool? isActive = null,
    [FromQuery] bool includeSubUnits = true,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50)
{
    // 💡 قدم اول: query رو روی تمام واحدها (نه فقط ParentUnitId == null) شروع می‌کنیم
    var baseQuery = _context.Units
        .Include(u => u.Company) // برای دسترسی به CompanyName
        .Include(u => u.ParentUnit) // برای دسترسی به ParentUnitName
        .AsQueryable();

    // فیلتر بر اساس شرکت (قبل از جستجو اعمال شود)
    if (companyId.HasValue)
        baseQuery = baseQuery.Where(u => u.CompanyId == companyId.Value);

    // 💡 تغییرات جستجو در اینجا! (جستجو روی تمام فیلدهای رشته‌ای و قابل تبدیل به رشته، هم در واحدها و هم در زیرواحدها)
    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchTerm = search.Trim().ToLower();
        baseQuery = baseQuery.Where(u =>
            u.Name.ToLower().Contains(searchTerm) ||
            (u.Code != null && u.Code.ToLower().Contains(searchTerm)) ||
            u.IsActive.ToString().ToLower().Contains(searchTerm) ||
            (u.Company != null && u.Company.Name.ToLower().Contains(searchTerm)) || // جستجو در نام شرکت
            (u.ParentUnit != null && u.ParentUnit.Name.ToLower().Contains(searchTerm)) || // جستجو در نام واحد والد
            // جستجو در زیرواحدها (برای هر زیرواحد هم همین فیلدها رو چک می‌کنیم)
            u.SubUnits.Any(su =>
                su.Name.ToLower().Contains(searchTerm) ||
                (su.Code != null && su.Code.ToLower().Contains(searchTerm)) ||
                su.IsActive.ToString().ToLower().Contains(searchTerm)
                // نیازی نیست نام شرکت و والد را در زیرواحدها چک کنیم، چون اگر زیرواحد پیدا شود، والدش لود می‌شود و اطلاعات شرکت از طریق آن قابل دسترسی است.
                // اگر زیرواحدها هم خودشان SubUnits دارند، می‌توانید این الگو را تکرار کنید (مانند مورد Menu).
            )
        );
    }

    // فیلتر بر اساس کد (این فیلتر رو اگه دوست داری میتونی نگه داری، چون جستجوی کلی کد رو پوشش میده)
    if (!string.IsNullOrWhiteSpace(code))
        baseQuery = baseQuery.Where(u => u.Code != null && u.Code.Contains(code.Trim()));

    // فیلتر بر اساس وضعیت فعال/غیرفعال (برای واحدهایی که نتیجه جستجو هستن)
    if (isActive.HasValue)
        baseQuery = baseQuery.Where(u => u.IsActive == isActive.Value);

    // گرفتن IDهای تمام واحدهایی که با سرچ مطابقت دارند (هم اصلی هم زیرواحد)
    var matchingUnitIds = await baseQuery.Select(u => u.Id).ToListAsync();

    // حالا کوئری اصلی رو برای گرفتن واحدهای ریشه (ParentUnitId == null) آماده می‌کنیم
    // و مطمئن میشیم که یا خود واحد ریشه با سرچ مطابقت داره یا یکی از زیرواحدهایش (به هر عمقی)
    var finalQuery = _context.Units
        .Where(u => u.ParentUnitId == null && (
            matchingUnitIds.Contains(u.Id) || // خود واحد ریشه با سرچ مطابقت دارد
            _context.Units.Any(sub => sub.ParentUnitId == u.Id && matchingUnitIds.Contains(sub.Id)) // یکی از فرزندان مستقیم آن مطابقت دارد
            // برای جستجوی عمیق‌تر در زیرواحدها به صورت Recursive باید از CTE یا روش‌های دیگر استفاده کرد.
            // در حال حاضر این جستجو یک سطح زیرواحد را پوشش می‌دهد.
        ))
        .AsQueryable();

    // فیلتر بر اساس وضعیت فعال/غیرفعال (مجدداً برای واحدهای اصلی)
    if (isActive.HasValue)
        finalQuery = finalQuery.Where(u => u.IsActive == isActive.Value);

    // تعداد کل نتایج اصلی
    var totalCount = await finalQuery.CountAsync();

    // صفحه‌بندی و لود زیرواحدها
    if (includeSubUnits)
    {
        // این Include ها برای لود کردن تمام زیرواحدهاست تا ساختار درختی کامل بشه
        finalQuery = finalQuery
            .Include(u => u.Company) // برای MapToDto
            .Include(u => u.ParentUnit) // برای MapToDto
            .Include(u => u.SubUnits)
            .ThenInclude(su => su.SubUnits) // اگر زیرواحدها هم زیرواحد دارند
            .ThenInclude(ssu => ssu.SubUnits); // و باز هم زیرواحد دارند (این قسمت رو بسته به عمق نیازت اضافه کن)
    }

    var units = await finalQuery
        .OrderBy(u => u.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Ok(new
    {
        totalCount,
        page,
        pageSize,
        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        units = units.Select(MapToDto).ToList()
    });
}

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UnitDto>> GetUnit(Guid id)
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
    [Authorize]
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
    [Authorize]
    public async Task<ActionResult<UnitDto>> UpdateUnit(Guid id, UpdateUnitDto dto)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
            return NotFound(new { success = false, message = "واحد یافت نشد" });

        if (dto.Name != null) unit.Name = dto.Name;
        if (dto.Code != null) unit.Code = dto.Code;
        
        if (dto.CompanyId.HasValue && dto.CompanyId.Value != unit.CompanyId)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId.Value);
            if (company == null)
                return NotFound(new { success = false, message = "شرکت جدید یافت نشد" });
            unit.CompanyId = dto.CompanyId.Value;
            // اگر شرکت تغییر کرد، ParentUnitId باید null شود
            unit.ParentUnitId = null;
        }
        
        if (dto.ParentUnitId.HasValue)
        {
            var parentUnit = await _context.Units.FindAsync(dto.ParentUnitId.Value);
            if (parentUnit == null)
                return NotFound(new { success = false, message = "واحد والد یافت نشد" });
            if (parentUnit.CompanyId != unit.CompanyId)
                return BadRequest(new { success = false, message = "واحد والد باید در همان شرکت باشد" });
            unit.ParentUnitId = dto.ParentUnitId.Value;
        }
        
        if (dto.IsActive.HasValue) unit.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        var updated = await _context.Units
            .Include(u => u.Company)
            .Include(u => u.ParentUnit)
            .FirstAsync(u => u.Id == id);

        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteUnit(Guid id)
    {
        var unit = await _context.Units
            .Include(u => u.SubUnits)
            .Include(u => u.UserRoleUnits)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (unit == null)
            return NotFound(new { success = false, message = "واحد یافت نشد" });

        if (unit.SubUnits.Any())
            return BadRequest(new { success = false, message = "ابتدا زیرواحدها را حذف کنید" });

        if (unit.UserRoleUnits.Any())
            return BadRequest(new { success = false, message = "این واحد دارای کاربران مرتبط است و قابل حذف نیست" });

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "واحد با موفقیت حذف شد" });
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
