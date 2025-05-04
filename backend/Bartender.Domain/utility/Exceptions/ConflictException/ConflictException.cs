namespace Bartender.Domain.Utility.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message, object? data = null)
        : base(message, data)
    {
    }
}
