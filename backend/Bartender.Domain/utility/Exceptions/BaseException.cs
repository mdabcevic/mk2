
namespace Bartender.Domain.Utility.Exceptions;

public class BaseException : Exception
{
    public string? LogMessage { get; private set; }
    protected BaseException(string message, object? additionalData = null)
        : base(message)
    {
        if (additionalData != null)
        {
            Data["AdditionalData"] = additionalData;
        }
    }

    public BaseException WithLogMessage(string logMessage)
    {
        LogMessage = logMessage;
        return this;
    }

    public virtual string GetLogMessage() => LogMessage ?? Message ?? GetType().Name;
}
