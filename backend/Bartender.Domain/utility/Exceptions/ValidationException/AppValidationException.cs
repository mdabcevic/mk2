namespace Bartender.Domain.utility.Exceptions.ValidationException;

public class AppValidationException : BaseException
{
    public AppValidationException(string message, object? data = null)
    : base(message, data)
    {
    }
}
