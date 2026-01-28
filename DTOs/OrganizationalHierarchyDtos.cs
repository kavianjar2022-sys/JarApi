namespace JarApi.DTOs;

public class ManagerDto
{
    public string UserId { get; set; } = string.Empty;
    public string PersonnelCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobPositionTitle { get; set; }
    public string? UnitName { get; set; }
}

public class DirectReportDto
{
    public string UserId { get; set; } = string.Empty;
    public string PersonnelCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobPositionTitle { get; set; }
    public string? UnitName { get; set; }
    public int DirectReportsCount { get; set; }
}

public class ChainOfCommandDto
{
    public List<ManagerDto> Chain { get; set; } = new();
    public int Levels { get; set; }
}

public class OrgChartNodeDto
{
    public string UserId { get; set; } = string.Empty;
    public string PersonnelCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobPositionTitle { get; set; }
    public int JobPositionLevel { get; set; }
    public string? UnitName { get; set; }
    public string? ManagerId { get; set; }
    public List<OrgChartNodeDto> DirectReports { get; set; } = new();
}

public class AssignManagerDto
{
    public string UserId { get; set; } = string.Empty;
    public string? ManagerId { get; set; }
}

public class TeamStatisticsDto
{
    public int DirectReportsCount { get; set; }
    public int TotalSubordinatesCount { get; set; }
    public int MaxDepth { get; set; }
    public Dictionary<string, int> ByJobPosition { get; set; } = new();
    public Dictionary<string, int> ByUnit { get; set; } = new();
}
