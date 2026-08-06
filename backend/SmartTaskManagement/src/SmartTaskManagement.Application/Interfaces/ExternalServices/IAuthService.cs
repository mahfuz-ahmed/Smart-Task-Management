using SmartTaskManagement.Application.DTOs.Auth;

namespace SmartTaskManagement.Application.Interfaces.ExternalServices;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);

    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);

    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);

    Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct = default);
}