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
        if (exception is AppValidationException appValidationException)
        {
            logger.LogError(exception, appValidationException.GetLogMessage());

            var additionalData = appValidationException.Data["AdditionalData"];
            var response = new ErrorResponse(exception.Message, StatusCodes.Status400BadRequest, additionalData);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            
            return true;
        }
        else if (exception is ValidationException || exception is ArgumentNullException || exception is InvalidOperationException)
        {
            logger.LogError(exception, exception.Message ?? exception.GetType().Name);
            var response = new ErrorResponse(exception.Message ?? exception.GetType().Name, StatusCodes.Status400BadRequest);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);

            return true;
        }

        return false;
    }
}
