using System.Text.Json;
using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.Infrastructure.Tenancy;

namespace DocumentTasking.Api.Services;

public interface IAuditLogger
{
    Task LogAsync(Guid userId, string eventType, string entityType, Guid? entityId, object? data, CancellationToken ct = default);
}

public class AuditLogger : IAuditLogger
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public AuditLogger(ApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task LogAsync(Guid userId, string eventType, string entityType, Guid? entityId, object? data, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            CompanyId = _tenantProvider.CompanyId,
            UserId = userId,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            Data = JsonSerializer.Serialize(data ?? new { }),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(ct);
    }
}
