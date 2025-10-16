namespace DocumentTasking.Api.DTOs;

public record CompanySummaryDto(Guid Id, string Name);

public record CreateCompanyRequest(string Name);

public record UpdateCompanyRequest(string Name);
