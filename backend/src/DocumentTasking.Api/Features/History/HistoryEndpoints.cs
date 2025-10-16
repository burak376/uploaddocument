using DocumentTasking.Api.Data;
using DocumentTasking.Api.DTOs;
using DocumentTasking.Api.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace DocumentTasking.Api.Features.History;

public static class HistoryEndpoints
{
    public static RouteGroupBuilder MapHistoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/companies/{companyId:guid}/history", async (Guid companyId, string? entityType, Guid? entityId, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var query = db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(log => log.EntityType == entityType);
            }

            if (entityId.HasValue)
            {
                query = query.Where(log => log.EntityId == entityId);
            }

            return await query
                .OrderByDescending(log => log.CreatedAtUtc)
                .Take(50)
                .Select(log => new AuditLogDto(log.Id, log.UserId, log.EventType, log.EntityType, log.EntityId, log.CreatedAtUtc, log.Data))
                .ToListAsync();
        });

        return group;
    }
}
