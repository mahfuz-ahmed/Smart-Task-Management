using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Constants;
using SmartTaskManagement.Application.Exceptions;

namespace SmartTaskManagement.API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected async Task<IActionResult?> ValidateAsync<T>(T dto, IValidator<T> validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);

        if (validationResult.IsValid)return null;

        return BadRequest(ApiResponse.Fail(ErrorMessages.ValidationFailed, validationResult.Errors.Select(x => x.ErrorMessage), errorCode: ErrorCodes.Validation));
    }

    protected Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))throw new UnauthorizedException("Invalid user authentication context.");

        return userId;
    }

    protected IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(x => x.Value);
    }
}