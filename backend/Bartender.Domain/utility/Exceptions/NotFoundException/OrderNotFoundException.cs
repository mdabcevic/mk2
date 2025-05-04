namespace Bartender.Domain.Utility.Exceptions;

public class OrderNotFoundException : NotFoundException
{
    public int OrderId { get; }

    public OrderNotFoundException(int orderId)
        : base($"Order was not found.")
    {
        OrderId = orderId;
    }

    public override string GetLogMessage()
    {
        return $"Order with ID {OrderId} was not found.";
    }
}
