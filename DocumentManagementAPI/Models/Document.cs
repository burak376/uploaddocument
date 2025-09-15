using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagementAPI.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty; // System generated name

        [Required]
        [StringLength(255)]
        public string OriginalName { get; set; } = string.Empty; // User uploaded name

        [Required]
        public decimal FileSize { get; set; } // MB cinsinden

        [Required]
        [StringLength(10)]
        public string FileExtension { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public int DocumentTypeId { get; set; }

        [ForeignKey("DocumentTypeId")]
        public DocumentType DocumentType { get; set; } = null!;

        [Required]
        public int UploadedById { get; set; }

        [ForeignKey("UploadedById")]
        public User UploadedBy { get; set; } = null!;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; } = null!;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}