using Microsoft.AspNetCore.Identity;

namespace JarApi.Models;

public class ApplicationUser : IdentityUser
{
    // کد پرسنلی - جایگزین Username/Email
    public string PersonnelCode { get; set; } = string.Empty;
    
    // کد فیس
    public string? FaceCode { get; set; }
    
    // تاریخ تولد
    public DateTime? BirthDate { get; set; }
    
    // تاریخ استخدام
    public DateTime? HireDate { get; set; }
    
    // نام
    public string FirstName { get; set; } = string.Empty;
    
    // نام خانوادگی
    public string LastName { get; set; } = string.Empty;
    
    // شماره تلفن همراه (از PhoneNumber ارثی می‌شود، اما این واضح‌تر است)
    public string? MobileNumber { get; set; }

    // کد ملی
    public string? NationalCode { get; set; }

    // کد بیمه
    public string? InsuranceCode { get; set; }

    // شماره تلفن خانه
    public string? HomePhoneNumber { get; set; }

    // جنسیت
    public Gender Gender { get; set; } = Gender.Male;

    // مدرک تحصیلی کاربر
    public Guid? EducationDegreeId { get; set; }
    public EducationDegree? EducationDegree { get; set; }

    // شیفت‌های کاربر
    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();

    // سمت شغلی کاربر
    public Guid? JobPositionId { get; set; }
    public JobPosition? JobPosition { get; set; }

    // مدیر مستقیم کاربر
    public string? ManagerId { get; set; }
    public ApplicationUser? Manager { get; set; }

    // زیردستان مستقیم
    public ICollection<ApplicationUser> DirectReports { get; set; } = new List<ApplicationUser>();
}
