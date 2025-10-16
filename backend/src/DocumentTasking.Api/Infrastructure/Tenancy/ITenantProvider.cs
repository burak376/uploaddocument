namespace DocumentTasking.Api.Infrastructure.Tenancy;

public interface ITenantProvider
{
    Guid CompanyId { get; }
    void SetCompany(Guid companyId);
}
