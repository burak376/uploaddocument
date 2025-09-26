using DocumentManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Company configuration
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.TaxNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // DocumentType configuration
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.HasOne(d => d.Company)
                    .WithMany(p => p.DocumentTypes)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Document configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(e => e.FileSize)
                    .HasPrecision(10, 2);

                entity.HasOne(d => d.DocumentType)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.UploadedBy)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Only seed data if not using InMemory database in production
            // (InMemory will be seeded at runtime)
            
            // Seed Company
            modelBuilder.Entity<Company>().HasData(
                new Company
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
                }
            );

            // Seed Users with hashed passwords
            var superAdminPasswordHash = BCrypt.Net.BCrypt.HashPassword("12345");
            var companyAdminPasswordHash = BCrypt.Net.BCrypt.HashPassword("12345");
            var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("12345");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "superadmin",
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "admin@system.com",
                    PasswordHash = superAdminPasswordHash,
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
                    PasswordHash = companyAdminPasswordHash,
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
                    PasswordHash = userPasswordHash,
                    Role = UserRole.User,
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed DocumentTypes
            modelBuilder.Entity<DocumentType>().HasData(
                new DocumentType
                {
                    Id = 1,
                    Name = "TC Kimlik",
                    Description = "TC Kimlik belgesi",
                    AllowedExtensions = "[\".pdf\", \".jpg\", \".png\"]",
                    MaxFileSize = 5,
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DocumentType
                {
                    Id = 2,
                    Name = "Özlük Hakları",
                    Description = "Özlük hakları belgeleri",
                    AllowedExtensions = "[\".pdf\", \".docx\"]",
                    MaxFileSize = 10,
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new DocumentType
                {
                    Id = 3,
                    Name = "E-Defter",
                    Description = "E-Defter excel dosyaları",
                    AllowedExtensions = "[\".xlsx\", \".xls\"]",
                    MaxFileSize = 20,
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}