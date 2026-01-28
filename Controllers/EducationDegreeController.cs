using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class EducationDegreeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EducationDegreeController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// دریافت لیست تمام مدارک تحصیلی
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EducationDegreeDto>>> GetEducationDegrees(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null)
    {
        var query = _context.EducationDegrees.AsQueryable();

        // فیلتر بر اساس وضعیت فعال/غیرفعال
        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);

        // جستجو در نام
         if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(searchTerm) ||
                e.IsActive.ToString().ToLower().Contains(searchTerm) // جستجو در وضعیت فعال/غیرفعال
                // اگه فیلد رشته‌ای یا قابل تبدیل به رشته دیگه‌ای داری، اینجا اضافه کن
            );
        }

        var degrees = await query
            .OrderBy(e => e.Name)
            .Select(e => new EducationDegreeDto
            {
                Id = e.Id,
                Name = e.Name,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(degrees);
    }

    /// <summary>
    /// دریافت یک مدرک تحصیلی بر اساس ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<EducationDegreeDto>> GetEducationDegree(Guid id)
    {
        var degree = await _context.EducationDegrees.FindAsync(id);

        if (degree == null)
            return NotFound(new { message = "مدرک تحصیلی یافت نشد" });

        return Ok(new EducationDegreeDto
        {
            Id = degree.Id,
            Name = degree.Name,
            IsActive = degree.IsActive
        });
    }

    /// <summary>
    /// ایجاد مدرک تحصیلی جدید
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<EducationDegreeDto>> CreateEducationDegree(CreateEducationDegreeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "اطلاعات وارد شده نامعتبر است", errors = ModelState });

        // بررسی تکراری نبودن نام
        var exists = await _context.EducationDegrees.AnyAsync(e => e.Name == dto.Name);
        if (exists)
            return BadRequest(new { success = false, message = "مدرک تحصیلی با این نام قبلاً ثبت شده است" });

        var degree = new EducationDegree
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            IsActive = dto.IsActive
        };

        _context.EducationDegrees.Add(degree);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEducationDegree),
            new { id = degree.Id },
            new EducationDegreeDto
            {
                Id = degree.Id,
                Name = degree.Name,
                IsActive = degree.IsActive
            });
    }

    /// <summary>
    /// به‌روزرسانی مدرک تحصیلی
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateEducationDegree(Guid id, UpdateEducationDegreeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "اطلاعات وارد شده نامعتبر است", errors = ModelState });

        var degree = await _context.EducationDegrees.FindAsync(id);
        if (degree == null)
            return NotFound(new { message = "مدرک تحصیلی یافت نشد" });

        // به‌روزرسانی فیلدها
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            // بررسی تکراری نبودن نام
            var exists = await _context.EducationDegrees.AnyAsync(e => e.Name == dto.Name && e.Id != id);
            if (exists)
                return BadRequest(new { success = false, message = "مدرک تحصیلی با این نام قبلاً ثبت شده است" });

            degree.Name = dto.Name;
        }

        if (dto.IsActive.HasValue)
            degree.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "مدرک تحصیلی با موفقیت به‌روزرسانی شد",
            degree = new EducationDegreeDto
            {
                Id = degree.Id,
                Name = degree.Name,
                IsActive = degree.IsActive
            }
        });
    }

    /// <summary>
    /// حذف مدرک تحصیلی
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEducationDegree(Guid id)
    {
        var degree = await _context.EducationDegrees.FindAsync(id);
        if (degree == null)
            return NotFound(new { message = "مدرک تحصیلی یافت نشد" });

        // بررسی اینکه آیا کاربری با این مدرک تحصیلی وجود دارد
        var hasUsers = await _context.Users.AnyAsync(u => u.EducationDegreeId == id);
        if (hasUsers)
            return BadRequest(new { success = false, message = "امکان حذف این مدرک تحصیلی وجود ندارد، چون کاربرانی با این مدرک وجود دارند. ابتدا مدرک کاربران را تغییر دهید" });

        _context.EducationDegrees.Remove(degree);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "مدرک تحصیلی با موفقیت حذف شد" });
    }
}
