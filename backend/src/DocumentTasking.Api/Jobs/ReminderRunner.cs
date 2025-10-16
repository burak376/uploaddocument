using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using DocumentTasking.Api.Infrastructure.Email;
using DocumentTasking.Api.Infrastructure.Tenancy;
using DocumentTasking.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocumentTasking.Api.Jobs;

public class ReminderRunner
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TaskService _taskService;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReminderRunner> _logger;
    private readonly ITenantProvider _tenantProvider;

    public ReminderRunner(
        ApplicationDbContext dbContext,
        TaskService taskService,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        IConfiguration configuration,
        ILogger<ReminderRunner> logger,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _taskService = taskService;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _configuration = configuration;
        _logger = logger;
        _tenantProvider = tenantProvider;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var reminders = await _dbContext.EmailQueues
            .Where(e => e.Status == EmailQueueStatus.Pending && (e.NextTryAtUtc == null || e.NextTryAtUtc <= nowUtc))
            .ToListAsync(ct);

        foreach (var email in reminders)
        {
            try
            {
                _tenantProvider.SetCompany(email.CompanyId);

                var task = await _dbContext.Tasks
                    .Include(t => t.Assignee)
                    .FirstAsync(t => t.Id == email.EntityId, ct);

                var missing = await _taskService.GetMissingDocumentTypesAsync(task.Id, ct);
                if (!missing.Any())
                {
                    email.Status = EmailQueueStatus.Sent;
                    email.SentAtUtc = nowUtc;
                    continue;
                }

                var timeZoneId = _configuration["Company:TimeZoneId"] ?? "Europe/Istanbul";
                var timeZone = GetTimeZone(timeZoneId);
                var model = new ReminderEmailModel(
                    _configuration["Company:Name"] ?? "Demo Şirket",
                    task.Assignee?.FullName ?? "",
                    task.Title,
                    timeZone.Id,
                    TimeZoneInfo.ConvertTimeFromUtc(task.DueDateUtc, timeZone).ToString("dd.MM.yyyy HH:mm"),
                    missing.Select(m => m.Name).ToList(),
                    string.Format(_configuration["Frontend:TaskUrl"] ?? "https://app.local/companies/{0}/tasks/{1}", task.CompanyId, task.Id)
                );

                var body = await _templateRenderer.RenderAsync("ReminderEmail", model);
                await _emailSender.SendAsync(email.To, email.Subject, body, ct);
                email.Status = EmailQueueStatus.Sent;
                email.SentAtUtc = nowUtc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reminder processing failed for {EmailId}", email.Id);
                email.TryCount += 1;
                email.Status = email.TryCount >= int.Parse(_configuration["Reminders:MaxCount"] ?? "3")
                    ? EmailQueueStatus.Failed
                    : EmailQueueStatus.Pending;
                email.NextTryAtUtc = nowUtc.AddHours(int.Parse(_configuration["Reminders:IntervalHours"] ?? "24"));
                email.Error = ex.Message;
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
