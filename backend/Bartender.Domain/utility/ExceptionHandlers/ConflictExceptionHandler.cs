
using Bartender.Domain.utility.Exceptions.ConflictException;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ConflictException conflictException)
        {
            logger.LogError(exception, conflictException.GetLogMessage());

            var response = new ErrorResponse(exception.Message, StatusCodes.Status409Conflict, conflictException.GetData());

            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            return true;
        }

        return false;
    }
}
