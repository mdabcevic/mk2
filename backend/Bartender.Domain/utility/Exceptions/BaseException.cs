
namespace Bartender.Domain.utility.Exceptions;

public class BaseException : Exception
{
    protected BaseException(string message, object? additionalData = null)
        : base(message)
    {
        if (additionalData != null)
        {
            Data["AdditionalData"] = additionalData;
        }
    }

    public virtual string GetLogMessage() => Message ?? GetType().Name;
}
