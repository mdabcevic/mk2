namespace Bartender.Domain.utility.Exceptions;

public class NotFoundException : BaseException
{
    public string? LogMessage { get; }
    public NotFoundException(string message)
        : base(message)
    {
    }
    public NotFoundException(string message, string? logMessage = null)
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
