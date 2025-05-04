namespace Bartender.Domain.utility.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
    : base(message)
    {
    }
}
