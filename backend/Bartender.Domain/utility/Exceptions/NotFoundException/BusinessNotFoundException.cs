

namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class BusinessNotFoundException : NotFoundException
{
    public int BusinessId { get; }

    public BusinessNotFoundException(int businessId, object? data)
        : base($"Business was not found.", data)
    {
        BusinessId = businessId;
    }

    public override string GetLogMessage()
    {
        return $"Business with ID {BusinessId} was not found.";
    }
}
