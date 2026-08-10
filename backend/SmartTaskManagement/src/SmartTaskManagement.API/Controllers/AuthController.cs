using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Constants;
using SmartTaskManagement.Application.DTOs.Auth;
using SmartTaskManagement.Application.Interfaces.ExternalServices;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;

    public AuthController(IAuthService authService, IValidator<RegisterDto> registerValidator, IValidator<LoginDto> loginValidator, IValidator<RefreshTokenRequestDto> refreshTokenValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _registerValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _authService.RegisterAsync(dto, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponseDto>.Ok(result, SuccessMessages.Created));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _loginValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _authService.LoginAsync(dto, cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, SuccessMessages.LogIn));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(GetCurrentUserId(), dto.RefreshToken, cancellationToken);

        return Ok(ApiResponse.Ok(SuccessMessages.LoggedOut));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _refreshTokenValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _authService.RefreshTokenAsync(dto, cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, SuccessMessages.TokenRefreshed));
    }
}