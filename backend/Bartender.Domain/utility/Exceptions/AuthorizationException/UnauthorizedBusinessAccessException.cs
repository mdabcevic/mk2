
namespace Bartender.Domain.utility.Exceptions;

public class UnauthorizedBusinessAccessException : AuthorizationException
{

    public UnauthorizedBusinessAccessException()
        : base($"Failure fetching business with requested id.")
    {
    }

    public override string GetLogMessage()
    {
        return $"Cross-entity request detected";
        //return $"Cross-entity request detected. User {username} ({userId}) tried to access business with id {BusinessId}";
    }
}
