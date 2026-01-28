using System.Globalization;
using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ShiftController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserResolver _userResolver;

    public ShiftController(ApplicationDbContext db)
    {
        _db = db;
        _userResolver = new UserResolver(db);
    }

    [HttpGet("definitions")]
[Authorize]
public async Task<IActionResult> GetDefinitions(
    [FromQuery] string? search = null,
    [FromQuery] ShiftType? type = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50)
{
    var query = _db.ShiftDefinitions
        .Include(s => s.RotationSegments)
        .AsQueryable();

// 💡 تغییرات جستجو در اینجا!
if (!string.IsNullOrWhiteSpace(search))
{
    var searchTerm = search.Trim().ToLower();
    query = query.Where(s =>
        s.Name.ToLower().Contains(searchTerm) ||
        (s.Description != null && s.Description.ToLower().Contains(searchTerm)) ||
        // اضافه شدن جستجو در فیلدهای دیگر
        (s.FixedStartTime != null && s.FixedStartTime.Value.ToString("hh\\:mm").Contains(searchTerm)) || // جستجو در زمان شروع ثابت
        (s.FixedEndTime != null && s.FixedEndTime.Value.ToString("hh\\:mm").Contains(searchTerm)) ||   // جستجو در زمان پایان ثابت
        (s.Type.ToString().ToLower().Contains(searchTerm)) || // جستجو در نوع شیفت (Fixed/Rotational)
        // این قسمت باید با || به بقیه وصل بشه ✅
        s.RotationSegments.Any(rs =>
            rs.Label.ToLower().Contains(searchTerm) ||
            (rs.StartTime != null && rs.StartTime.Value.ToString("hh\\:mm").Contains(searchTerm)) ||
            (rs.EndTime != null && rs.EndTime.Value.ToString("hh\\:mm").Contains(searchTerm))
        )
    );
}

    // فیلتر بر اساس نوع شیفت
    if (type.HasValue)
        query = query.Where(s => s.Type == type.Value);

    // تعداد کل نتایج
    var totalCount = await query.CountAsync();

    // صفحه‌بندی
    var list = await query
        .OrderBy(s => s.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    // ✨ اینجا لیست مدل اصلی رو به لیست DTO تبدیل میکنیم ✨
    var dtoList = list.Select(s => new ShiftDefinitionListDto
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        Type = s.Type,
        FixedStartTime = s.FixedStartTime,
        FixedEndTime = s.FixedEndTime,
        FixedBreakMinutes = s.FixedBreakMinutes,
        WorkingDaysMask = s.WorkingDaysMask,
        RotationSegments = s.RotationSegments.Select(rs => new RotationSegmentForListDto
        {
            Id = rs.Id,
            ShiftDefinitionId = rs.ShiftDefinitionId,
            Label = rs.Label,
            DurationDays = rs.DurationDays,
            IsOff = rs.IsOff,
            StartTime = rs.StartTime,
            EndTime = rs.EndTime
        }).ToList()
    }).ToList();

    return Ok(new
    {
        totalCount,
        page,
        pageSize,
        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        shifts = dtoList // حالا داریم لیست DTO رو برمیگردونیم که لوپ نداره
    });
}

    [HttpGet("definitions/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetDefinition(Guid id)
    {
        var item = await _db.ShiftDefinitions.Include(s => s.RotationSegments).FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound(new { success = false, message = "شیفت یافت نشد" });
        return Ok(item);
    }

    [HttpPost("definitions")]
    [Authorize]
    public async Task<IActionResult> CreateDefinition([FromBody] CreateShiftDefinitionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.Type == ShiftType.Fixed)
        {
            if (dto.FixedStartTime is null || dto.FixedEndTime is null)
                return BadRequest(new { success = false, message = "برای شیفت عادی، زمان شروع و پایان الزامی است" });
        }
        else
        {
            if (dto.Segments == null || dto.Segments.Count == 0)
                return BadRequest(new { success = false, message = "برای شیفت چرخشی، حداقل یک سگمنت لازم است" });
            if (dto.Segments.Any(s => !s.IsOff && (s.StartTime is null || s.EndTime is null)))
                return BadRequest(new { success = false, message = "برای سگمنت‌های کاری، زمان شروع/پایان الزامی است" });
        }

        var entity = new ShiftDefinition
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            FixedStartTime = dto.FixedStartTime,
            FixedEndTime = dto.FixedEndTime,
            FixedBreakMinutes = dto.FixedBreakMinutes,
            WorkingDaysMask = dto.WorkingDaysMask
        };

        if (dto.Type == ShiftType.Rotational && dto.Segments != null)
        {
            foreach (var seg in dto.Segments)
            {
                entity.RotationSegments.Add(new ShiftRotationSegment
                {
                    Label = seg.Label,
                    DurationDays = seg.DurationDays,
                    IsOff = seg.IsOff,
                    StartTime = seg.StartTime,
                    EndTime = seg.EndTime
                });
            }
        }

        _db.ShiftDefinitions.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(new { success = true, message = "شیفت ایجاد شد", id = entity.Id });
    }

    [HttpPut("definitions/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateDefinition(Guid id, [FromBody] UpdateShiftDefinitionDto dto)
    {
        var entity = await _db.ShiftDefinitions.Include(s => s.RotationSegments).FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound(new { success = false, message = "شیفت یافت نشد" });

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Type = dto.Type;
        entity.FixedStartTime = dto.FixedStartTime;
        entity.FixedEndTime = dto.FixedEndTime;
        entity.FixedBreakMinutes = dto.FixedBreakMinutes;
        entity.WorkingDaysMask = dto.WorkingDaysMask;

        if (dto.Type == ShiftType.Rotational)
        {
            // replace segments
            _db.ShiftRotationSegments.RemoveRange(entity.RotationSegments);
            entity.RotationSegments.Clear();
            if (dto.Segments != null)
            {
                foreach (var seg in dto.Segments)
                {
                    entity.RotationSegments.Add(new ShiftRotationSegment
                    {
                        Label = seg.Label,
                        DurationDays = seg.DurationDays,
                        IsOff = seg.IsOff,
                        StartTime = seg.StartTime,
                        EndTime = seg.EndTime
                    });
                }
            }
        }
        else
        {
            // remove any rotation segments if switching to fixed
            _db.ShiftRotationSegments.RemoveRange(entity.RotationSegments);
            entity.RotationSegments.Clear();
        }

        await _db.SaveChangesAsync();
        return Ok(new { success = true, message = "شیفت به‌روزرسانی شد" });
    }

    [HttpDelete("definitions/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteDefinition(Guid id)
    {
        var hasAssign = await _db.ShiftAssignments.AnyAsync(a => a.ShiftDefinitionId == id);
        if (hasAssign) return BadRequest(new { success = false, message = "به علت انتساب به کاربران، قابل حذف نیست" });

        var entity = await _db.ShiftDefinitions.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound(new { success = false, message = "شیفت یافت نشد" });
        _db.ShiftDefinitions.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(new { success = true, message = "حذف شد" });
    }

    [HttpPost("assignments")]
    [Authorize]
    public async Task<IActionResult> AssignShift([FromBody] AssignShiftDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = await _userResolver.ResolveUserAsync(dto.UserId, dto.PersonnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });
        var def = await _db.ShiftDefinitions.Include(s => s.RotationSegments).FirstOrDefaultAsync(s => s.Id == dto.ShiftDefinitionId);
        if (def == null) return NotFound(new { success = false, message = "شیفت یافت نشد" });

        var assign = new ShiftAssignment
        {
            UserId = user.Id,
            ShiftDefinitionId = def.Id,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate?.Date,
            RotationStartIndex = dto.RotationStartIndex
        };
        _db.ShiftAssignments.Add(assign);
        await _db.SaveChangesAsync();
        return Ok(new { success = true, message = "شیفت به کاربر انتساب یافت", id = assign.Id });
    }

    [HttpGet("assignments")]
        [Authorize]

    public async Task<IActionResult> ListAssignments([FromQuery] string? userId, [FromQuery] string? personnelCode)
    {
        var user = await _userResolver.ResolveUserAsync(userId, personnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });
        var list = await _db.ShiftAssignments
            .Include(a => a.ShiftDefinition)
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();
        return Ok(list);
    }

    [HttpDelete("assignments/{id:guid}")]
        [Authorize]

    public async Task<IActionResult> DeleteAssignment(Guid id)
    {
        var entity = await _db.ShiftAssignments.FirstOrDefaultAsync(a => a.Id == id);
        if (entity == null) return NotFound(new { success = false, message = "انتساب یافت نشد" });
        _db.ShiftAssignments.Remove(entity);
        await _db.SaveChangesAsync();
        return Ok(new { success = true, message = "انتساب حذف شد" });
    }

    [HttpGet("schedule")]
        [Authorize]

    public async Task<IActionResult> GetSchedule([FromQuery] ScheduleQueryDto dto)
    {
        if (dto.To.Date < dto.From.Date) return BadRequest(new { success = false, message = "بازه تاریخ نامعتبر است" });
        var user = await _userResolver.ResolveUserAsync(dto.UserId, dto.PersonnelCode);
        if (user == null) return NotFound(new { success = false, message = "کاربر یافت نشد" });

        var assignment = await _db.ShiftAssignments
            .Include(a => a.ShiftDefinition)
            .ThenInclude(s => s.RotationSegments)
            .Where(a => a.UserId == user.Id && a.StartDate <= dto.To && (a.EndDate == null || a.EndDate >= dto.From))
            .OrderByDescending(a => a.StartDate)
            .FirstOrDefaultAsync();

        if (assignment == null) return Ok(new { success = true, days = Array.Empty<object>() });

        var result = new List<object>();
        var def = assignment.ShiftDefinition;
        var day = dto.From.Date;
        while (day <= dto.To.Date)
        {
            if (def.Type == ShiftType.Fixed)
            {
                bool work = true;
                if (def.WorkingDaysMask.HasValue)
                {
                    var weekday = (int)(day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek);
                    work = ((def.WorkingDaysMask.Value >> (weekday - 1)) & 1) == 1;
                }
                result.Add(new
                {
                    date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    label = "Fixed",
                    isOff = !work,
                    startTime = work ? def.FixedStartTime?.ToString() : null,
                    endTime = work ? def.FixedEndTime?.ToString() : null
                });
            }
            else
            {
                var segs = def.RotationSegments.OrderBy(s => s.Id).ToList();
                if (segs.Count == 0)
                {
                    result.Add(new { date = day.ToString("yyyy-MM-dd"), label = "N/A", isOff = true, startTime = (string?)null, endTime = (string?)null });
                }
                else
                {
                    var daysFromStart = (int)(day - assignment.StartDate.Date).TotalDays;
                    if (daysFromStart < 0)
                    {
                        result.Add(new { date = day.ToString("yyyy-MM-dd"), label = "N/A", isOff = true, startTime = (string?)null, endTime = (string?)null });
                    }
                    else
                    {
                        var idx = assignment.RotationStartIndex % segs.Count;
                        if (idx < 0) idx += segs.Count;
                        int remaining = daysFromStart;
                        while (true)
                        {
                            var seg = segs[idx];
                            if (remaining < seg.DurationDays)
                            {
                                result.Add(new
                                {
                                    date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                    label = seg.Label,
                                    isOff = seg.IsOff,
                                    startTime = seg.IsOff ? null : seg.StartTime?.ToString(),
                                    endTime = seg.IsOff ? null : seg.EndTime?.ToString()
                                });
                                break;
                            }
                            remaining -= seg.DurationDays;
                            idx = (idx + 1) % segs.Count;
                        }
                    }
                }
            }
            day = day.AddDays(1);
        }

        return Ok(new { success = true, days = result });
    }

    [HttpGet("on-duty")]
        [Authorize]

    public async Task<IActionResult> GetOnDuty([FromQuery] string? dt)
    {
        var now = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(dt))
        {
            if (!DateTime.TryParse(dt, out now))
                return BadRequest(new { success = false, message = "فرمت تاریخ/زمان نامعتبر است" });
        }

        var date = now.Date;
        var time = now.TimeOfDay;

        var assigns = await _db.ShiftAssignments
            .Include(a => a.User)
            .Include(a => a.ShiftDefinition)
                .ThenInclude(s => s.RotationSegments)
            .Where(a => a.StartDate <= date && (a.EndDate == null || a.EndDate >= date))
            .ToListAsync();

        var results = new List<object>();

        foreach (var a in assigns)
        {
            var def = a.ShiftDefinition;
            bool onDuty = false;
            string label = "";
            TimeSpan? start = null;
            TimeSpan? end = null;

            if (def.Type == ShiftType.Fixed)
            {
                bool work = true;
                if (def.WorkingDaysMask.HasValue)
                {
                    var weekday = (int)(date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek);
                    work = ((def.WorkingDaysMask.Value >> (weekday - 1)) & 1) == 1;
                }
                if (work && def.FixedStartTime.HasValue && def.FixedEndTime.HasValue)
                {
                    start = def.FixedStartTime.Value;
                    end = def.FixedEndTime.Value;
                    label = "Fixed";
                    if (start <= end)
                        onDuty = time >= start && time < end;
                    else
                        onDuty = time >= start || time < end; // cross-midnight
                }
            }
            else
            {
                var segs = def.RotationSegments.OrderBy(s => s.Id).ToList();
                if (segs.Count > 0)
                {
                    var daysFromStart = (int)(date - a.StartDate.Date).TotalDays;
                    if (daysFromStart >= 0)
                    {
                        var idx = a.RotationStartIndex % segs.Count;
                        if (idx < 0) idx += segs.Count;
                        int remaining = daysFromStart;
                        while (true)
                        {
                            var seg = segs[idx];
                            if (remaining < seg.DurationDays)
                            {
                                label = seg.Label;
                                start = seg.StartTime;
                                end = seg.EndTime;
                                if (!seg.IsOff && start.HasValue && end.HasValue)
                                {
                                    if (start <= end)
                                        onDuty = time >= start && time < end;
                                    else
                                        onDuty = time >= start || time < end; // cross-midnight
                                }
                                break;
                            }
                            remaining -= seg.DurationDays;
                            idx = (idx + 1) % segs.Count;
                        }
                    }
                }
            }

            if (onDuty)
            {
                results.Add(new
                {
                    userId = a.UserId,
                    personnelCode = a.User.PersonnelCode,
                    firstName = a.User.FirstName,
                    lastName = a.User.LastName,
                    shiftName = def.Name,
                    label,
                    startTime = start?.ToString(),
                    endTime = end?.ToString()
                });
            }
        }

        return Ok(new { success = true, count = results.Count, items = results });
    }
}

internal class UserResolver
{
    private readonly ApplicationDbContext _db;
    public UserResolver(ApplicationDbContext db) { _db = db; }
    public async Task<ApplicationUser?> ResolveUserAsync(string? userId, string? personnelCode)
    {
        if (!string.IsNullOrWhiteSpace(userId))
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (!string.IsNullOrWhiteSpace(personnelCode))
            return await _db.Users.FirstOrDefaultAsync(u => u.PersonnelCode == personnelCode);
        return null;
    }
}
