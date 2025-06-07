namespace Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;

public class UnauthorizedOrderAccessException(int orderId) : AuthorizationException($"Cannot access this order")
{
    public int OrderId { get; } = orderId;

    public override string GetLogMessage()
    {
        return $"Unauthorized attempt to access order with ID {OrderId}";
    }
}
