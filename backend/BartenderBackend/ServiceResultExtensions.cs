using Bartender.Domain;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult<T>(this ServiceResult<T> result)
    {
        if (result.Success && result.Data is not null)
            return new OkObjectResult(result.Data);

        object response = result.Data switch
        {
            null => new { error = result.Error },
            _ => new { error = result.Error, data = result.Data }
        };

        return result.errorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(new { response }),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(new { response }),
            ErrorType.Conflict => new ConflictObjectResult(new { response }),
            ErrorType.Validation => new BadRequestObjectResult(new { response }),
            _ => new ObjectResult(new { response }) { StatusCode = 500 }
        };
    }

    public static IActionResult ToActionResult(this ServiceResult result)
    {
        if (result.Success)
            return new NoContentResult();

        return result.errorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(new { error = result.Error }),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(new { error = result.Error }),
            ErrorType.Conflict => new ConflictObjectResult(new { error = result.Error }),
            ErrorType.Validation => new BadRequestObjectResult(new { error = result.Error }),
            _ => new ObjectResult(new { error = result.Error }) { StatusCode = 500 }
        };
    }
}
