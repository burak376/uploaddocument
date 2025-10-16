using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace DocumentTasking.Api.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserCompanyRole> UserCompanyRoles => Set<UserCompanyRole>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<DocumentGroup> DocumentGroups => Set<DocumentGroup>();
    public DbSet<DocumentGroupItem> DocumentGroupItems => Set<DocumentGroupItem>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskRequiredGroup> TaskRequiredGroups => Set<TaskRequiredGroup>();
    public DbSet<TaskDocument> TaskDocuments => Set<TaskDocument>();
    public DbSet<EmailQueue> EmailQueues => Set<EmailQueue>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DocumentType>().HasQueryFilter(c => c.CompanyId == _tenantProvider.CompanyId);
        modelBuilder.Entity<DocumentGroup>().HasQueryFilter(c => c.CompanyId == _tenantProvider.CompanyId);
        modelBuilder.Entity<TaskItem>().HasQueryFilter(c => c.CompanyId == _tenantProvider.CompanyId);
        modelBuilder.Entity<TaskDocument>().HasQueryFilter(c => c.CompanyId == _tenantProvider.CompanyId);
        modelBuilder.Entity<EmailQueue>().HasQueryFilter(c => c.CompanyId == _tenantProvider.CompanyId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(c => c.CompanyId == _tenantProvider.CompanyId);

        modelBuilder.Entity<UserCompanyRole>().HasKey(x => new { x.UserId, x.CompanyId, x.Role });
        modelBuilder.Entity<DocumentGroupItem>().HasKey(x => new { x.DocumentGroupId, x.DocumentTypeId });
        modelBuilder.Entity<TaskRequiredGroup>().HasKey(x => new { x.TaskId, x.DocumentGroupId });

        modelBuilder.Entity<Company>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<DocumentType>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<DocumentGroup>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<TaskItem>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<TaskDocument>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<EmailQueue>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<AuditLog>().Property(x => x.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
    }
}
