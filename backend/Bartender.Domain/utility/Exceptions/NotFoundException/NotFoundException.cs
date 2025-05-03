namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
