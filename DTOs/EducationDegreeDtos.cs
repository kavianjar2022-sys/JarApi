using System.ComponentModel.DataAnnotations;

namespace JarApi.DTOs;

/// <summary>
/// DTO برای نمایش مدرک تحصیلی
/// </summary>
public class EducationDegreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO برای ایجاد مدرک تحصیلی جدید
/// </summary>
public class CreateEducationDegreeDto
{
    [Required(ErrorMessage = "نام مدرک تحصیلی الزامی است")]
    [MaxLength(200, ErrorMessage = "نام مدرک تحصیلی نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO برای به‌روزرسانی مدرک تحصیلی
/// </summary>
public class UpdateEducationDegreeDto
{
    [MaxLength(200, ErrorMessage = "نام مدرک تحصیلی نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string? Name { get; set; }
    
    public bool? IsActive { get; set; }
}
