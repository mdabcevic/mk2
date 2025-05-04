using Bartender.Domain.utility.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class AuthorizationExceptionHandler(ILogger<AuthorizationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (AuthorizationException or UnauthorizedAccessException))
            return false;

        var logMessage = exception is AuthorizationException authEx ? authEx.GetLogMessage() : exception.Message ?? exception.GetType().Name;
        var data = exception is AuthorizationException ? exception.Data["AdditionalData"] : null;

        logger.LogWarning(exception, logMessage);
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        var response = new ErrorResponse(exception.Message ?? exception.GetType().Name, StatusCodes.Status401Unauthorized, data);

        await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);

        return true;
    }
}
