namespace Bartender.Domain.Utility.Exceptions;

public class AppValidationException(string message, object? data = null) : BaseException(message, data)
{
}
