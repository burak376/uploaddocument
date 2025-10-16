namespace DocumentTasking.Api.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
