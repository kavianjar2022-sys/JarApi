using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobPositionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public JobPositionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<object>> GetJobPositions(
        [FromQuery] string? search = null,
        [FromQuery] int? level = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.JobPositions
            .Include(j => j.ParentPosition)
            .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(searchTerm) ||
                (j.Code != null && j.Code.ToLower().Contains(searchTerm)) ||
                (j.Description != null && j.Description.ToLower().Contains(searchTerm)) ||
                j.Level.ToString().Contains(searchTerm) ||
                j.IsActive.ToString().ToLower().Contains(searchTerm) ||
                (j.ParentPosition != null && j.ParentPosition.Title.ToLower().Contains(searchTerm)) || // جستجو در عنوان سمت والد
                // 💡 اضافه شدن جستجو در عنوان والدِ والد
                (j.ParentPosition != null && j.ParentPosition.ParentPosition != null && j.ParentPosition.ParentPosition.Title.ToLower().Contains(searchTerm))
            );
        }

        if (level.HasValue)
        {
            query = query.Where(j => j.Level == level.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(j => j.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(j => j.Level)
            .ThenBy(j => j.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobPositionDto
            {
                Id = j.Id,
                Title = j.Title,
                Code = j.Code,
                Description = j.Description,
                Level = j.Level,
                ParentPositionId = j.ParentPositionId,
                ParentPositionTitle = j.ParentPosition != null ? j.ParentPosition.Title : null,
                IsActive = j.IsActive
            })
            .ToListAsync();

        return Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> GetJobPosition(Guid id)
    {
        var position = await _context.JobPositions
            .Include(j => j.ParentPosition)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (position == null)
            return NotFound(new { message = "سمت شغلی یافت نشد" });

        return Ok(new JobPositionDto
        {
            Id = position.Id,
            Title = position.Title,
            Code = position.Code,
            Description = position.Description,
            Level = position.Level,
            ParentPositionId = position.ParentPositionId,
            ParentPositionTitle = position.ParentPosition?.Title,
            IsActive = position.IsActive
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> CreateJobPosition(CreateJobPositionDto dto)
    {
        // بررسی تکراری بودن عنوان
        if (await _context.JobPositions.AnyAsync(j => j.Title == dto.Title))
            return BadRequest(new { message = "سمت شغلی با این عنوان قبلاً ثبت شده است" });

        // بررسی تکراری بودن کد
        if (!string.IsNullOrEmpty(dto.Code) && await _context.JobPositions.AnyAsync(j => j.Code == dto.Code))
            return BadRequest(new { message = "کد سمت شغلی تکراری است" });

        // بررسی وجود سمت والد
        if (dto.ParentPositionId.HasValue)
        {
            var parent = await _context.JobPositions.FindAsync(dto.ParentPositionId.Value);
            if (parent == null)
                return BadRequest(new { message = "سمت والد یافت نشد" });

            // سطح باید بزرگتر از سطح والد باشد
            if (dto.Level <= parent.Level)
                return BadRequest(new { message = "سطح سمت باید بزرگتر از سطح سمت والد باشد" });
        }

        var position = new JobPosition
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Code = dto.Code,
            Description = dto.Description,
            Level = dto.Level,
            ParentPositionId = dto.ParentPositionId,
            IsActive = dto.IsActive
        };

        _context.JobPositions.Add(position);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJobPosition), new { id = position.Id }, new JobPositionDto
        {
            Id = position.Id,
            Title = position.Title,
            Code = position.Code,
            Description = position.Description,
            Level = position.Level,
            ParentPositionId = position.ParentPositionId,
            IsActive = position.IsActive
        });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<JobPositionDto>> UpdateJobPosition(Guid id, UpdateJobPositionDto dto)
    {
        var position = await _context.JobPositions.FindAsync(id);
        if (position == null)
            return NotFound(new { message = "سمت شغلی یافت نشد" });

        // بررسی تکراری بودن عنوان
        if (await _context.JobPositions.AnyAsync(j => j.Title == dto.Title && j.Id != id))
            return BadRequest(new { message = "سمت شغلی با این عنوان قبلاً ثبت شده است" });

        // بررسی تکراری بودن کد
        if (!string.IsNullOrEmpty(dto.Code) && await _context.JobPositions.AnyAsync(j => j.Code == dto.Code && j.Id != id))
            return BadRequest(new { message = "کد سمت شغلی تکراری است" });

        // بررسی وجود سمت والد
        if (dto.ParentPositionId.HasValue)
        {
            if (dto.ParentPositionId.Value == id)
                return BadRequest(new { message = "سمت نمی‌تواند والد خودش باشد" });

            var parent = await _context.JobPositions.FindAsync(dto.ParentPositionId.Value);
            if (parent == null)
                return BadRequest(new { message = "سمت والد یافت نشد" });

            if (dto.Level <= parent.Level)
                return BadRequest(new { message = "سطح سمت باید بزرگتر از سطح سمت والد باشد" });

            // جلوگیری از ایجاد حلقه (circular reference)
            if (await IsDescendant(dto.ParentPositionId.Value, id))
                return BadRequest(new { message = "نمی‌توان سمت را والد زیرمجموعه خودش قرار داد" });
        }

        position.Title = dto.Title;
        position.Code = dto.Code;
        position.Description = dto.Description;
        position.Level = dto.Level;
        position.ParentPositionId = dto.ParentPositionId;
        position.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new JobPositionDto
        {
            Id = position.Id,
            Title = position.Title,
            Code = position.Code,
            Description = position.Description,
            Level = position.Level,
            ParentPositionId = position.ParentPositionId,
            IsActive = position.IsActive
        });
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteJobPosition(Guid id)
    {
        var position = await _context.JobPositions
            .Include(j => j.Users)
            .Include(j => j.SubPositions)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (position == null)
            return NotFound(new { message = "سمت شغلی یافت نشد" });

        if (position.Users.Any())
            return BadRequest(new { message = "این سمت به کاربرانی اختصاص داده شده است و قابل حذف نیست" });

        if (position.SubPositions.Any())
            return BadRequest(new { message = "این سمت دارای زیرمجموعه است و قابل حذف نیست" });

        _context.JobPositions.Remove(position);
        await _context.SaveChangesAsync();

        return Ok(new { message = "سمت شغلی با موفقیت حذف شد" });
    }

    [HttpGet("hierarchy")]
    [Authorize]
    public async Task<ActionResult<List<JobPositionHierarchyDto>>> GetHierarchy()
    {
        var positions = await _context.JobPositions
            .Include(j => j.SubPositions)
            .Include(j => j.Users)
            .Where(j => j.IsActive)
            .ToListAsync();

        var rootPositions = positions.Where(j => j.ParentPositionId == null).ToList();

        var hierarchy = rootPositions.Select(p => BuildHierarchy(p, positions)).ToList();

        return Ok(hierarchy);
    }

    private JobPositionHierarchyDto BuildHierarchy(JobPosition position, List<JobPosition> allPositions)
    {
        var dto = new JobPositionHierarchyDto
        {
            Id = position.Id,
            Title = position.Title,
            Code = position.Code,
            Level = position.Level,
            IsActive = position.IsActive,
            EmployeeCount = position.Users.Count,
            SubPositions = new List<JobPositionHierarchyDto>()
        };

        var children = allPositions.Where(p => p.ParentPositionId == position.Id).ToList();
        foreach (var child in children)
        {
            dto.SubPositions.Add(BuildHierarchy(child, allPositions));
        }

        return dto;
    }

    private async Task<bool> IsDescendant(Guid ancestorId, Guid descendantId)
    {
        var current = await _context.JobPositions.FindAsync(ancestorId);
        while (current != null && current.ParentPositionId.HasValue)
        {
            if (current.ParentPositionId.Value == descendantId)
                return true;
            current = await _context.JobPositions.FindAsync(current.ParentPositionId.Value);
        }
        return false;
    }
}
