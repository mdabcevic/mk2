namespace Bartender.Domain.utility.Exceptions;

public class AuthorizationException : BaseException
{
    public AuthorizationException(string message)
    : base(message)
    {
    }
}
