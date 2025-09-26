using DocumentManagementAPI.DTOs;
using DocumentManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DocumentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation("Login request received for user: {Username}", loginRequest.Username);
                
                var result = await _authService.LoginAsync(loginRequest);
                
                if (result == null)
                {
                    _logger.LogWarning("Login failed for user: {Username}", loginRequest.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                _logger.LogInformation("User {Username} logged in successfully", loginRequest.Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginRequest.Username);
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}