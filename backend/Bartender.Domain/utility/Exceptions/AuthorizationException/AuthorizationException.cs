namespace Bartender.Domain.utility.Exceptions;

public class AuthorizationException : BaseException
{
    public string? LogMessage { get; }
    public AuthorizationException(string message)
    : base(message)
    {
    }

    public AuthorizationException(string message, string? logMessage = null)
    : base(message)
    {
        LogMessage = logMessage;
    }

    public override string GetLogMessage()
    {
        return string.IsNullOrEmpty(LogMessage) ? 
            $"Cross-entity request detected" :
            LogMessage;
    }
}
