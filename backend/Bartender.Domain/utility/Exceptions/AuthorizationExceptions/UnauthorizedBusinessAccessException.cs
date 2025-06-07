namespace Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;

public class UnauthorizedBusinessAccessException(int? businessId = null) : AuthorizationException($"Access to business denied")
{
    public int? BusinessId { get; } = businessId;

    public override string GetLogMessage()
    {
        return BusinessId.HasValue ?
            $"Unauthorized attempt to access business with ID {BusinessId}" :
            $"Cross-entity request detected";
    }
}
