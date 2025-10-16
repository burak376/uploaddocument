namespace DocumentTasking.Api.DTOs;

public record DocumentTypeDto(Guid Id, string Name, string Code, bool IsActive);

public record CreateDocumentTypeRequest(string Name, string Code);

public record UpdateDocumentTypeRequest(string Name, string Code, bool IsActive);

public record DocumentGroupDto(Guid Id, string Name, string Code, bool IsActive, IEnumerable<Guid> DocumentTypeIds);

public record CreateDocumentGroupRequest(string Name, string Code, IEnumerable<Guid> DocumentTypeIds);

public record UpdateDocumentGroupRequest(string Name, string Code, bool IsActive, IEnumerable<Guid> DocumentTypeIds);
