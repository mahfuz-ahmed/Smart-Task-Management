using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManagement.Application.DTOs.Auth;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces.ExternalServices;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _uow;

    public AuthService(IUnitOfWork uow,

        IConfiguration config, ILogger<AuthService> logger)
    {
        _uow = uow;
        _config = config;

        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        _logger.LogInformation("User logged in: {Email}", user.Email);
        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task LogoutAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var token = await _uow.RefreshTokens.GetActiveTokenAsync(refreshToken, ct);
            if (token != null && token.UserId == userId)
            {
                token.IsRevoked = true;
                token.LastModifiedAtUtc = DateTime.UtcNow;
                _uow.RefreshTokens.Update(token);
                await _uow.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("User logged out: {UserId}", userId);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        // 1. Guard against Null / Empty values from client request body
        if (dto == null || string.IsNullOrWhiteSpace(dto.AccessToken) || string.IsNullOrWhiteSpace(dto.RefreshToken))
            throw new UnauthorizedException("Access token and Refresh token are required.");

        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);

        var jwtId = principal.FindFirstValue(JwtRegisteredClaimNames.Jti)
            ?? throw new UnauthorizedException("Invalid token payload.");

        var storedToken = await _uow.RefreshTokens.GetActiveTokenAsync(dto.RefreshToken, ct)
            ?? throw new UnauthorizedException("Refresh token is invalid or expired.");

        if (storedToken.JwtId != jwtId)
            throw new UnauthorizedException("Token mismatch.");

        // Safe User Navigation check
        var user = storedToken.User ?? await _uow.Users.GetByIdAsync(storedToken.UserId, ct)
            ?? throw new UnauthorizedException("User associated with this token no longer exists.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        // Single-use Token Rotation
        storedToken.IsUsed = true;
        storedToken.LastModifiedAtUtc = DateTime.UtcNow;
        _uow.RefreshTokens.Update(storedToken);

        return await BuildAuthResponseAsync(user, ct);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var normalizedEmail = dto.Email.Trim();

        if (await _uow.Users.EmailExistsAsync(normalizedEmail, ct))
            throw new ConflictException($"Email '{dto.Email}' is already registered.");

        // Prevent self-assignment of Admin role
        if (dto.Role == UserRole.Admin)
            throw new BusinessException("Admin role cannot be " +
                "self-assigned during" +
                "                          registration.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role, // Use role from registration form
            IsActive = true
        };

        await _uow.Users.AddAsync(user, ct);
        var response = await BuildAuthResponseAsync(user, ct);

        _logger.LogInformation("User registered successfully: {Email} with role {Role}", user.Email, user.Role);
        return response;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user, CancellationToken ct)
    {
        var jwtId = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddMinutes(GetExpiryMinutes());
        var accessToken = GenerateJwt(user, jwtId, expiry);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, jwtId, ct);

        // Commit all changes in a single database transaction
        await _uow.SaveChangesAsync(ct);

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            expiry,
            new UserProfileDto(user.Id, user.FirstName, user.LastName,
                               user.FullName, user.Email, user.Role.ToString())
        );
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId, string jwtId, CancellationToken ct)
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var tokenString = Convert.ToBase64String(bytes);

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenString,
            JwtId = jwtId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        await _uow.RefreshTokens.AddAsync(token, ct);
        return tokenString;
    }

    private string GenerateJwt(User user, string jwtId, DateTime expiry)
    {
        var secretKey = _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,       user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email,     user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName,user.LastName),
            new(JwtRegisteredClaimNames.Jti,       jwtId),
            new(ClaimTypes.NameIdentifier,         user.Id.ToString()),
            new(ClaimTypes.Role,                   user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetExpiryMinutes() =>
        int.TryParse(_config["JwtSettings:ExpiryMinutes"], out var m) ? m : 15;

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // PREVENT System.ArgumentNullException (IDX10000)
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedException("Token parameter cannot be null or empty.");

        var secretKey = _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = _config["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _config["JwtSettings:Audience"],
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token,
                        parameters, out
                            var secToken);

            if (secToken is not JwtSecurityToken jwt ||
                                !jwt.Header.Alg.
                Equals(SecurityAlgorithms.
                            HmacSha256, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedException("Invalid token format.");

            return principal;
        }
        catch (Exception ex) when (ex is not UnauthorizedException)
        {
            throw new UnauthorizedException("Invalid access token format or structure.");
        }
    }
}