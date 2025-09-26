using DocumentManagementAPI.Data;
using DocumentManagementAPI.DTOs;
using DocumentManagementAPI.Models;
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
    public class DocumentTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentTypesController> _logger;

        public DocumentTypesController(ApplicationDbContext context, ILogger<DocumentTypesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,User")]
        public async Task<ActionResult<List<DocumentTypeDto>>> GetDocumentTypes()
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                IQueryable<DocumentType> query = _context.DocumentTypes.Include(dt => dt.Company);

                // SuperAdmin can see all document types
                // CompanyAdmin and User can see global types (companyId = null) and their company's types
                if (currentUserRole != "SuperAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        query = query.Where(dt => dt.CompanyId == null || dt.CompanyId == companyId);
                    }
                    else
                    {
                        query = query.Where(dt => dt.CompanyId == null);
                    }
                }

                var documentTypes = await query
                    .Where(dt => dt.IsActive)
                    .Select(dt => new DocumentTypeDto
                    {
                        Id = dt.Id,
                        Name = dt.Name,
                        Description = dt.Description,
                        AllowedExtensions = JsonSerializer.Deserialize<List<string>>(dt.AllowedExtensions) ?? new List<string>(),
                        MaxFileSize = dt.MaxFileSize,
                        CompanyId = dt.CompanyId,
                        CompanyName = dt.Company != null ? dt.Company.Name : null,
                        IsActive = dt.IsActive,
                        CreatedAt = dt.CreatedAt
                    })
                    .ToListAsync();

                return Ok(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document types");
                return StatusCode(500, new { message = "An error occurred while getting document types" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<DocumentTypeDto>> GetDocumentType(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                var query = _context.DocumentTypes.Include(dt => dt.Company).AsQueryable();

                // CompanyAdmin can only see their company's document types and global ones
                if (currentUserRole == "CompanyAdmin" && int.TryParse(currentUserCompanyId, out var companyId))
                {
                    query = query.Where(dt => dt.CompanyId == null || dt.CompanyId == companyId);
                }

                var documentType = await query
                    .Where(dt => dt.Id == id)
                    .Select(dt => new DocumentTypeDto
                    {
                        Id = dt.Id,
                        Name = dt.Name,
                        Description = dt.Description,
                        AllowedExtensions = JsonSerializer.Deserialize<List<string>>(dt.AllowedExtensions) ?? new List<string>(),
                        MaxFileSize = dt.MaxFileSize,
                        CompanyId = dt.CompanyId,
                        CompanyName = dt.Company != null ? dt.Company.Name : null,
                        IsActive = dt.IsActive,
                        CreatedAt = dt.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (documentType == null)
                {
                    return NotFound(new { message = "Document type not found" });
                }

                return Ok(documentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document type {DocumentTypeId}", id);
                return StatusCode(500, new { message = "An error occurred while getting the document type" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<DocumentTypeDto>> CreateDocumentType([FromBody] CreateDocumentTypeDto createDocumentTypeDto)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                _logger.LogInformation("Creating document type: {Name} by user role: {Role}", createDocumentTypeDto.Name, currentUserRole);

                // CompanyAdmin can only create document types for their company
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        createDocumentTypeDto.CompanyId = companyId;
                    }
                    else
                    {
                        _logger.LogWarning("CompanyAdmin without valid company ID trying to create document type");
                        return BadRequest(new { message = "Invalid company information" });
                    }
                }

                // Validate company exists if specified
                if (createDocumentTypeDto.CompanyId.HasValue)
                {
                    var companyExists = await _context.Companies.AnyAsync(c => c.Id == createDocumentTypeDto.CompanyId.Value);
                    if (!companyExists)
                    {
                        _logger.LogWarning("Document type creation failed: Invalid company ID {CompanyId}", createDocumentTypeDto.CompanyId);
                        return BadRequest(new { message = "Invalid company" });
                    }
                }

                // Check if document type with same name exists for the same company
                var existingDocumentType = await _context.DocumentTypes
                    .Where(dt => dt.Name == createDocumentTypeDto.Name && dt.CompanyId == createDocumentTypeDto.CompanyId)
                    .AnyAsync();

                if (existingDocumentType)
                {
                    _logger.LogWarning("Document type creation failed: Duplicate name {Name} for company {CompanyId}", createDocumentTypeDto.Name, createDocumentTypeDto.CompanyId);
                    return BadRequest(new { message = "Document type with same name already exists for this company" });
                }

                var documentType = new DocumentType
                {
                    Name = createDocumentTypeDto.Name,
                    Description = createDocumentTypeDto.Description,
                    AllowedExtensions = JsonSerializer.Serialize(createDocumentTypeDto.AllowedExtensions),
                    MaxFileSize = createDocumentTypeDto.MaxFileSize,
                    CompanyId = createDocumentTypeDto.CompanyId,
                    IsActive = createDocumentTypeDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Adding document type to context: {Name}", documentType.Name);
                _context.DocumentTypes.Add(documentType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Document type saved successfully with ID: {Id}", documentType.Id);

                // Load company name for response
                await _context.Entry(documentType).Reference(dt => dt.Company).LoadAsync();

                var documentTypeDto = new DocumentTypeDto
                {
                    Id = documentType.Id,
                    Name = documentType.Name,
                    Description = documentType.Description,
                    AllowedExtensions = createDocumentTypeDto.AllowedExtensions,
                    MaxFileSize = documentType.MaxFileSize,
                    CompanyId = documentType.CompanyId,
                    CompanyName = documentType.Company?.Name,
                    IsActive = documentType.IsActive,
                    CreatedAt = documentType.CreatedAt
                };

                _logger.LogInformation("Document type {DocumentTypeName} created successfully", documentType.Name);
                return CreatedAtAction(nameof(GetDocumentType), new { id = documentType.Id }, documentTypeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document type");
                return StatusCode(500, new { message = "An error occurred while creating the document type" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<DocumentTypeDto>> UpdateDocumentType(int id, [FromBody] UpdateDocumentTypeDto updateDocumentTypeDto)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                var documentType = await _context.DocumentTypes.Include(dt => dt.Company).FirstOrDefaultAsync(dt => dt.Id == id);
                if (documentType == null)
                {
                    return NotFound(new { message = "Document type not found" });
                }

                // CompanyAdmin can only update their company's document types
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        if (documentType.CompanyId != companyId)
                        {
                            return Forbid();
                        }
                    }
                }

                // Check if another document type with same name exists for the same company
                var existingDocumentType = await _context.DocumentTypes
                    .Where(dt => dt.Id != id && dt.Name == updateDocumentTypeDto.Name && dt.CompanyId == documentType.CompanyId)
                    .AnyAsync();

                if (existingDocumentType)
                {
                    return BadRequest(new { message = "Another document type with same name already exists for this company" });
                }

                documentType.Name = updateDocumentTypeDto.Name;
                documentType.Description = updateDocumentTypeDto.Description;
                documentType.AllowedExtensions = JsonSerializer.Serialize(updateDocumentTypeDto.AllowedExtensions);
                documentType.MaxFileSize = updateDocumentTypeDto.MaxFileSize;
                documentType.IsActive = updateDocumentTypeDto.IsActive;
                documentType.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var documentTypeDto = new DocumentTypeDto
                {
                    Id = documentType.Id,
                    Name = documentType.Name,
                    Description = documentType.Description,
                    AllowedExtensions = updateDocumentTypeDto.AllowedExtensions,
                    MaxFileSize = documentType.MaxFileSize,
                    CompanyId = documentType.CompanyId,
                    CompanyName = documentType.Company?.Name,
                    IsActive = documentType.IsActive,
                    CreatedAt = documentType.CreatedAt
                };

                _logger.LogInformation("Document type {DocumentTypeId} updated successfully", id);
                return Ok(documentTypeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document type {DocumentTypeId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the document type" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult> DeleteDocumentType(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                var documentType = await _context.DocumentTypes.FindAsync(id);
                if (documentType == null)
                {
                    return NotFound(new { message = "Document type not found" });
                }

                // CompanyAdmin can only delete their company's document types
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        if (documentType.CompanyId != companyId)
                        {
                            return Forbid();
                        }
                    }
                }

                // Check if document type has documents
                var hasDocuments = await _context.Documents.AnyAsync(d => d.DocumentTypeId == id);
                if (hasDocuments)
                {
                    return BadRequest(new { message = "Cannot delete document type with existing documents" });
                }

                _context.DocumentTypes.Remove(documentType);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document type {DocumentTypeId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document type {DocumentTypeId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the document type" });
            }
        }
    }
}