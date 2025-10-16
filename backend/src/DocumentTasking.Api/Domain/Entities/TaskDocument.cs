namespace DocumentTasking.Api.Domain.Entities;

public class TaskDocument : MultiTenantEntity
{
    public Guid TaskId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public DocumentWorkflowStatus Status { get; set; } = DocumentWorkflowStatus.Uploaded;
    public string? Notes { get; set; }
    public TaskItem? Task { get; set; }
    public DocumentType? DocumentType { get; set; }
}

public enum DocumentWorkflowStatus
{
    Uploaded,
    Approved,
    Rejected
}
