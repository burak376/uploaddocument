namespace DocumentTasking.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<UserCompanyRole> CompanyRoles { get; set; } = new List<UserCompanyRole>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
