namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class NotFoundException(string message) : BaseException(message)
{
}
