namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class OrderNotFoundException : NotFoundException
{
    public int OrderId { get; }

    public OrderNotFoundException(int orderId, object? data)
        : base($"Order was not found.", data)
    {
        OrderId = orderId;
    }

    public override string GetLogMessage()
    {
        return $"Order with ID {OrderId} was not found.";
    }
}
