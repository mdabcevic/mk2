
namespace Bartender.Domain.utility.Exceptions;

public class UnauthorizedBusinessAccessException : AuthorizationException
{
    public int? BusinessId { get; }
    public UnauthorizedBusinessAccessException(int? businessId = null)
        : base($"Access to business denied")
    {
        BusinessId = businessId;
    }

    public override string GetLogMessage()
    {
        return BusinessId.HasValue ?
            $"Unauthorized attempt to access business with ID {BusinessId}" :
            $"Cross-entity request detected";
    }
}
