
namespace Bartender.Domain.utility.Exceptions;

public class UnknownErrorException : Exception
{
    public UnknownErrorException(string message) : base(message) { }
    public virtual string GetLogMessage()
    {
        return null;
    }
}
