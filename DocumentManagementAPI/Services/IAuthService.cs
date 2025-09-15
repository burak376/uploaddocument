using DocumentManagementAPI.DTOs;

namespace DocumentManagementAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        string GenerateJwtToken(UserDto user);
    }
}