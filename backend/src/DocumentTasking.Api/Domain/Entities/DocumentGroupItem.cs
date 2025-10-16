namespace DocumentTasking.Api.Domain.Entities;

public class DocumentGroupItem
{
    public Guid DocumentGroupId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public DocumentGroup? Group { get; set; }
    public DocumentType? DocumentType { get; set; }
}
