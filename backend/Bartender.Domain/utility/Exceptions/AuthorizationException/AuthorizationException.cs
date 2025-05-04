namespace Bartender.Domain.Utility.Exceptions;

public class AuthorizationException : BaseException
{
    public AuthorizationException(string message)
    : base(message)
    {
    }
}
