using JarApi.Data;
using JarApi.DTOs;
using JarApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrganizationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("org-chart")]
    [Authorize]
    public async Task<ActionResult<List<OrgChartNodeDto>>> GetOrgChart([FromQuery] string? rootUserId = null)
    {
        IQueryable<ApplicationUser> query = _userManager.Users
            .Include(u => u.JobPosition)
            .Include(u => u.DirectReports);

        List<ApplicationUser> users;
        if (!string.IsNullOrEmpty(rootUserId))
        {
            // نمودار از یک کاربر خاص
            var rootUser = await query.FirstOrDefaultAsync(u => u.Id == rootUserId);
            if (rootUser == null)
                return NotFound(new { message = "کاربر یافت نشد" });

            users = await GetAllSubordinatesHelper(rootUserId);
            users.Insert(0, rootUser);
        }
        else
        {
            // کل نمودار سازمانی
            users = await query.ToListAsync();
        }

        var chart = BuildOrgChart(users, rootUserId);
        return Ok(chart);
    }

    [HttpGet("user/{userId}/manager")]
    [Authorize]
    public async Task<ActionResult<ManagerDto>> GetManager(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Manager)
            .ThenInclude(m => m!.JobPosition)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        if (user.Manager == null)
            return Ok(new { message = "این کاربر مدیر ندارد", manager = (ManagerDto?)null });

        var managerUnit = await _context.UserRoleUnits
            .Where(uru => uru.UserId == user.Manager.Id)
            .Include(uru => uru.Unit)
            .Select(uru => uru.Unit.Name)
            .FirstOrDefaultAsync();

        return Ok(new ManagerDto
        {
            UserId = user.Manager.Id,
            PersonnelCode = user.Manager.PersonnelCode,
            FirstName = user.Manager.FirstName,
            LastName = user.Manager.LastName,
            JobPositionTitle = user.Manager.JobPosition?.Title,
            UnitName = managerUnit
        });
    }

    [HttpGet("user/{userId}/chain-of-command")]
    [Authorize]
    public async Task<ActionResult<ChainOfCommandDto>> GetChainOfCommand(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        var chain = new List<ManagerDto>();
        var currentUserId = user.ManagerId;
        var depth = 0;
        var maxDepth = 20; // جلوگیری از حلقه بی‌نهایت

        while (!string.IsNullOrEmpty(currentUserId) && depth < maxDepth)
        {
            var manager = await _userManager.Users
                .Include(u => u.JobPosition)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (manager == null)
                break;

            var managerUnit = await _context.UserRoleUnits
                .Where(uru => uru.UserId == manager.Id)
                .Include(uru => uru.Unit)
                .Select(uru => uru.Unit.Name)
                .FirstOrDefaultAsync();

            chain.Add(new ManagerDto
            {
                UserId = manager.Id,
                PersonnelCode = manager.PersonnelCode,
                FirstName = manager.FirstName,
                LastName = manager.LastName,
                JobPositionTitle = manager.JobPosition?.Title,
                UnitName = managerUnit
            });

            currentUserId = manager.ManagerId;
            depth++;
        }

        return Ok(new ChainOfCommandDto
        {
            Chain = chain,
            Levels = chain.Count
        });
    }

    [HttpGet("user/{userId}/direct-reports")]
    [Authorize]
    public async Task<ActionResult<List<DirectReportDto>>> GetDirectReports(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        var directReports = await _userManager.Users
            .Where(u => u.ManagerId == userId)
            .Include(u => u.JobPosition)
            .Include(u => u.DirectReports)
            .ToListAsync();

        var result = new List<DirectReportDto>();
        foreach (var report in directReports)
        {
            var unitName = await _context.UserRoleUnits
                .Where(uru => uru.UserId == report.Id)
                .Include(uru => uru.Unit)
                .Select(uru => uru.Unit.Name)
                .FirstOrDefaultAsync();

            result.Add(new DirectReportDto
            {
                UserId = report.Id,
                PersonnelCode = report.PersonnelCode,
                FirstName = report.FirstName,
                LastName = report.LastName,
                JobPositionTitle = report.JobPosition?.Title,
                UnitName = unitName,
                DirectReportsCount = report.DirectReports.Count
            });
        }

        return Ok(result);
    }

    [HttpGet("user/{userId}/all-subordinates")]
    [Authorize]
    public async Task<ActionResult<List<DirectReportDto>>> GetAllSubordinatesEndpoint(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        var subordinates = await GetAllSubordinatesHelper(userId);

        var result = new List<DirectReportDto>();
        foreach (var subordinate in subordinates)
        {
            var unitName = await _context.UserRoleUnits
                .Where(uru => uru.UserId == subordinate.Id)
                .Include(uru => uru.Unit)
                .Select(uru => uru.Unit.Name)
                .FirstOrDefaultAsync();

            result.Add(new DirectReportDto
            {
                UserId = subordinate.Id,
                PersonnelCode = subordinate.PersonnelCode,
                FirstName = subordinate.FirstName,
                LastName = subordinate.LastName,
                JobPositionTitle = subordinate.JobPosition?.Title,
                UnitName = unitName,
                DirectReportsCount = subordinate.DirectReports.Count
            });
        }

        return Ok(result);
    }

    [HttpPut("user/{userId}/assign-manager")]
    [Authorize]
    public async Task<ActionResult> AssignManager(string userId, [FromBody] AssignManagerDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        // بررسی وجود مدیر جدید
        if (!string.IsNullOrEmpty(dto.ManagerId))
        {
            if (dto.ManagerId == userId)
                return BadRequest(new { message = "کاربر نمی‌تواند مدیر خودش باشد" });

            var manager = await _userManager.FindByIdAsync(dto.ManagerId);
            if (manager == null)
                return NotFound(new { message = "مدیر یافت نشد" });

            // بررسی ایجاد حلقه (کاربر نباید مدیر، مدیر خودش باشد)
            if (await IsSubordinate(dto.ManagerId, userId))
                return BadRequest(new { message = "نمی‌توان کاربر را مدیر زیردست خودش قرار داد" });
        }

        user.ManagerId = dto.ManagerId;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "مدیر با موفقیت تخصیص داده شد" });
    }

    [HttpGet("user/{userId}/team-statistics")]
    [Authorize]
    public async Task<ActionResult<TeamStatisticsDto>> GetTeamStatistics(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "کاربر یافت نشد" });

        var directReports = await _userManager.Users
            .Where(u => u.ManagerId == userId)
            .ToListAsync();

        var allSubordinates = await GetAllSubordinatesHelper(userId);

        var maxDepth = await CalculateMaxDepth(userId);

        var byJobPosition = allSubordinates
            .Where(s => s.JobPosition != null)
            .GroupBy(s => s.JobPosition!.Title)
            .ToDictionary(g => g.Key, g => g.Count());

        var byUnit = new Dictionary<string, int>();
        foreach (var subordinate in allSubordinates)
        {
            var unitName = await _context.UserRoleUnits
                .Where(uru => uru.UserId == subordinate.Id)
                .Include(uru => uru.Unit)
                .Select(uru => uru.Unit.Name)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(unitName))
            {
                if (!byUnit.ContainsKey(unitName))
                    byUnit[unitName] = 0;
                byUnit[unitName]++;
            }
        }

        return Ok(new TeamStatisticsDto
        {
            DirectReportsCount = directReports.Count,
            TotalSubordinatesCount = allSubordinates.Count,
            MaxDepth = maxDepth,
            ByJobPosition = byJobPosition,
            ByUnit = byUnit
        });
    }

    #region Helper Methods

    private async Task<List<ApplicationUser>> GetAllSubordinatesHelper(string managerId)
    {
        var result = new List<ApplicationUser>();
        var directReports = await _userManager.Users
            .Where(u => u.ManagerId == managerId)
            .Include(u => u.JobPosition)
            .Include(u => u.DirectReports)
            .ToListAsync();

        foreach (var report in directReports)
        {
            result.Add(report);
            var subSubordinates = await GetAllSubordinatesHelper(report.Id);
            result.AddRange(subSubordinates);
        }

        return result;
    }

    private async Task<bool> IsSubordinate(string potentialSubordinateId, string managerId)
    {
        var current = await _userManager.FindByIdAsync(potentialSubordinateId);
        var depth = 0;
        var maxDepth = 20;

        while (current != null && !string.IsNullOrEmpty(current.ManagerId) && depth < maxDepth)
        {
            if (current.ManagerId == managerId)
                return true;

            current = await _userManager.FindByIdAsync(current.ManagerId);
            depth++;
        }

        return false;
    }

    private async Task<int> CalculateMaxDepth(string managerId, int currentDepth = 0)
    {
        var directReports = await _userManager.Users
            .Where(u => u.ManagerId == managerId)
            .Select(u => u.Id)
            .ToListAsync();

        if (!directReports.Any())
            return currentDepth;

        var depths = new List<int>();
        foreach (var reportId in directReports)
        {
            var depth = await CalculateMaxDepth(reportId, currentDepth + 1);
            depths.Add(depth);
        }

        return depths.Max();
    }

    private List<OrgChartNodeDto> BuildOrgChart(List<ApplicationUser> users, string? rootUserId)
    {
        var userDict = users.ToDictionary(u => u.Id);
        var roots = new List<OrgChartNodeDto>();

        foreach (var user in users)
        {
            if ((rootUserId == null && user.ManagerId == null) || 
                (rootUserId != null && user.Id == rootUserId))
            {
                roots.Add(BuildOrgNode(user, userDict));
            }
        }

        return roots;
    }

    private OrgChartNodeDto BuildOrgNode(ApplicationUser user, Dictionary<string, ApplicationUser> userDict)
    {
        var node = new OrgChartNodeDto
        {
            UserId = user.Id,
            PersonnelCode = user.PersonnelCode,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JobPositionTitle = user.JobPosition?.Title,
            JobPositionLevel = user.JobPosition?.Level ?? 0,
            ManagerId = user.ManagerId,
            DirectReports = new List<OrgChartNodeDto>()
        };

        var subordinates = userDict.Values.Where(u => u.ManagerId == user.Id).ToList();
        foreach (var subordinate in subordinates)
        {
            node.DirectReports.Add(BuildOrgNode(subordinate, userDict));
        }

        return node;
    }

    #endregion
}
