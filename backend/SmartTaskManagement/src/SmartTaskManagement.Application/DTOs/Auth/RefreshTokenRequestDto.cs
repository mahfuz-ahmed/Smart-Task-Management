namespace SmartTaskManagement.Application.DTOs.Auth;

public sealed record RefreshTokenRequestDto(
    string AccessToken,
    string RefreshToken
);
