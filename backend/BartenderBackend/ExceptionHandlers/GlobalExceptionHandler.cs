using Bartender.Domain.utility.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ErrorResponse response;

        if (exception is UnknownErrorException unknownError)
        {
            var additionalData = unknownError.Data["AdditionalData"];
            response = new ErrorResponse(unknownError.Message ?? "An unexpected error occurred.", StatusCodes.Status500InternalServerError, additionalData);
            string logMessage = string.IsNullOrEmpty(unknownError.Message)
                ? "An unknown error occurred."
                : unknownError.Message;
            logger.LogError(exception, logMessage);
        }
        else
        {
            logger.LogError(exception, "An unexpected error occurred.");
            response = new ErrorResponse()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Error = "An unexpected error occurred. Please try again later."
            };
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new { response }, cancellationToken);  

        return true;
    }
}
