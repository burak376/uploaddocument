namespace DocumentTasking.Api.Domain.Entities;

public class DocumentGroup : MultiTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<DocumentGroupItem> DocumentTypes { get; set; } = new List<DocumentGroupItem>();
    public ICollection<TaskRequiredGroup> RequiredForTasks { get; set; } = new List<TaskRequiredGroup>();
}
