using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.Infrastructure.Tenancy;
using DocumentTasking.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DocumentTasking.Tests.Services;

public class TaskServiceTests
{
    [Fact]
    public async Task GetMissingDocumentTypesAsync_ReturnsMissingTypes()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.CompanyId).Returns(Guid.NewGuid());

        await using var context = new ApplicationDbContext(options, tenantProvider.Object);

        var groupId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        context.DocumentTypes.Add(new DocumentType { Id = documentTypeId, CompanyId = tenantProvider.Object.CompanyId, Name = "Kimlik", Code = "TC" });
        context.DocumentGroups.Add(new DocumentGroup { Id = groupId, CompanyId = tenantProvider.Object.CompanyId, Name = "Genel", Code = "GEN" });
        context.DocumentGroupItems.Add(new DocumentGroupItem { DocumentGroupId = groupId, DocumentTypeId = documentTypeId });
        context.Tasks.Add(new TaskItem { Id = taskId, CompanyId = tenantProvider.Object.CompanyId, Title = "Belge yükle", AssigneeUserId = Guid.NewGuid(), DueDateUtc = DateTime.UtcNow, Priority = TaskPriority.Normal });
        context.TaskRequiredGroups.Add(new TaskRequiredGroup { TaskId = taskId, DocumentGroupId = groupId });
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var missing = await service.GetMissingDocumentTypesAsync(taskId);

        Assert.Single(missing);
        Assert.Equal(documentTypeId, missing.First().Id);
    }
}
