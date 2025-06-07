namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class BusinessNotFoundException(int businessId) : NotFoundException($"Business was not found.")
{
    public int BusinessId { get; } = businessId;

    public override string GetLogMessage()
    {
        return $"Business with ID {BusinessId} was not found.";
    }
}
