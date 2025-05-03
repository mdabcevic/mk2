namespace Bartender.Domain.utility.Exceptions;

public class AppValidationException : BaseException
{
    public AppValidationException(string message, object? data = null)
    : base(message, data)
    {
    }
}
