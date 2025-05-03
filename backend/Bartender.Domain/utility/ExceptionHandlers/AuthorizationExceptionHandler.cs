
using Bartender.Domain.utility.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class AuthorizationExceptionHandler(ILogger<AuthorizationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AuthorizationException authorizationException)
        {
            logger.LogError(exception, authorizationException.GetLogMessage());

            var additionalData = authorizationException.Data["AdditionalData"];
            var response = new ErrorResponse(exception.Message, StatusCodes.Status401Unauthorized, additionalData);

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);
            

            return true;
        }

        return false;
    }
}
