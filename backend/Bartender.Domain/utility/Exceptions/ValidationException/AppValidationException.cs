namespace Bartender.Domain.utility.Exceptions.ValidationException;

public class AppValidationException : Exception
{
    public AppValidationException(string message) : base(message) { }
    public virtual string GetLogMessage()
    {
        return Message;
    }
}
