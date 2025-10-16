namespace DocumentTasking.Api.Domain.Entities;

public class UserCompanyRole
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public CompanyRole Role { get; set; }
    public User? User { get; set; }
    public Company? Company { get; set; }
}

public enum CompanyRole
{
    Admin,
    Manager,
    Assistant,
    Staff
}
