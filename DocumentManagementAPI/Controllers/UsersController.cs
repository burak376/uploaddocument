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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                IQueryable<User> query = _context.Users.Include(u => u.Company);

                // CompanyAdmin can only see users from their company
                if (currentUserRole == "CompanyAdmin" && int.TryParse(currentUserCompanyId, out var companyId))
                {
                    query = query.Where(u => u.CompanyId == companyId);
                }

                var users = await query
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role.ToString(),
                        CompanyId = u.CompanyId,
                        CompanyName = u.Company != null ? u.Company.Name : null,
                        IsActive = u.IsActive
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "An error occurred while getting users" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                var query = _context.Users.Include(u => u.Company).AsQueryable();

                // CompanyAdmin can only see users from their company
                if (currentUserRole == "CompanyAdmin" && int.TryParse(currentUserCompanyId, out var companyId))
                {
                    query = query.Where(u => u.CompanyId == companyId);
                }

                var user = await query
                    .Where(u => u.Id == id)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Role = u.Role.ToString(),
                        CompanyId = u.CompanyId,
                        CompanyName = u.Company != null ? u.Company.Name : null,
                        IsActive = u.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while getting the user" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;

                // CompanyAdmin can only create users for their company
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        createUserDto.CompanyId = companyId;
                        // CompanyAdmin cannot create SuperAdmin or CompanyAdmin users
                        if (createUserDto.Role != UserRole.User)
                        {
                            return BadRequest(new { message = "Company admin can only create regular users" });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Invalid company information" });
                    }
                }

                // Check if user with same username or email exists
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Username == createUserDto.Username || u.Email == createUserDto.Email);

                if (existingUser)
                {
                    return BadRequest(new { message = "User with same username or email already exists" });
                }

                // Validate company exists if specified
                if (createUserDto.CompanyId.HasValue)
                {
                    var companyExists = await _context.Companies.AnyAsync(c => c.Id == createUserDto.CompanyId.Value);
                    if (!companyExists)
                    {
                        return BadRequest(new { message = "Invalid company" });
                    }
                }

                var user = new User
                {
                    Username = createUserDto.Username,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Email = createUserDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                    Role = createUserDto.Role,
                    CompanyId = createUserDto.CompanyId,
                    IsActive = createUserDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Load company name for response
                await _context.Entry(user).Reference(u => u.Company).LoadAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.Name,
                    IsActive = user.IsActive
                };

                _logger.LogInformation("User {Username} created successfully", user.Username);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "An error occurred while creating the user" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var user = await _context.Users.Include(u => u.Company).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // CompanyAdmin can only update users from their company
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        if (user.CompanyId != companyId)
                        {
                            return Forbid();
                        }
                        updateUserDto.CompanyId = companyId;
                        // CompanyAdmin cannot update to SuperAdmin or CompanyAdmin roles
                        if (updateUserDto.Role != UserRole.User)
                        {
                            return BadRequest(new { message = "Company admin can only manage regular users" });
                        }
                    }
                }

                // Users cannot update themselves to avoid lockout
                if (currentUserId == id && !updateUserDto.IsActive)
                {
                    return BadRequest(new { message = "Cannot deactivate your own account" });
                }

                // Check if another user with same username or email exists
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Id != id && (u.Username == updateUserDto.Username || u.Email == updateUserDto.Email));

                if (existingUser)
                {
                    return BadRequest(new { message = "Another user with same username or email already exists" });
                }

                // Validate company exists if specified
                if (updateUserDto.CompanyId.HasValue)
                {
                    var companyExists = await _context.Companies.AnyAsync(c => c.Id == updateUserDto.CompanyId.Value);
                    if (!companyExists)
                    {
                        return BadRequest(new { message = "Invalid company" });
                    }
                }

                user.Username = updateUserDto.Username;
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.Email = updateUserDto.Email;
                user.Role = updateUserDto.Role;
                user.CompanyId = updateUserDto.CompanyId;
                user.IsActive = updateUserDto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload company information
                await _context.Entry(user).Reference(u => u.Company).LoadAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.Name,
                    IsActive = user.IsActive
                };

                _logger.LogInformation("User {UserId} updated successfully", id);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the user" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserCompanyId = User.FindFirst("CompanyId")?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Users cannot delete themselves
                if (currentUserId == id)
                {
                    return BadRequest(new { message = "Cannot delete your own account" });
                }

                // CompanyAdmin can only delete users from their company
                if (currentUserRole == "CompanyAdmin")
                {
                    if (int.TryParse(currentUserCompanyId, out var companyId))
                    {
                        if (user.CompanyId != companyId)
                        {
                            return Forbid();
                        }
                    }
                }

                // Check if user has documents
                var hasDocuments = await _context.Documents.AnyAsync(d => d.UploadedById == id);
                if (hasDocuments)
                {
                    return BadRequest(new { message = "Cannot delete user with existing documents" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the user" });
            }
        }

        [HttpPost("{id}/change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only change their own password, admins can change any password
                if (currentUserId != id && currentUserRole != "SuperAdmin" && currentUserRole != "CompanyAdmin")
                {
                    return Forbid();
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // If changing own password, verify current password
                if (currentUserId == id)
                {
                    if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                    {
                        return BadRequest(new { message = "Current password is incorrect" });
                    }
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed for user {UserId}", id);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while changing the password" });
            }
        }

        [HttpPost("{id}/admin-change-password")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult> AdminChangePassword(int id, [FromBody] AdminChangePasswordDto adminChangePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminChangePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed by admin for user {UserId}", id);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password by admin for user {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while changing the password" });
            }
        }
    }
}