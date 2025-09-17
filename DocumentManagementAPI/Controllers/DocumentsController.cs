using DocumentManagementAPI.Data;
using DocumentManagementAPI.DTOs;
using DocumentManagementAPI.Models;
using DocumentManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace DocumentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(ApplicationDbContext context, IFileService fileService, ILogger<DocumentsController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<DocumentDto>>> GetDocuments()
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                IQueryable<Document> query = _context.Documents
                    .Include(d => d.DocumentType)
                    .Include(d => d.UploadedBy)
                    .Include(d => d.Company);

                // Filter based on user role
                if (currentUserRole == "SuperAdmin")
                {
                    // SuperAdmin can see all documents
                }
                else if (currentUserRole == "CompanyAdmin")
                {
                    // CompanyAdmin can see all documents from their company
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        query = query.Where(d => d.CompanyId == companyId);
                    }
                }
                else // User
                {
                    // Regular users can only see their own documents
                    query = query.Where(d => d.UploadedById == currentUserId);
                }

                var documents = await query
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        OriginalName = d.OriginalName,
                        FileSize = d.FileSize,
                        FileExtension = d.FileExtension,
                        DocumentTypeId = d.DocumentTypeId,
                        DocumentTypeName = d.DocumentType.Name,
                        UploadedById = d.UploadedById,
                        UploadedByName = d.UploadedBy.FirstName + " " + d.UploadedBy.LastName,
                        CompanyId = d.CompanyId,
                        CompanyName = d.Company.Name,
                        UploadDate = d.UploadDate
                    })
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();

                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return StatusCode(500, new { message = "An error occurred while getting documents" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var query = _context.Documents
                    .Include(d => d.DocumentType)
                    .Include(d => d.UploadedBy)
                    .Include(d => d.Company)
                    .AsQueryable();

                // Filter based on user role
                if (currentUserRole == "SuperAdmin")
                {
                    // SuperAdmin can see all documents
                }
                else if (currentUserRole == "CompanyAdmin")
                {
                    // CompanyAdmin can see all documents from their company
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        query = query.Where(d => d.CompanyId == companyId);
                    }
                }
                else // User
                {
                    // Regular users can only see their own documents
                    query = query.Where(d => d.UploadedById == currentUserId);
                }

                var document = await query
                    .Where(d => d.Id == id)
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        OriginalName = d.OriginalName,
                        FileSize = d.FileSize,
                        FileExtension = d.FileExtension,
                        DocumentTypeId = d.DocumentTypeId,
                        DocumentTypeName = d.DocumentType.Name,
                        UploadedById = d.UploadedById,
                        UploadedByName = d.UploadedBy.FirstName + " " + d.UploadedBy.LastName,
                        CompanyId = d.CompanyId,
                        CompanyName = d.Company.Name,
                        UploadDate = d.UploadDate
                    })
                    .FirstOrDefaultAsync();

                if (document == null)
                {
                    return NotFound(new { message = "Document not found" });
                }

                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while getting the document" });
            }
        }

        [HttpPost("upload")]
        public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] UploadDocumentDto uploadDocumentDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserCompanyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");

                if (currentUserCompanyId == 0)
                {
                    return BadRequest(new { message = "User must belong to a company to upload documents" });
                }

                // Validate document type
                var documentType = await _context.DocumentTypes
                    .FirstOrDefaultAsync(dt => dt.Id == uploadDocumentDto.DocumentTypeId && dt.IsActive);

                if (documentType == null)
                {
                    return BadRequest(new { message = "Invalid document type" });
                }

                // Check if user can use this document type
                if (documentType.CompanyId.HasValue && documentType.CompanyId != currentUserCompanyId)
                {
                    return BadRequest(new { message = "You cannot use this document type" });
                }

                var file = uploadDocumentDto.File;

                // Validate file extension
                var allowedExtensions = JsonSerializer.Deserialize<List<string>>(documentType.AllowedExtensions) ?? new List<string>();
                if (!_fileService.IsValidFileExtension(file.FileName, allowedExtensions))
                {
                    return BadRequest(new { message = $"File extension not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}" });
                }

                // Validate file size
                if (!_fileService.IsValidFileSize(file.Length, documentType.MaxFileSize))
                {
                    return BadRequest(new { message = $"File size exceeds maximum allowed size of {documentType.MaxFileSize} MB" });
                }

                // Generate file path
                var ownerKey = $"user_{currentUserId}";
                var documentTypeKey = documentType.Name.Replace(" ", "_").ToLowerInvariant();
                var filePath = await _fileService.SaveFileAsync(file, currentUserCompanyId, ownerKey, documentTypeKey);

                // Create document record
                var document = new Document
                {
                    Name = Path.GetFileName(filePath),
                    OriginalName = file.FileName,
                    FileSize = Math.Round((decimal)file.Length / (1024 * 1024), 2), // Convert to MB
                    FileExtension = Path.GetExtension(file.FileName),
                    FilePath = filePath,
                    DocumentTypeId = uploadDocumentDto.DocumentTypeId,
                    UploadedById = currentUserId,
                    CompanyId = currentUserCompanyId,
                    UploadDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(document).Reference(d => d.DocumentType).LoadAsync();
                await _context.Entry(document).Reference(d => d.UploadedBy).LoadAsync();
                await _context.Entry(document).Reference(d => d.Company).LoadAsync();

                var documentDto = new DocumentDto
                {
                    Id = document.Id,
                    Name = document.Name,
                    OriginalName = document.OriginalName,
                    FileSize = document.FileSize,
                    FileExtension = document.FileExtension,
                    DocumentTypeId = document.DocumentTypeId,
                    DocumentTypeName = document.DocumentType.Name,
                    UploadedById = document.UploadedById,
                    UploadedByName = document.UploadedBy.FirstName + " " + document.UploadedBy.LastName,
                    CompanyId = document.CompanyId,
                    CompanyName = document.Company.Name,
                    UploadDate = document.UploadDate
                };

                _logger.LogInformation("Document {DocumentName} uploaded successfully by user {UserId}", document.OriginalName, currentUserId);
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, documentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new { message = "An error occurred while uploading the document" });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadDocument(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var query = _context.Documents.AsQueryable();

                // Filter based on user role
                if (currentUserRole == "SuperAdmin")
                {
                    // SuperAdmin can download all documents
                }
                else if (currentUserRole == "CompanyAdmin")
                {
                    // CompanyAdmin can download all documents from their company
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        query = query.Where(d => d.CompanyId == companyId);
                    }
                }
                else // User
                {
                    // Regular users can only download their own documents
                    query = query.Where(d => d.UploadedById == currentUserId);
                }

                var document = await query.FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                {
                    return NotFound(new { message = "Document not found" });
                }

                var (fileBytes, contentType, fileName) = await _fileService.GetFileAsync(document.FilePath);

                return File(fileBytes, contentType, document.OriginalName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { message = "File not found on disk" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while downloading the document" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var query = _context.Documents.AsQueryable();

                // Filter based on user role
                if (currentUserRole == "SuperAdmin")
                {
                    // SuperAdmin can delete all documents
                }
                else if (currentUserRole == "CompanyAdmin")
                {
                    // CompanyAdmin can delete all documents from their company
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        query = query.Where(d => d.CompanyId == companyId);
                    }
                }
                else // User
                {
                    // Regular users can only delete their own documents
                    query = query.Where(d => d.UploadedById == currentUserId);
                }

                var document = await query.FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                {
                    return NotFound(new { message = "Document not found" });
                }

                // Delete file from disk
                await _fileService.DeleteFileAsync(document.FilePath);

                // Delete document record
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document {DocumentId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the document" });
            }
        }

        [HttpPost("search")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<DocumentSearchResultDto>> SearchDocuments([FromBody] DocumentSearchDto searchDto)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                IQueryable<Document> query = _context.Documents
                    .Include(d => d.DocumentType)
                    .Include(d => d.UploadedBy)
                    .Include(d => d.Company);

                // Filter based on user role
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        query = query.Where(d => d.CompanyId == companyId);
                    }
                }

                // Apply search filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var searchTerm = searchDto.SearchTerm.ToLower();
                    query = query.Where(d => 
                        d.OriginalName.ToLower().Contains(searchTerm) ||
                        d.DocumentType.Name.ToLower().Contains(searchTerm) ||
                        (d.UploadedBy.FirstName + " " + d.UploadedBy.LastName).ToLower().Contains(searchTerm));
                }

                if (searchDto.DocumentTypeId.HasValue)
                {
                    query = query.Where(d => d.DocumentTypeId == searchDto.DocumentTypeId.Value);
                }

                if (searchDto.CompanyId.HasValue && currentUserRole == "SuperAdmin")
                {
                    query = query.Where(d => d.CompanyId == searchDto.CompanyId.Value);
                }

                if (searchDto.DateFrom.HasValue)
                {
                    query = query.Where(d => d.UploadDate >= searchDto.DateFrom.Value);
                }

                if (searchDto.DateTo.HasValue)
                {
                    var dateTo = searchDto.DateTo.Value.AddDays(1); // Include the entire day
                    query = query.Where(d => d.UploadDate < dateTo);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

                var documents = await query
                    .OrderByDescending(d => d.UploadDate)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        OriginalName = d.OriginalName,
                        FileSize = d.FileSize,
                        FileExtension = d.FileExtension,
                        DocumentTypeId = d.DocumentTypeId,
                        DocumentTypeName = d.DocumentType.Name,
                        UploadedById = d.UploadedById,
                        UploadedByName = d.UploadedBy.FirstName + " " + d.UploadedBy.LastName,
                        CompanyId = d.CompanyId,
                        CompanyName = d.Company.Name,
                        UploadDate = d.UploadDate
                    })
                    .ToListAsync();

                var result = new DocumentSearchResultDto
                {
                    Documents = documents,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents");
                return StatusCode(500, new { message = "An error occurred while searching documents" });
            }
        }
    }
}