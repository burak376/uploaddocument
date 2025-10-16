using DocumentTasking.Api.Services;
using Microsoft.AspNetCore.Http;

namespace DocumentTasking.Api.Infrastructure.Tenancy;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogger auditLogger)
    {
        await _next(context);

        if (context.Items.TryGetValue("AuditEntries", out var value) && value is List<AuditEntry> entries)
        {
            foreach (var entry in entries)
            {
                await auditLogger.LogAsync(entry.UserId, entry.EventType, entry.EntityType, entry.EntityId, entry.Data);
            }
        }
    }

    public class AuditEntry
    {
        public Guid UserId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public string EntityType { get; init; } = string.Empty;
        public Guid? EntityId { get; init; }
        public object? Data { get; init; }
    }
}
