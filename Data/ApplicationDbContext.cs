using JarApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JarApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets برای جداول جدید
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Widget> Widgets { get; set; }
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<RoleWidget> RoleWidgets { get; set; }

    // شرکت‌ها و تخصیص نقش به کاربر در شرکت
    public DbSet<Company> Companies { get; set; }
    public DbSet<UserRoleCompany> UserRoleCompanies { get; set; }

    // واحدها و تخصیص نقش به کاربر در واحد
    public DbSet<Unit> Units { get; set; }
    public DbSet<UserRoleUnit> UserRoleUnits { get; set; }

    // شیفت کاری
    public DbSet<ShiftDefinition> ShiftDefinitions { get; set; }
    public DbSet<ShiftRotationSegment> ShiftRotationSegments { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }

    // مدارک تحصیلی
    public DbSet<EducationDegree> EducationDegrees { get; set; }

    // سمت‌های شغلی
    public DbSet<JobPosition> JobPositions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // تنظیمات اضافی برای ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            // کد پرسنلی باید یونیک باشد
            entity.HasIndex(e => e.PersonnelCode).IsUnique();
            
            // تنظیم طول فیلدها
            entity.Property(e => e.PersonnelCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FaceCode).HasMaxLength(50);
            entity.Property(e => e.MobileNumber).HasMaxLength(15);
            entity.Property(e => e.NationalCode).HasMaxLength(20);
            entity.Property(e => e.InsuranceCode).HasMaxLength(50);
            entity.Property(e => e.HomePhoneNumber).HasMaxLength(15);

            // رابطه با مدرک تحصیلی
            entity.HasOne(e => e.EducationDegree)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.EducationDegreeId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با سمت شغلی
            entity.HasOne(e => e.JobPosition)
                .WithMany(j => j.Users)
                .HasForeignKey(e => e.JobPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه سلسله مراتب سازمانی (مدیر-زیردست)
            entity.HasOne(e => e.Manager)
                .WithMany(m => m.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // تنظیمات ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.HasOne(r => r.Company)
                .WithMany()
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // تنظیمات Menu
        builder.Entity<Menu>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Url).HasMaxLength(500);

            // رابطه خود-ارجاعی برای زیرمنوها
            entity.HasOne(e => e.ParentMenu)
                .WithMany(e => e.SubMenus)
                .HasForeignKey(e => e.ParentMenuId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // تنظیمات Widget
        builder.Entity<Widget>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.WidgetType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // تنظیمات RoleMenu (Many-to-Many)
        builder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(rm => new { rm.RoleId, rm.MenuId });

            entity.HasOne(rm => rm.Role)
                .WithMany()
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // تنظیمات RoleWidget (Many-to-Many)
        builder.Entity<RoleWidget>(entity =>
        {
            entity.HasKey(rw => new { rw.RoleId, rw.WidgetId });

            entity.HasOne(rw => rw.Role)
                .WithMany()
                .HasForeignKey(rw => rw.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rw => rw.Widget)
                .WithMany(w => w.RoleWidgets)
                .HasForeignKey(rw => rw.WidgetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // تنظیمات Company
        builder.Entity<Company>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Code).HasMaxLength(50);
        });

        // تنظیمات UserRoleCompany (نقش کاربر در شرکت)
        builder.Entity<UserRoleCompany>(entity =>
        {
            entity.HasKey(urc => new { urc.UserId, urc.RoleId, urc.CompanyId });

            entity.HasOne(urc => urc.User)
                .WithMany()
                .HasForeignKey(urc => urc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(urc => urc.Role)
                .WithMany()
                .HasForeignKey(urc => urc.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(urc => urc.Company)
                .WithMany(c => c.UserRoleCompanies)
                .HasForeignKey(urc => urc.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // تنظیمات Unit (واحد)
        builder.Entity<Unit>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Code).HasMaxLength(50);

            entity.HasOne(u => u.Company)
                .WithMany(c => c.Units)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // سلسله‌مراتب واحد
            entity.HasOne(u => u.ParentUnit)
                .WithMany(u => u.SubUnits)
                .HasForeignKey(u => u.ParentUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // تنظیمات UserRoleUnit (نقش کاربر در واحد)
        builder.Entity<UserRoleUnit>(entity =>
        {
            entity.HasKey(uru => new { uru.UserId, uru.RoleId, uru.UnitId });

            entity.HasOne(uru => uru.User)
                .WithMany()
                .HasForeignKey(uru => uru.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uru => uru.Role)
                .WithMany()
                .HasForeignKey(uru => uru.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uru => uru.Unit)
                .WithMany(u => u.UserRoleUnits)
                .HasForeignKey(uru => uru.UnitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // تنظیمات ShiftDefinition
        builder.Entity<ShiftDefinition>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.HasMany(s => s.RotationSegments)
                .WithOne(r => r.ShiftDefinition)
                .HasForeignKey(r => r.ShiftDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // تنظیمات ShiftRotationSegment
        builder.Entity<ShiftRotationSegment>(entity =>
        {
            entity.Property(r => r.Label).HasMaxLength(50).IsRequired();
            entity.Property(r => r.DurationDays).IsRequired();
        });

        // تنظیمات ShiftAssignment
        builder.Entity<ShiftAssignment>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.ShiftDefinition)
                .WithMany()
                .HasForeignKey(a => a.ShiftDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.UserId, a.StartDate, a.EndDate });
        });

        // تنظیمات EducationDegree
        builder.Entity<EducationDegree>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
        });

        // تنظیمات JobPosition
        builder.Entity<JobPosition>(entity =>
        {
            entity.Property(j => j.Title).HasMaxLength(200).IsRequired();
            entity.Property(j => j.Code).HasMaxLength(50);
            entity.Property(j => j.Description).HasMaxLength(500);

            // سلسله‌مراتب سمت‌های شغلی
            entity.HasOne(j => j.ParentPosition)
                .WithMany(p => p.SubPositions)
                .HasForeignKey(j => j.ParentPositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
