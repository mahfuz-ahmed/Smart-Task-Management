using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Extensions;
using SmartTaskManagement.Application.DTOs.AI;
using SmartTaskManagement.Application.Interfaces.ExternalServices;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
[Produces("application/json")]
public sealed class AiController : BaseController
{
    private readonly IAiService _aiService;
    private readonly IValidator<ImproveDescriptionDto> _validator;

    public AiController(IAiService aiService, IValidator<ImproveDescriptionDto> validator)
    {
        _aiService = aiService;
        _validator = validator;
    }

    [HttpPost("improve-description")]
    [ProducesResponseType(typeof(ApiResponse<ImproveDescriptionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImproveDescription([FromBody] ImproveDescriptionDto dto, CancellationToken cancellationToken)
    {
        var sanitizedRequest = dto with
        {
            Description = dto.Description?.SanitizeDescription() ?? string.Empty,
            TaskTitle = dto.TaskTitle?.SanitizeTitle() ?? string.Empty
        };

        var validationResponse = await ValidateAsync(sanitizedRequest, _validator, cancellationToken);

        if (validationResponse is not null)
            return validationResponse;

        var improvedDescription = await _aiService.ImproveDescriptionAsync(sanitizedRequest.Description, sanitizedRequest.TaskTitle, cancellationToken);

        var response = new ImproveDescriptionResponseDto(sanitizedRequest.Description, improvedDescription);

        return Ok(ApiResponse<ImproveDescriptionResponseDto>.Ok(response, "Description improved successfully."));
    }
}