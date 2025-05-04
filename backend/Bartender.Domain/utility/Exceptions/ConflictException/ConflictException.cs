namespace Bartender.Domain.utility.Exceptions;

public class ConflictException : BaseException
{
    public string? LogMessage { get; }
    public ConflictException(string message)
        : base(message)
    {
    }
    public ConflictException(string message, object? data = null)
        : base(message, data)
    {
    }
    public ConflictException(string message, string? logMessage = null)
    : base(message)
    {
        LogMessage = logMessage;
    }

    public override string GetLogMessage()
    {
        return string.IsNullOrEmpty(LogMessage) ?
            Message :
            LogMessage;
    }
}
