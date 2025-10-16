namespace DocumentTasking.Api.Domain.Entities;

public abstract class MultiTenantEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
