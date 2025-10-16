namespace DocumentTasking.Api.Domain.Entities;

public class TaskRequiredGroup
{
    public Guid TaskId { get; set; }
    public Guid DocumentGroupId { get; set; }
    public TaskItem? Task { get; set; }
    public DocumentGroup? DocumentGroup { get; set; }
}
