using AutomationExercise.Core.DTOs;

namespace AutomationExercise.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<UserResponseDto> LoginAsync(LoginDto loginDto);
    }
}