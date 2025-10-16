using DocumentTasking.Api.Domain.Entities;

namespace DocumentTasking.Api.DTOs;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Guid AssigneeUserId,
    DateTime DueDateUtc,
    TaskPriority Priority,
    IEnumerable<Guid> RequiredGroupIds);

public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    Guid AssigneeUserId,
    DateTime DueDateUtc,
    TaskPriority Priority,
    TaskStatus Status,
    IEnumerable<Guid> RequiredGroupIds,
    IEnumerable<TaskDocumentDto> Documents);

public record TaskDocumentDto(
    Guid Id,
    Guid DocumentTypeId,
    string FileName,
    DocumentWorkflowStatus Status,
    string? Notes);

public record UpdateTaskStatusRequest(TaskStatus Status);
