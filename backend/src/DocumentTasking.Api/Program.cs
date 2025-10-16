using System.Text;
using DocumentTasking.Api.Data;
using DocumentTasking.Api.Infrastructure.Email;
using DocumentTasking.Api.Infrastructure.Tenancy;
using DocumentTasking.Api.Jobs;
using DocumentTasking.Api.Services;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<IEmailTemplateRenderer, RazorEmailTemplateRenderer>();
builder.Services.AddScoped<TaskService>();

var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       "server=mysql;port=3306;database=document_tasking;user=root;password=Passw0rd!";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] ?? "local-dev-secret-key-change-me";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireClaim("role", "Admin"));
    options.AddPolicy("ManageTasks", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("role", "Admin") ||
            context.User.HasClaim("role", "Manager") ||
            context.User.HasClaim("role", "Assistant")));
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(config =>
    config.UseInMemoryStorage());
builder.Services.AddHangfireServer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(
            builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173" });
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();
app.MapHangfireDashboard("/jobs").RequireAuthorization("RequireAdmin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var api = app.MapGroup("/api").RequireAuthorization();

api.MapCompanyEndpoints();
api.MapTaskEndpoints();
api.MapDocumentTypeEndpoints();
api.MapHistoryEndpoints();

app.MapPost("/auth/login", (HttpContext httpContext) => Results.Ok(new { token = "stub", refreshToken = "stub" }));
app.MapPost("/auth/refresh", () => Results.Ok(new { token = "stub" }));

RecurringJob.AddOrUpdate<ReminderRunner>(
    "task-reminder-runner",
    runner => runner.ExecuteAsync(CancellationToken.None),
    Cron.HourInterval(24));

app.Run();

public partial class Program;
