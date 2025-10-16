using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DocumentTasking.Api.Infrastructure.Tenancy;

public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _overrideCompanyId;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid CompanyId
    {
        get
        {
            if (_overrideCompanyId.HasValue)
            {
                return _overrideCompanyId.Value;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var companyIdClaim = httpContext?.User?.FindFirst("companyId")?.Value;
            return Guid.TryParse(companyIdClaim, out var companyId)
                ? companyId
                : Guid.Empty;
        }
    }

    public void SetCompany(Guid companyId)
    {
        _overrideCompanyId = companyId;
    }
}
