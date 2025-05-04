
namespace Bartender.Domain.utility.Exceptions;

public class UnauthorizedBusinessAccessException : AuthorizationException
{

    public UnauthorizedBusinessAccessException()
        : base($"Access to business denied")
    {
    }

    public override string GetLogMessage()
    {
        return $"Cross-entity request detected";
        //return $"Cross-entity request detected. User {username} ({userId}) tried to access business with id {BusinessId}";
    }
}
