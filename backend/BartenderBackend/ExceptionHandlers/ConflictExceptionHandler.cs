
using Bartender.Domain.Utility.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ConflictException conflictException)
        {
            logger.LogError(exception, conflictException.GetLogMessage());

            var additionalData = conflictException.Data["AdditionalData"];
            var response = new ErrorResponse(exception.Message, StatusCodes.Status409Conflict, additionalData);

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            
            return true;
        }

        return false;
    }
}
