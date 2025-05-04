namespace Bartender.Domain.Utility.Exceptions;

public class AppValidationException : BaseException
{
    public AppValidationException(string message, object? data = null)
    : base(message, data)
    {
    }
}
