using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.DTOs;
using DocumentTasking.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DocumentTasking.Api.Features.DocumentTypes;

public static class DocumentTypeEndpoints
{
    public static RouteGroupBuilder MapDocumentTypeEndpoints(this RouteGroupBuilder group)
    {
        var documentGroup = group.MapGroup("/companies/{companyId:guid}");

        documentGroup.MapGet("/document-types", async (Guid companyId, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            return await db.DocumentTypes
                .Select(dt => new DocumentTypeDto(dt.Id, dt.Name, dt.Code, dt.IsActive))
                .ToListAsync();
        });

        documentGroup.MapPost("/document-types", async Task<Results<Created<DocumentTypeDto>, ValidationProblem>> (Guid companyId, CreateDocumentTypeRequest request, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = new[] { "Name required" } });
            }

            var docType = new DocumentType
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Name = request.Name,
                Code = request.Code,
                IsActive = true
            };

            db.DocumentTypes.Add(docType);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/companies/{companyId}/document-types/{docType.Id}", new DocumentTypeDto(docType.Id, docType.Name, docType.Code, docType.IsActive));
        });

        documentGroup.MapPut("/document-types/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid companyId, Guid id, UpdateDocumentTypeRequest request, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var docType = await db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (docType is null)
            {
                return TypedResults.NotFound();
            }

            docType.Name = request.Name;
            docType.Code = request.Code;
            docType.IsActive = request.IsActive;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        });

        documentGroup.MapDelete("/document-types/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid companyId, Guid id, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var docType = await db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (docType is null)
            {
                return TypedResults.NotFound();
            }

            db.DocumentTypes.Remove(docType);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        });

        documentGroup.MapGet("/document-groups", async (Guid companyId, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            return await db.DocumentGroups
                .Include(g => g.DocumentTypes)
                .Select(g => new DocumentGroupDto(g.Id, g.Name, g.Code, g.IsActive, g.DocumentTypes.Select(i => i.DocumentTypeId)))
                .ToListAsync();
        });

        documentGroup.MapPost("/document-groups", async Task<Results<Created<DocumentGroupDto>, ValidationProblem>> (Guid companyId, CreateDocumentGroupRequest request, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            tenant.SetCompany(companyId);
            var groupEntity = new DocumentGroup
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Name = request.Name,
                Code = request.Code,
                IsActive = true
            };

            foreach (var documentTypeId in request.DocumentTypeIds)
            {
                groupEntity.DocumentTypes.Add(new DocumentGroupItem
                {
                    DocumentGroupId = groupEntity.Id,
                    DocumentTypeId = documentTypeId
                });
            }

            db.DocumentGroups.Add(groupEntity);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/companies/{companyId}/document-groups/{groupEntity.Id}", new DocumentGroupDto(groupEntity.Id, groupEntity.Name, groupEntity.Code, groupEntity.IsActive, groupEntity.DocumentTypes.Select(i => i.DocumentTypeId)));
        });

        return group;
    }
}
