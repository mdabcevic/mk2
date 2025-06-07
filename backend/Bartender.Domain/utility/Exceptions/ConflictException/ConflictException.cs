namespace Bartender.Domain.Utility.Exceptions;

public class ConflictException(string message, object? data = null) : BaseException(message, data)
{
}
