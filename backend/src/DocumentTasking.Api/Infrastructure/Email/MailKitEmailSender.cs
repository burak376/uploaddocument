using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace DocumentTasking.Api.Infrastructure.Email;

public class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IConfiguration configuration, ILogger<MailKitEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_configuration["Smtp:From"] ?? "noreply@example.com"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Smtp:Host"] ?? "mailhog",
            int.Parse(_configuration["Smtp:Port"] ?? "1025"),
            false,
            ct);
        if (!string.IsNullOrWhiteSpace(_configuration["Smtp:User"]))
        {
            await client.AuthenticateAsync(_configuration["Smtp:User"], _configuration["Smtp:Password"]);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email sent to {Recipient}", to);
    }
}
