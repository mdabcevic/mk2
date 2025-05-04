using Bartender.Domain.utility.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (AppValidationException or ValidationException or ArgumentNullException or InvalidOperationException))
            return false;

        var logMessage = exception is AppValidationException appEx ? appEx.GetLogMessage() : exception.Message ?? exception.GetType().Name;
        var data = exception is AppValidationException ? exception.Data["AdditionalData"] : null;

        logger.LogError(exception, logMessage);
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new ErrorResponse(exception.Message ?? exception.GetType().Name, StatusCodes.Status400BadRequest, data);

        await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);

        return true;
    }
}
