using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponseDto { Success = false, Message = "اطلاعات وارد شده نامعتبر است" });

        var existing = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == model.PersonnelCode);
        if (existing != null)
            return BadRequest(new AuthResponseDto { Success = false, Message = "کاربری با این کد پرسنلی وجود دارد" });

        var user = new ApplicationUser
        {
            UserName = model.PersonnelCode,
            PersonnelCode = model.PersonnelCode,
            FirstName = model.FirstName,
            LastName = model.LastName,
            FaceCode = model.FaceCode,
            BirthDate = model.BirthDate,
            HireDate = model.HireDate,
            MobileNumber = model.MobileNumber,
            NationalCode = model.NationalCode,
            InsuranceCode = model.InsuranceCode,
            HomePhoneNumber = model.HomePhoneNumber
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "ثبت‌نام با موفقیت انجام شد",
            UserInfo = new UserInfoDto
            {
                PersonnelCode = user.PersonnelCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FaceCode = user.FaceCode,
                BirthDate = user.BirthDate,
                HireDate = user.HireDate,
                MobileNumber = user.MobileNumber,
                InsuranceCode = user.InsuranceCode,
                HomePhoneNumber = user.HomePhoneNumber
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponseDto { Success = false, Message = "اطلاعات وارد شده نامعتبر است" });

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == model.PersonnelCode);
        if (user == null)
            return Unauthorized(new AuthResponseDto { Success = false, Message = "کد پرسنلی یا رمز عبور اشتباه است" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new AuthResponseDto { Success = false, Message = "کد پرسنلی یا رمز عبور اشتباه است" });

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "ورود با موفقیت انجام شد",
            UserInfo = new UserInfoDto
            {
                PersonnelCode = user.PersonnelCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FaceCode = user.FaceCode,
                BirthDate = user.BirthDate,
                HireDate = user.HireDate,
                MobileNumber = user.MobileNumber,
                InsuranceCode = user.InsuranceCode,
                HomePhoneNumber = user.HomePhoneNumber
            }
        });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordVerifyResponseDto>> ForgotPassword(ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PersonnelCode))
            return BadRequest(new ForgotPasswordVerifyResponseDto { Success = false, Message = "کد پرسنلی الزامی است" });

        // Normalize inputs
        var personnelCode = dto.PersonnelCode.Trim();
        var nationalCode = dto.InsuranceCode?.Trim();
        var mobile = dto.MobileNumber?.Trim();

        // Find user only by PersonnelCode, then verify the other fields to avoid strict null-matching issues
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == personnelCode);
        if (user == null)
            return NotFound(new ForgotPasswordVerifyResponseDto { Success = false, Message = "کاربر با این کد پرسنلی یافت نشد" });

        // Verify against InsuranceCode (به‌عنوان کد ملی)
        if (!string.IsNullOrWhiteSpace(user.InsuranceCode))
        {
            if (string.IsNullOrWhiteSpace(nationalCode) || !string.Equals(user.InsuranceCode, nationalCode, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ForgotPasswordVerifyResponseDto { Success = false, Message = "کد ملی (InsuranceCode) وارد شده با اطلاعات کاربر مطابقت ندارد" });
        }

        // If MobileNumber exists in DB, require match; if it is null in DB, skip this check
        if (!string.IsNullOrWhiteSpace(user.MobileNumber))
        {
            if (string.IsNullOrWhiteSpace(mobile) || !string.Equals(user.MobileNumber, mobile, StringComparison.Ordinal))
                return BadRequest(new ForgotPasswordVerifyResponseDto { Success = false, Message = "شماره موبایل وارد شده با اطلاعات کاربر مطابقت ندارد" });
        }

        // Passed verification
        return Ok(new ForgotPasswordVerifyResponseDto { Success = true, UserId = user.Id, Message = "تایید شد. اکنون می‌توانید رمز عبور را تغییر دهید." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> ResetPassword(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new AuthResponseDto { Success = false, Message = "همه فیلدها الزامی است" });

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new AuthResponseDto { Success = false, Message = "کاربر یافت نشد" });

        // Remove old password and set new one
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new AuthResponseDto { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        return Ok(new AuthResponseDto { Success = true, Message = "رمز عبور با موفقیت تغییر کرد" });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.PersonnelCode ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("PersonnelCode", user.PersonnelCode ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? string.Empty));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("user-permissions")]
    [Authorize]
    public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions()
    {
        var personnelCode = User.FindFirst("PersonnelCode")?.Value;
        if (string.IsNullOrEmpty(personnelCode))
            return Unauthorized(new { message = "کاربر احراز هویت نشده است" });

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonnelCode == personnelCode);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        // نقش‌های اختصاص‌یافته به‌صورت سراسری (Identity roles)
        var globalRoleNames = await _userManager.GetRolesAsync(user);

        // نقش‌های اختصاص‌یافته به‌صورت Scoped در شرکت‌ها (UserRoleCompany)
        var userRoleCompanies = await _context.UserRoleCompanies
            .Where(urc => urc.UserId == user.Id)
            .Include(urc => urc.Role)
            .Include(urc => urc.Company)
            .ToListAsync();

        // نقش‌های اختصاص‌یافته به‌صورت Scoped در واحدها (UserRoleUnit)
        var userRoleUnits = await _context.UserRoleUnits
            .Where(uru => uru.UserId == user.Id)
            .Include(uru => uru.Role)
            .Include(uru => uru.Unit)
            .ThenInclude(u => u.Company)
            .ToListAsync();

        // لیست شرکت‌های کاربر
        var companies = userRoleCompanies
            .Select(urc => new CompanyDto { Id = urc.Company.Id, Name = urc.Company.Name, Code = urc.Company.Code })
            .DistinctBy(c => c.Id)
            .ToList();

        // لیست واحدهای کاربر
        var units = userRoleUnits
            .Select(uru => new UnitInfoDto
            {
                Id = uru.Unit.Id,
                Name = uru.Unit.Name,
                Code = uru.Unit.Code,
                CompanyId = uru.Unit.CompanyId,
                CompanyName = uru.Unit.Company?.Name
            })
            .DistinctBy(u => u.Id)
            .ToList();

        // نقش‌های کلی (نام‌ها)
        var roles = new List<string>(globalRoleNames);
        roles.AddRange(userRoleCompanies.Select(urc => urc.Role.Name!));
        roles.AddRange(userRoleUnits.Select(uru => uru.Role.Name!));
        roles = roles.Distinct().ToList();

        // بررسی اینکه آیا دسترسی سراسری وجود دارد (از طریق نقش‌ها)
        var roleInfos = await _context.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => new { r.Id, r.IsGlobalAccess })
            .ToListAsync();

        var isGlobal = roleInfos.Any(r => r.IsGlobalAccess);
        var roleIdList = roleInfos.Select(r => r.Id).ToList();

        // جمع‌آوری منوها و ویجت‌ها برای نقش‌هایی که کاربر دارد
        var userMenus = await _context.RoleMenus
            .Where(rm => roleIdList.Contains(rm.RoleId))
            .Include(rm => rm.Menu)
            .ThenInclude(m => m.SubMenus)
            .Where(rm => rm.Menu.IsActive)
            .Select(rm => new UserMenuDto
            {
                Id = rm.Menu.Id,
                Name = rm.Menu.Name,
                Title = rm.Menu.Title,
                Icon = rm.Menu.Icon,
                Url = rm.Menu.Url,
                DisplayOrder = rm.Menu.DisplayOrder,
                ParentMenuId = rm.Menu.ParentMenuId,
                CanView = rm.CanView,
                CanCreate = rm.CanCreate,
                CanEdit = rm.CanEdit,
                CanDelete = rm.CanDelete
            })
            .Distinct()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        var userWidgets = await _context.RoleWidgets
            .Where(rw => roleIdList.Contains(rw.RoleId))
            .Include(rw => rw.Widget)
            .Where(rw => rw.Widget.IsActive)
            .Select(rw => new UserWidgetDto
            {
                Id = rw.Widget.Id,
                Name = rw.Widget.Name,
                Title = rw.Widget.Title,
                Description = rw.Widget.Description,
                Icon = rw.Widget.Icon,
                WidgetType = rw.Widget.WidgetType,
                Configuration = rw.Widget.Configuration,
                DisplayOrder = rw.Widget.DisplayOrder,
                CanView = rw.CanView,
                CanConfigure = rw.CanConfigure
            })
            .Distinct()
            .OrderBy(w => w.DisplayOrder)
            .ToListAsync();

        return Ok(new UserPermissionsDto
        {
            PersonnelCode = user.PersonnelCode,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            IsGlobalAccess = isGlobal,
            Companies = companies,
            Units = units,
            Menus = userMenus,
            Widgets = userWidgets
        });
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
    {
        var users = await _userManager.Users
            .Select(u => new
            {
                u.Id,
                u.PersonnelCode,
                u.FirstName,
                u.LastName,
                u.MobileNumber,
                // NationalCode حذف شد تا ابهام ایجاد نشود
                u.FaceCode,
                u.BirthDate,
                u.HireDate,
                u.InsuranceCode,
                u.HomePhoneNumber
            })
            .OrderBy(u => u.PersonnelCode)
            .ToListAsync();

        return Ok(users);
    }
}
