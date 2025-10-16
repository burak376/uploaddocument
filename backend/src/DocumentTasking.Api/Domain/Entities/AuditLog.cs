namespace DocumentTasking.Api.Domain.Entities;

public class AuditLog : MultiTenantEntity
{
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
