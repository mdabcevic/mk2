using Bartender.Domain.utility.Exceptions.NotFoundException;
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

            var response = new ErrorResponse(exception.Message, StatusCodes.Status404NotFound, exception.Data);

            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

            return true;
        }

        return false;
    }
}
