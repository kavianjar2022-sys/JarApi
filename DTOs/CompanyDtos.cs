using System.ComponentModel.DataAnnotations;

namespace JarApi.DTOs;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCompanyDto
{
    [Required(ErrorMessage = "نام شرکت الزامی است")]
    [MaxLength(200, ErrorMessage = "نام شرکت نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "کد شرکت نباید بیشتر از ۵۰ کاراکتر باشد")]
    public string? Code { get; set; }

    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCompanyDto
{
    [MaxLength(200, ErrorMessage = "نام شرکت نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string? Name { get; set; }

    [MaxLength(50, ErrorMessage = "کد شرکت نباید بیشتر از ۵۰ کاراکتر باشد")]
    public string? Code { get; set; }

    public string? Address { get; set; }
    public bool? IsActive { get; set; }
}
