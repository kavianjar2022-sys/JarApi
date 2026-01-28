using System.ComponentModel.DataAnnotations;

namespace JarApi.Models;

public enum ShiftType
{
    Fixed = 0,
    Rotational = 1
}

public class ShiftDefinition
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ShiftType Type { get; set; }

    // Fixed shift fields
    public TimeSpan? FixedStartTime { get; set; }
    public TimeSpan? FixedEndTime { get; set; }
    public int? FixedBreakMinutes { get; set; }
    // Bitmask for working days (1=Monday ... 7=Sunday)
    public int? WorkingDaysMask { get; set; }

    public ICollection<ShiftRotationSegment> RotationSegments { get; set; } = new List<ShiftRotationSegment>();
}

public class ShiftRotationSegment
{
    public Guid Id { get; set; }
    public Guid ShiftDefinitionId { get; set; }
    public ShiftDefinition ShiftDefinition { get; set; } = null!;

    [MaxLength(50)]
    public string Label { get; set; } = string.Empty; // e.g., A, B, C, Off

    public int DurationDays { get; set; } // number of consecutive days

    public bool IsOff { get; set; }

    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

public class ShiftAssignment
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public Guid ShiftDefinitionId { get; set; }
    public ShiftDefinition ShiftDefinition { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // 0-based index of starting segment in rotation (for rotational types)
    public int RotationStartIndex { get; set; }
}
