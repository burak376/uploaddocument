using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.DTOs;
using DocumentTasking.Api.Infrastructure.Tenancy;
using DocumentTasking.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DocumentTasking.Api.Features.Tasks;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this RouteGroupBuilder group)
    {
        var tasks = group.MapGroup("/companies/{companyId:guid}/tasks").RequireAuthorization("ManageTasks");

        tasks.MapPost("", async Task<Results<Created<TaskDetailDto>, ValidationProblem>> (Guid companyId, CreateTaskRequest request, ApplicationDbContext db, TaskService taskService, ITenantProvider tenant, HttpContext context) =>
        {
            tenant.SetCompany(companyId);

            if (request.RequiredGroupIds is null || !request.RequiredGroupIds.Any())
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["RequiredGroupIds"] = new[] { "At least one document group is required" } });
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Title = request.Title,
                Description = request.Description,
                AssigneeUserId = request.AssigneeUserId,
                DueDateUtc = request.DueDateUtc,
                Priority = request.Priority,
                Status = TaskStatus.Open,
                CreatedByUserId = Guid.Parse(context.User.FindFirst("sub")?.Value ?? Guid.Empty.ToString()),
                CreatedAtUtc = DateTime.UtcNow
            };

            await taskService.CreateTaskAsync(task, request.RequiredGroupIds, context.RequestAborted);

            context.AddAuditEntry(new AuditLoggingMiddleware.AuditEntry
            {
                UserId = task.CreatedByUserId,
                EventType = "TaskCreated",
                EntityType = nameof(TaskItem),
                EntityId = task.Id,
                Data = new { task.Title, task.AssigneeUserId, task.DueDateUtc }
            });

            return TypedResults.Created($"/api/companies/{companyId}/tasks/{task.Id}", MapToDetail(task));
        });

        tasks.MapGet("", async (Guid companyId, [AsParameters] TaskFilter filter, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var query = db.Tasks.AsQueryable();

            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status);
            }

            if (filter.AssigneeUserId.HasValue)
            {
                query = query.Where(t => t.AssigneeUserId == filter.AssigneeUserId);
            }

            return await query
                .Select(t => new TaskDetailDto(t.Id, t.Title, t.Description, t.AssigneeUserId, t.DueDateUtc, t.Priority, t.Status, t.RequiredGroups.Select(r => r.DocumentGroupId), t.Documents.Select(d => new TaskDocumentDto(d.Id, d.DocumentTypeId, d.FileName, d.Status, d.Notes))))
                .ToListAsync();
        });

        tasks.MapGet("/{taskId:guid}", async Task<Results<Ok<TaskDetailDto>, NotFound>> (Guid companyId, Guid taskId, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var task = await db.Tasks
                .Include(t => t.RequiredGroups)
                .Include(t => t.Documents)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(MapToDetail(task));
        });

        tasks.MapPatch("/{taskId:guid}/status", async Task<Results<NoContent, NotFound>> (Guid companyId, Guid taskId, UpdateTaskStatusRequest request, ApplicationDbContext db, HttpContext context, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task is null)
            {
                return TypedResults.NotFound();
            }

            task.Status = request.Status;
            await db.SaveChangesAsync();

            context.AddAuditEntry(new AuditLoggingMiddleware.AuditEntry
            {
                UserId = Guid.Parse(context.User.FindFirst("sub")?.Value ?? Guid.Empty.ToString()),
                EventType = "TaskStatusChanged",
                EntityType = nameof(TaskItem),
                EntityId = task.Id,
                Data = new { task.Status }
            });

            return TypedResults.NoContent();
        });

        return group;
    }

    private static TaskDetailDto MapToDetail(TaskItem task)
    {
        return new TaskDetailDto(
            task.Id,
            task.Title,
            task.Description,
            task.AssigneeUserId,
            task.DueDateUtc,
            task.Priority,
            task.Status,
            task.RequiredGroups.Select(r => r.DocumentGroupId),
            task.Documents.Select(d => new TaskDocumentDto(d.Id, d.DocumentTypeId, d.FileName, d.Status, d.Notes))
        );
    }

    public record TaskFilter(TaskStatus? Status, Guid? AssigneeUserId);
}
