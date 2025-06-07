namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class BusinessNotFoundException : NotFoundException
{
    public int? BusinessId { get; }

    public BusinessNotFoundException(int? businessId = null)
        : base($"Business was not found.")
    {
        if (businessId != null)
            BusinessId = businessId;
    }

    public override string GetLogMessage()
    {
        return BusinessId.HasValue ?
            $"Business with ID {BusinessId} was not found." :
            "Business not found";
    }
}
