namespace DocumentTasking.Api.Domain.Entities;

public class TaskItem : MultiTenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid AssigneeUserId { get; set; }
    public User? Assignee { get; set; }
    public DateTime DueDateUtc { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Open;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<TaskRequiredGroup> RequiredGroups { get; set; } = new List<TaskRequiredGroup>();
    public ICollection<TaskDocument> Documents { get; set; } = new List<TaskDocument>();
}

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum TaskStatus
{
    Open,
    InProgress,
    Review,
    Completed,
    Cancelled
}
