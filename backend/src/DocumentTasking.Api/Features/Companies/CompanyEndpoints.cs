using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.DTOs;
using DocumentTasking.Api.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DocumentTasking.Api.Features.Companies;

public static class CompanyEndpoints
{
    public static RouteGroupBuilder MapCompanyEndpoints(this RouteGroupBuilder group)
    {
        var companies = group.MapGroup("/companies").RequireAuthorization("RequireAdmin");

        companies.MapGet("", async (ApplicationDbContext db, ITenantProvider tenant) =>
        {
            var query = db.Companies.AsQueryable();
            if (tenant.CompanyId != Guid.Empty)
            {
                query = query.Where(c => c.CompanyId == tenant.CompanyId);
            }

            return await query
                .Select(c => new CompanySummaryDto(c.Id, c.Name))
                .ToListAsync();
        });

        companies.MapPost("", async Task<Results<Created<CompanySummaryDto>, ValidationProblem>> (CreateCompanyRequest request, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = new[] { "Name is required" } });
            }

            var newCompanyId = Guid.NewGuid();
            var company = new Company
            {
                Id = newCompanyId,
                CompanyId = newCompanyId,
                Name = request.Name,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Companies.Add(company);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/companies/{company.Id}", new CompanySummaryDto(company.Id, company.Name));
        });

        companies.MapPut("/{companyId:guid}", async Task<Results<NoContent, NotFound>> (Guid companyId, UpdateCompanyRequest request, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId && c.CompanyId == tenant.CompanyId);
            if (company is null)
            {
                return TypedResults.NotFound();
            }

            company.Name = request.Name;
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        });

        companies.MapDelete("/{companyId:guid}", async Task<Results<NoContent, NotFound>> (Guid companyId, ApplicationDbContext db, ITenantProvider tenant) =>
        {
            var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId && c.CompanyId == tenant.CompanyId);
            if (company is null)
            {
                return TypedResults.NotFound();
            }

            db.Companies.Remove(company);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        });

        return group;
    }
}
