namespace JarApi.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRoleCompany> UserRoleCompanies { get; set; } = new List<UserRoleCompany>();
}
