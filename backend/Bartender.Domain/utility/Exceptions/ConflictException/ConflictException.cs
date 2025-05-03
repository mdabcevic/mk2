namespace Bartender.Domain.utility.Exceptions.ConflictException;

public class ConflictException : BaseException
{
    public ConflictException(string message, object? data = null)
        : base(message, data)
    {
    }
}
