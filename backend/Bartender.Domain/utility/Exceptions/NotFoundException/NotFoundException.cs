namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public virtual string GetLogMessage()
    {
        return Message;
    }
}
