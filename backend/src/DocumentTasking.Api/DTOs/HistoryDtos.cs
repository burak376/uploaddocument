namespace DocumentTasking.Api.DTOs;

public record AuditLogDto(Guid Id, Guid UserId, string EventType, string EntityType, Guid? EntityId, DateTime CreatedAtUtc, string Data);
