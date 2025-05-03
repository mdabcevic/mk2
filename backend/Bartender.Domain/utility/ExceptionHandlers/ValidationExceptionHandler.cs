using Bartender.Domain.utility.Exceptions.NotFoundException;
using Bartender.Domain.utility.Exceptions.ValidationException;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AppValidationException appValidationException)
        {
            var additionalData = appValidationException.Data["AdditionalData"];
            var response = new ErrorResponse(exception.Message, StatusCodes.Status400BadRequest, additionalData);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            
            return true;
        }

        return false;
    }
}
