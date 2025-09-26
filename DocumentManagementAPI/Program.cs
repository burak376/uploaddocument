using DocumentManagementAPI.Data;
using DocumentManagementAPI.Services;
using DocumentManagementAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Check if we have environment variables for database connection (Render deployment)
    var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "sql7.freesqldatabase.com";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "sql7800199";
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sql7800199";
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "xa3L1w7xpG";
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
    
    // Always use MySQL for production/render deployment
    if (!builder.Environment.IsDevelopment())
    {
        // Production/Render - use MySQL with FreeSQLDatabase
        var connectionString = $"Server={dbServer};Database={dbName};User={dbUser};Password={dbPassword};Port={dbPort};SslMode=Required;";
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
    else
    {
        // Local development - use appsettings or InMemory
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        else
        {
            // Fallback to InMemory for development
            options.UseInMemoryDatabase("DocumentManagementDB");
        }
    }
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173", 
            "http://localhost:3000",
            "https://*.netlify.app",
            "https://68d6976--dokumanyukleme.netlify.app",
            "https://dokumanyukleme.netlify.app",
            "https://uploaddocumentbe.onrender.com"
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Document Management API",
        Version = "v1",
        Description = "API for Document Management System"
    });

    // JWT Bearer token configuration for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Management API V1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization - non-blocking
_ = Task.Run(async () =>
{
    await Task.Delay(2000); // Wait 2 seconds for app to start
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (app.Environment.IsDevelopment())
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // For InMemory database, ensure created and seed data
            await context.Database.EnsureCreatedAsync();
            await SeedInMemoryDataAsync(context);
        }
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Database initialization failed, will retry on first request");
    }
});

Log.Information("Document Management API starting up...");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Seed data for InMemory database
async Task SeedInMemoryDataAsync(ApplicationDbContext context)
{
    if (await context.Companies.AnyAsync()) return; // Already seeded
    
    // Seed Company
    var company = new Company
    {
        Id = 1,
        Name = "Bugibo Yazılım",
        TaxNumber = "1234567890",
        Address = "İstanbul, Türkiye",
        Phone = "0212 123 45 67",
        Email = "info@bugibo.com",
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    context.Companies.Add(company);
    await context.SaveChangesAsync();

    // Seed Users
    var users = new[]
    {
        new User
        {
            Id = 1,
            Username = "superadmin",
            FirstName = "Super",
            LastName = "Admin",
            Email = "admin@system.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345"),
            Role = UserRole.SuperAdmin,
            CompanyId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new User
        {
            Id = 2,
            Username = "bugibo_admin",
            FirstName = "Bugibo",
            LastName = "Admin",
            Email = "admin@bugibo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345"),
            Role = UserRole.CompanyAdmin,
            CompanyId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new User
        {
            Id = 3,
            Username = "burak",
            FirstName = "Burak",
            LastName = "Kullanıcı",
            Email = "burak@bugibo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345"),
            Role = UserRole.User,
            CompanyId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    };
    
    context.Users.AddRange(users);
    await context.SaveChangesAsync();
    
    Log.Information("InMemory database seeded successfully");
}