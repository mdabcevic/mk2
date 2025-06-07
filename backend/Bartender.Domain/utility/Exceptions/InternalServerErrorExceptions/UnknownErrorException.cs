namespace Bartender.Domain.Utility.Exceptions;

public class UnknownErrorException(string message, object? data = null) : BaseException(message, data)
{
}
