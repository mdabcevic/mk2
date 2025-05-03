using Bartender.Domain.utility.Exceptions.NotFoundException;
using Bartender.Domain.utility.Exceptions.ValidationException;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is NotFoundException notFoundException)
        {
            logger.LogError(exception, notFoundException.GetLogMessage());

            var additionalData = notFoundException.Data["AdditionalData"];
            var response = new ErrorResponse(exception.Message, StatusCodes.Status404NotFound, additionalData);

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken); 

            return true;
        }

        return false;
    }
}
