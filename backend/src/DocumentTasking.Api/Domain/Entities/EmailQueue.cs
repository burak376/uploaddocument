namespace DocumentTasking.Api.Domain.Entities;

public class EmailQueue : MultiTenantEntity
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;
    public int TryCount { get; set; }
    public DateTime? NextTryAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string? Error { get; set; }
}

public enum EmailQueueStatus
{
    Pending,
    InProgress,
    Failed,
    Sent
}
