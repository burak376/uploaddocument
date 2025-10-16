using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentTasking.Api.Services;

public class TaskService
{
    private readonly ApplicationDbContext _dbContext;

    public TaskService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task, IEnumerable<Guid> requiredGroupIds, CancellationToken ct = default)
    {
        var groups = await _dbContext.DocumentGroups
            .Where(g => requiredGroupIds.Contains(g.Id))
            .ToListAsync(ct);

        foreach (var group in groups)
        {
            task.RequiredGroups.Add(new TaskRequiredGroup
            {
                TaskId = task.Id,
                DocumentGroupId = group.Id
            });
        }

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(ct);
        return task;
    }

    public async Task<IReadOnlyCollection<DocumentType>> GetMissingDocumentTypesAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.RequiredGroups)
            .ThenInclude(rg => rg.DocumentGroup)
            .ThenInclude(g => g.DocumentTypes)
            .ThenInclude(d => d.DocumentType)
            .Include(t => t.Documents)
            .FirstAsync(t => t.Id == taskId, ct);

        var requiredDocumentTypeIds = task.RequiredGroups
            .SelectMany(group => group.DocumentGroup?.DocumentTypes ?? Array.Empty<DocumentGroupItem>())
            .Select(item => item.DocumentTypeId)
            .Distinct()
            .ToList();

        var uploadedDocumentTypeIds = task.Documents
            .Where(d => d.Status != DocumentWorkflowStatus.Rejected)
            .Select(d => d.DocumentTypeId)
            .Distinct()
            .ToList();

        var missing = await _dbContext.DocumentTypes
            .Where(dt => requiredDocumentTypeIds.Contains(dt.Id) && !uploadedDocumentTypeIds.Contains(dt.Id))
            .ToListAsync(ct);

        return missing;
    }
}
