

namespace Bartender.Domain.Utility.Exceptions;

public class BusinessNotFoundException : NotFoundException
{
    public int BusinessId { get; }

    public BusinessNotFoundException(int businessId)
        : base($"Business was not found.")
    {
        BusinessId = businessId;
    }

    public override string GetLogMessage()
    {
        return $"Business with ID {BusinessId} was not found.";
    }
}
