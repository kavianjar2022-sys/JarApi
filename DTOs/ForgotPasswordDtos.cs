namespace JarApi.DTOs;

public class ForgotPasswordDto
{
    public string PersonnelCode { get; set; } = string.Empty;
    // توجه: در این سیستم "InsuranceCode" معادل کد ملی است
    public string InsuranceCode { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
}

public class ForgotPasswordVerifyResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; } // For next step (reset)
}

public class ResetPasswordDto
{
    public string UserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
