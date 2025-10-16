using Microsoft.AspNetCore.Http;

namespace DocumentTasking.Api.Infrastructure.Tenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (context.Request.Headers.TryGetValue("X-Company-Id", out var values) && Guid.TryParse(values, out var companyId))
        {
            tenantProvider.SetCompany(companyId);
        }

        await _next(context);
    }
}
