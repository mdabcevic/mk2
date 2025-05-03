namespace Bartender.Domain.utility.Exceptions.ConflictException;

public class ConflictException : Exception
{
    public object? Data { get; set; }
    public ConflictException(string message, object? data = null) : base(message)
    {
        Data = data;
    }
    public virtual object? GetData()
    {
        return null;
    }
    public virtual string GetLogMessage()
    {
        return Message;
    }
}
