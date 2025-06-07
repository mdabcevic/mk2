namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

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
