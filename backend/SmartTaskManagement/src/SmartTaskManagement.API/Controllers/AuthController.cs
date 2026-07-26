
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Auth;
using SmartTaskManagement.Application.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IValidator<RegisterDto> _registerVal;
    private readonly IValidator<LoginDto> _loginVal;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenVal;

    public AuthController(IAuthService auth,
        IValidator<RegisterDto> registerVal,
        IValidator<LoginDto> loginVal,
        IValidator<RefreshTokenRequestDto> refreshTokenVal)
    {
        _auth = auth;
        _registerVal = registerVal;
        _loginVal = loginVal;
        _refreshTokenVal = refreshTokenVal;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var (isValid, errorResponse) = await ValidateRequestAsync(dto, _registerVal, ct);
        if (!isValid) return errorResponse;

        var result = await _auth.RegisterAsync(dto, ct);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponseDto>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var v = await _loginVal.ValidateAsync(dto, ct);
        if (!v.IsValid) return BadRequest(ApiResponse<object>.Fail(v.Errors.Select(e => e.ErrorMessage)));

        var result = await _auth.LoginAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid user context"));

        await _auth.LogoutAsync(userId, dto.RefreshToken, ct);
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto,CancellationToken ct)
    {
        var (isValid, errorResponse) =await ValidateRequestAsync(dto, _refreshTokenVal, ct);

        if (!isValid)return errorResponse!;

        var result = await _auth.RefreshTokenAsync(dto, ct);

        return Ok(ApiResponse<AuthResponseDto>.Ok(result,"Token refreshed."));
    }

    private async Task<(bool IsValid, IActionResult? Response)> ValidateRequestAsync<T>(
        T dto, IValidator<T> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(dto, ct);
        if (!result.IsValid)
            return (false, BadRequest(ApiResponse<object>.Fail(
                result.Errors.Select(e => e.ErrorMessage))));
        return (true, null);
    }
}