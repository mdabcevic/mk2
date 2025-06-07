namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class OrderNotFoundException(int orderId) : NotFoundException($"Order was not found.")
{
    public int OrderId { get; } = orderId;

    public override string GetLogMessage()
    {
        return $"Order with ID {OrderId} was not found.";
    }
}
