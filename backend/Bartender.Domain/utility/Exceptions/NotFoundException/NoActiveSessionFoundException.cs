
namespace Bartender.Domain.utility.Exceptions;

public class NoActiveSessionFoundException : NotFoundException
{

    public NoActiveSessionFoundException()
        : base($"There is currently no active session found")
    {
    }

    public override string GetLogMessage()
    {
        return $"There is currently no active session found";
    }
}
