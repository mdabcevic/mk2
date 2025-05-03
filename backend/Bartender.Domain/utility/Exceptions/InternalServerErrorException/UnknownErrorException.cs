namespace Bartender.Domain.utility.Exceptions;

public class UnknownErrorException : BaseException
{
    public UnknownErrorException(string message, object? data = null)
    : base(message, data)
    {
    }
}
