using System.ComponentModel.DataAnnotations;
using JarApi.Models;

namespace JarApi.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام الزامی است")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    public string LastName { get; set; } = string.Empty;

    // جنسیت
    public Gender Gender { get; set; } = Gender.Male;

    // مدرک تحصیلی (اختیاری)
    public Guid? EducationDegreeId { get; set; }

    // فیلدهای اختیاری
    public string? FaceCode { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public string? MobileNumber { get; set; }
    public string? InsuranceCode { get; set; }
    public string? HomePhoneNumber { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است")]
    public string PersonnelCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public UserInfoDto? UserInfo { get; set; }
}

public class UserInfoDto
{
    public string UserId { get; set; } = string.Empty;
    public string PersonnelCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public Guid? EducationDegreeId { get; set; }
    public string? EducationDegreeName { get; set; }
    public string? FaceCode { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public string? MobileNumber { get; set; }
    public string? InsuranceCode { get; set; }
    public string? HomePhoneNumber { get; set; }
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "نام الزامی است")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است")]
    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public Guid? EducationDegreeId { get; set; }
    public string? FaceCode { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public string? MobileNumber { get; set; }
    public string? InsuranceCode { get; set; }
    public string? HomePhoneNumber { get; set; }
}
