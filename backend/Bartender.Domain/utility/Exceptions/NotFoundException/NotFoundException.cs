namespace Bartender.Domain.Utility.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
    : base(message)
    {
    }
}
