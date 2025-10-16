namespace DocumentTasking.Api.Infrastructure.Email;

public record ReminderEmailModel(
    string CompanyName,
    string AssigneeName,
    string TaskTitle,
    string CompanyTimeZoneId,
    string DueDateLocal,
    IReadOnlyCollection<string> MissingDocuments,
    string TaskUrl);
