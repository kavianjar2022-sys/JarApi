using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CompanyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CompanyController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// دریافت لیست تمام شرکت‌ها با فیلتر و صفحه‌بندی
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? code = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Companies.AsQueryable();

        // جستجو در نام و آدرس
          if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                (c.Address != null && c.Address.ToLower().Contains(searchTerm)) ||
                (c.Code != null && c.Code.ToLower().Contains(searchTerm)) // اضافه شدن جستجو در Code
            );
        }

        // فیلتر بر اساس کد
        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(c => c.Code != null && c.Code.Contains(code.Trim()));

        // فیلتر بر اساس وضعیت فعال/غیرفعال
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        // تعداد کل نتایج
        var totalCount = await query.CountAsync();

        // صفحه‌بندی
        var companies = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Address = c.Address,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            companies
        });
    }

    /// <summary>
    /// دریافت اطلاعات یک شرکت
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CompanyDto>> Get(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
        {
            return NotFound(new { success = false, message = "شرکت یافت نشد" });
        }

        var dto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Code = company.Code,
            Address = company.Address,
            IsActive = company.IsActive
        };

        return Ok(dto);
    }

    /// <summary>
    /// ایجاد شرکت جدید
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "اطلاعات وارد شده معتبر نیست", errors = ModelState });
        }

        var company = new Company
        {
            Name = dto.Name,
            Code = dto.Code,
            Address = dto.Address,
            IsActive = dto.IsActive
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        var resultDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Code = company.Code,
            Address = company.Address,
            IsActive = company.IsActive
        };

        return CreatedAtAction(nameof(Get), new { id = company.Id }, resultDto);
    }

    /// <summary>
    /// ویرایش شرکت
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "اطلاعات وارد شده معتبر نیست", errors = ModelState });
        }

        var company = await _context.Companies.FindAsync(id);
        if (company == null)
        {
            return NotFound(new { success = false, message = "شرکت یافت نشد" });
        }

        // به‌روزرسانی فقط فیلدهایی که مقدار دارند
        if (dto.Name != null) company.Name = dto.Name;
        if (dto.Code != null) company.Code = dto.Code;
        if (dto.Address != null) company.Address = dto.Address;
        if (dto.IsActive.HasValue) company.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        var resultDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Code = company.Code,
            Address = company.Address,
            IsActive = company.IsActive
        };

        return Ok(resultDto);
    }

    /// <summary>
    /// حذف شرکت
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var company = await _context.Companies
            .Include(c => c.Units)
            .Include(c => c.UserRoleCompanies)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return NotFound(new { success = false, message = "شرکت یافت نشد" });
        }

        // بررسی وجود واحدها یا کاربران مرتبط
        if (company.Units.Any())
        {
            return BadRequest(new { success = false, message = "شرکت دارای واحدهای فعال است و قابل حذف نیست" });
        }

        if (company.UserRoleCompanies.Any())
        {
            return BadRequest(new { success = false, message = "شرکت دارای کاربران مرتبط است و قابل حذف نیست" });
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "شرکت با موفقیت حذف شد" });
    }
}
