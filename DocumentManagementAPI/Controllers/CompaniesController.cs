using DocumentManagementAPI.Data;
using DocumentManagementAPI.DTOs;
using DocumentManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocumentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(ApplicationDbContext context, ILogger<CompaniesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<List<CompanyDto>>> GetCompanies()
        {
            try
            {
                var companies = await _context.Companies
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        TaxNumber = c.TaxNumber,
                        Address = c.Address,
                        Phone = c.Phone,
                        Email = c.Email,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies");
                return StatusCode(500, new { message = "An error occurred while getting companies" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            try
            {
                var company = await _context.Companies
                    .Where(c => c.Id == id)
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        TaxNumber = c.TaxNumber,
                        Address = c.Address,
                        Phone = c.Phone,
                        Email = c.Email,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company {CompanyId}", id);
                return StatusCode(500, new { message = "An error occurred while getting the company" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CreateCompanyDto createCompanyDto)
        {
            try
            {
                // Check if company with same tax number or email exists
                var existingCompany = await _context.Companies
                    .AnyAsync(c => c.TaxNumber == createCompanyDto.TaxNumber || c.Email == createCompanyDto.Email);

                if (existingCompany)
                {
                    return BadRequest(new { message = "Company with same tax number or email already exists" });
                }

                var company = new Company
                {
                    Name = createCompanyDto.Name,
                    TaxNumber = createCompanyDto.TaxNumber,
                    Address = createCompanyDto.Address,
                    Phone = createCompanyDto.Phone,
                    Email = createCompanyDto.Email,
                    IsActive = createCompanyDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var companyDto = new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    TaxNumber = company.TaxNumber,
                    Address = company.Address,
                    Phone = company.Phone,
                    Email = company.Email,
                    IsActive = company.IsActive,
                    CreatedAt = company.CreatedAt
                };

                _logger.LogInformation("Company {CompanyName} created successfully", company.Name);
                return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, companyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                return StatusCode(500, new { message = "An error occurred while creating the company" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<CompanyDto>> UpdateCompany(int id, [FromBody] UpdateCompanyDto updateCompanyDto)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                // Check if another company with same tax number or email exists
                var existingCompany = await _context.Companies
                    .AnyAsync(c => c.Id != id && (c.TaxNumber == updateCompanyDto.TaxNumber || c.Email == updateCompanyDto.Email));

                if (existingCompany)
                {
                    return BadRequest(new { message = "Another company with same tax number or email already exists" });
                }

                company.Name = updateCompanyDto.Name;
                company.TaxNumber = updateCompanyDto.TaxNumber;
                company.Address = updateCompanyDto.Address;
                company.Phone = updateCompanyDto.Phone;
                company.Email = updateCompanyDto.Email;
                company.IsActive = updateCompanyDto.IsActive;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var companyDto = new CompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    TaxNumber = company.TaxNumber,
                    Address = company.Address,
                    Phone = company.Phone,
                    Email = company.Email,
                    IsActive = company.IsActive,
                    CreatedAt = company.CreatedAt
                };

                _logger.LogInformation("Company {CompanyId} updated successfully", id);
                return Ok(companyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the company" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult> DeleteCompany(int id)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                // Check if company has users or documents
                var hasUsers = await _context.Users.AnyAsync(u => u.CompanyId == id);
                var hasDocuments = await _context.Documents.AnyAsync(d => d.CompanyId == id);

                if (hasUsers || hasDocuments)
                {
                    return BadRequest(new { message = "Cannot delete company with existing users or documents" });
                }

                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Company {CompanyId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {CompanyId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the company" });
            }
        }
    }
}