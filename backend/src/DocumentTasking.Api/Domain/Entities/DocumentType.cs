namespace DocumentTasking.Api.Domain.Entities;

public class DocumentType : MultiTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<DocumentGroupItem> DocumentGroups { get; set; } = new List<DocumentGroupItem>();
}
