using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SmartTaskManagement.API.Controllers
{
    /// <summary>
    /// Base controller providing shared validation helper.
    /// </summary>
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Validates a request DTO using the supplied FluentValidation validator.
        /// Returns a tuple indicating whether validation succeeded and an optional BadRequest result.
        /// </summary>
        protected async Task<(bool IsValid, IActionResult? Result)> ValidateRequestAsync<T>(
            T request,
            IValidator<T> validator,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return (false, BadRequest(ApiResponse<object>.Fail(errors)));
            }
            return (true, null);
        }
    }
}
