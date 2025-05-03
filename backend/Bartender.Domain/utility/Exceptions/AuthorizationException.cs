
namespace Bartender.Domain.utility.Exceptions;

public class AuthorizationException : Exception
{
    public AuthorizationException(string message) : base(message) { }
    public virtual string GetLogMessage()
    {
        return Message;
    }
}
