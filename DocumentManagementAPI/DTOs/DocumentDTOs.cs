using System.ComponentModel.DataAnnotations;

namespace DocumentManagementAPI.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public decimal FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public int UploadedById { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
    }

    public class UploadDocumentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public int DocumentTypeId { get; set; }
    }

    public class DocumentSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? DocumentTypeId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 0; // 0 means use default
    }

    public class DocumentSearchResultDto
    {
        public List<DocumentDto> Documents { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}