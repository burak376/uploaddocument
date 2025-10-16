using System;
using DocumentTasking.Api.Data;
using DocumentTasking.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DocumentTasking.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.AuditLog", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<string>("Data").IsRequired().HasColumnType("longtext");
                b.Property<string>("EntityType").IsRequired().HasColumnType("varchar(100)");
                b.Property<Guid?>("EntityId").HasColumnType("char(36)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.Property<string>("EventType").IsRequired().HasColumnType("varchar(100)");
                b.Property<Guid>("UserId").HasColumnType("char(36)");
                b.HasKey("Id");
                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.Company", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<string>("Name").IsRequired().HasColumnType("varchar(200)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.HasKey("Id");
                b.ToTable("Companies");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.DocumentGroup", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<string>("Code").IsRequired().HasColumnType("varchar(50)");
                b.Property<string>("Name").IsRequired().HasColumnType("varchar(200)");
                b.Property<bool>("IsActive").HasColumnType("tinyint(1)");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.HasKey("Id");
                b.ToTable("DocumentGroups");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.DocumentGroupItem", b =>
            {
                b.Property<Guid>("DocumentGroupId").HasColumnType("char(36)");
                b.Property<Guid>("DocumentTypeId").HasColumnType("char(36)");
                b.HasKey("DocumentGroupId", "DocumentTypeId");
                b.ToTable("DocumentGroupItems");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.DocumentType", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<string>("Code").IsRequired().HasColumnType("varchar(50)");
                b.Property<bool>("IsActive").HasColumnType("tinyint(1)");
                b.Property<string>("Name").IsRequired().HasColumnType("varchar(200)");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.HasKey("Id");
                b.ToTable("DocumentTypes");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.EmailQueue", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<string>("Body").IsRequired().HasColumnType("longtext");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<string>("Error").HasColumnType("longtext");
                b.Property<Guid?>("EntityId").HasColumnType("char(36)");
                b.Property<DateTime?>("NextTryAtUtc").HasColumnType("datetime(6)");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.Property<DateTime?>("SentAtUtc").HasColumnType("datetime(6)");
                b.Property<int>("Status").HasColumnType("int");
                b.Property<string>("Subject").IsRequired().HasColumnType("varchar(255)");
                b.Property<string>("To").IsRequired().HasColumnType("varchar(255)");
                b.Property<int>("TryCount").HasColumnType("int");
                b.HasKey("Id");
                b.ToTable("EmailQueues");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.TaskDocument", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<string>("ContentType").IsRequired().HasColumnType("varchar(255)");
                b.Property<Guid>("DocumentTypeId").HasColumnType("char(36)");
                b.Property<string>("FileName").IsRequired().HasColumnType("varchar(255)");
                b.Property<string>("FilePath").IsRequired().HasColumnType("varchar(500)");
                b.Property<string>("Notes").HasColumnType("longtext");
                b.Property<long>("Size").HasColumnType("bigint");
                b.Property<int>("Status").HasColumnType("int");
                b.Property<Guid>("TaskId").HasColumnType("char(36)");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.Property<DateTime>("UploadedAtUtc").HasColumnType("datetime(6)");
                b.Property<Guid>("UploadedByUserId").HasColumnType("char(36)");
                b.HasKey("Id");
                b.ToTable("TaskDocuments");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.TaskItem", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<Guid>("AssigneeUserId").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime(6)");
                b.Property<Guid>("CreatedByUserId").HasColumnType("char(36)");
                b.Property<string>("Description").HasColumnType("longtext");
                b.Property<DateTime>("DueDateUtc").HasColumnType("datetime(6)");
                b.Property<int>("Priority").HasColumnType("int");
                b.Property<byte[]>("RowVersion").IsConcurrencyToken().IsRequired().HasColumnType("rowversion");
                b.Property<int>("Status").HasColumnType("int");
                b.Property<string>("Title").IsRequired().HasColumnType("varchar(200)");
                b.HasKey("Id");
                b.ToTable("Tasks");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.TaskRequiredGroup", b =>
            {
                b.Property<Guid>("TaskId").HasColumnType("char(36)");
                b.Property<Guid>("DocumentGroupId").HasColumnType("char(36)");
                b.HasKey("TaskId", "DocumentGroupId");
                b.ToTable("TaskRequiredGroups");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id").HasColumnType("char(36)");
                b.Property<string>("Email").IsRequired().HasColumnType("varchar(255)");
                b.Property<string>("FullName").IsRequired().HasColumnType("varchar(255)");
                b.Property<bool>("IsActive").HasColumnType("tinyint(1)");
                b.HasKey("Id");
                b.ToTable("Users");
            });

            modelBuilder.Entity("DocumentTasking.Api.Domain.Entities.UserCompanyRole", b =>
            {
                b.Property<Guid>("UserId").HasColumnType("char(36)");
                b.Property<Guid>("CompanyId").HasColumnType("char(36)");
                b.Property<int>("Role").HasColumnType("int");
                b.HasKey("UserId", "CompanyId", "Role");
                b.ToTable("UserCompanyRoles");
            });
        }
    }
}
