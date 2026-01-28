using System.ComponentModel.DataAnnotations;
using JarApi.Models;

namespace JarApi.DTOs;

public class CreateRotationSegmentDto
{
    [Required]
    public string Label { get; set; } = string.Empty;
    [Range(1, 365)]
    public int DurationDays { get; set; }
    public bool IsOff { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class CreateShiftDefinitionDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public ShiftType Type { get; set; }

    // Fixed
    public TimeSpan? FixedStartTime { get; set; }
    public TimeSpan? FixedEndTime { get; set; }
    public int? FixedBreakMinutes { get; set; }
    public int? WorkingDaysMask { get; set; }

    // Rotational
 public List<CreateRotationSegmentDto>? Segments { get; set; } 
 }

public class UpdateShiftDefinitionDto : CreateShiftDefinitionDto { }

public class AssignShiftDto
{
    public string? UserId { get; set; }
    public string? PersonnelCode { get; set; }
    [Required]
    public Guid ShiftDefinitionId { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int RotationStartIndex { get; set; } = 0;
}

public class ScheduleQueryDto
{
    public string? UserId { get; set; }
    public string? PersonnelCode { get; set; }
    [Required]
    public DateTime From { get; set; }
    [Required]
    public DateTime To { get; set; }
}

public class ShiftInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ShiftType Type { get; set; }
    public TimeSpan? FixedStartTime { get; set; }
    public TimeSpan? FixedEndTime { get; set; }
    public int? FixedBreakMinutes { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
}

public class RotationSegmentForListDto // اسمش رو گذاشتم ForListDto که بدونی مال نمایش توی لیست شیفت‌هاست
{
    public Guid Id { get; set; }
    public Guid ShiftDefinitionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public bool IsOff { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // دقت کن! اینجا دیگه پراپرتی public ShiftDefinition ShiftDefinition { get; set; } رو نداری
    // همین باعث میشه لوپ از بین بره
}
public class ShiftDefinitionListDto // اسمش رو گذاشتم ListDto که با بقیه قاطی نشه
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ShiftType Type { get; set; }
    public TimeSpan? FixedStartTime { get; set; }
    public TimeSpan? FixedEndTime { get; set; }
    public int? FixedBreakMinutes { get; set; }
    public int? WorkingDaysMask { get; set; } // اینو هم اضافه کردم چون توی مدل اصلیت بود

    // اینجا لیستی از DTO برای RotationSegments قرار میگیره
    public List<RotationSegmentForListDto> RotationSegments { get; set; } = new List<RotationSegmentForListDto>();
}