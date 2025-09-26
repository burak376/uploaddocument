using DocumentManagementAPI.Data;
using DocumentManagementAPI.Services;
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
    // Try multiple connection string sources
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
    Log.Information("Using connection string: {ConnectionString}", 
        connectionString?.Replace("Password=XiYk50BTwrsVV110", "Password=***"));
    
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            Log.Information("Attempting TiDB Cloud connection...");
            
            // Try with fixed server version first
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
            options.UseMySql(connectionString, serverVersion, mysqlOptions =>
            {
                mysqlOptions.CommandTimeout(60);
                // Disable retry for debugging
                // mysqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(10), null);
            });
            
            Log.Information("TiDB Cloud MySQL configuration applied successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure TiDB Cloud: {ErrorMessage}. Inner Exception: {InnerException}", 
                ex.Message, ex.InnerException?.Message);
            
            // Try alternative connection string format
            try
            {
                Log.Information("Trying alternative connection format...");
                var altConnectionString = "server=gateway01.eu-central-1.prod.aws.tidbcloud.com;port=4000;database=test;uid=oWWCakYcn8Js91E.root;pwd=XiYk50BTwrsVV110;sslmode=required;";
                
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
                options.UseMySql(altConnectionString, serverVersion, mysqlOptions =>
                {
                    mysqlOptions.CommandTimeout(60);
                });
                
                Log.Information("Alternative connection format worked!");
            }
            catch (Exception altEx)
            {
                Log.Error(altEx, "Alternative connection also failed: {ErrorMessage}", altEx.Message);
                Log.Information("Falling back to InMemory database");
                options.UseInMemoryDatabase("DocumentManagementDB");
            }
        }
    }
    else
    {
        Log.Warning("No connection string found, using InMemory database");
        options.UseInMemoryDatabase("DocumentManagementDB");
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

// Database initialization and seeding - non-blocking
_ = Task.Run(async () =>
{
    await Task.Delay(10000); // Wait 10 seconds for app to start
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Test database connection first
        Log.Information("Testing database connection...");
        await context.Database.CanConnectAsync();
        Log.Information("Database connection test successful");
        
        // Always ensure database is created and seeded
        Log.Information("Ensuring database schema and seeding data...");
        await context.Database.EnsureCreatedAsync();
        await SeedDatabaseAsync(context);
        
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database initialization failed: {ErrorMessage}. Using InMemory fallback.", ex.Message);
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

// Seed data for database
async Task SeedDatabaseAsync(ApplicationDbContext context)
{
    if (await context.Companies.AnyAsync()) 
    {
        Log.Information("Database already contains data, skipping seed");
        return; // Already seeded
    }
    
    Log.Information("Seeding database with initial data...");
    
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
    Log.Information("Added company: {CompanyName}", company.Name);
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
    
    Log.Information("Database seeded successfully with {UserCount} users and {CompanyCount} companies", 
        users.Length, 1);
}