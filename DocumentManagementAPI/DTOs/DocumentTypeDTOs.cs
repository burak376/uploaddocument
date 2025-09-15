using System.ComponentModel.DataAnnotations;

namespace DocumentManagementAPI.DTOs
{
    public class DocumentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> AllowedExtensions { get; set; } = new();
        public int MaxFileSize { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDocumentTypeDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public List<string> AllowedExtensions { get; set; } = new();

        [Required]
        [Range(1, 100)]
        public int MaxFileSize { get; set; }

        public int? CompanyId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDocumentTypeDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public List<string> AllowedExtensions { get; set; } = new();

        [Required]
        [Range(1, 100)]
        public int MaxFileSize { get; set; }

        public bool IsActive { get; set; }
    }
}