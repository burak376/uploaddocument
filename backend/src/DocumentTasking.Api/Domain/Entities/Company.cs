namespace DocumentTasking.Api.Domain.Entities;

public class Company : MultiTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
    public ICollection<UserCompanyRole> UserRoles { get; set; } = new List<UserCompanyRole>();
    public ICollection<DocumentType> DocumentTypes { get; set; } = new List<DocumentType>();
    public ICollection<DocumentGroup> DocumentGroups { get; set; } = new List<DocumentGroup>();
}
