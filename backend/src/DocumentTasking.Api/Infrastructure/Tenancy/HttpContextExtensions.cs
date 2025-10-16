using Microsoft.AspNetCore.Http;

namespace DocumentTasking.Api.Infrastructure.Tenancy;

public static class HttpContextExtensions
{
    public static void AddAuditEntry(this HttpContext context, AuditLoggingMiddleware.AuditEntry entry)
    {
        if (!context.Items.TryGetValue("AuditEntries", out var value) || value is not List<AuditLoggingMiddleware.AuditEntry> entries)
        {
            entries = new List<AuditLoggingMiddleware.AuditEntry>();
            context.Items["AuditEntries"] = entries;
        }

        entries.Add(entry);
    }
}
