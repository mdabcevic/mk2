using Bartender.Domain.utility.Exceptions.ValidationException;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AppValidationException)
        {
            var response = new ErrorResponse(exception.Message, StatusCodes.Status400BadRequest, exception.Data);

            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return true;
        }

        return false;
    }
}
