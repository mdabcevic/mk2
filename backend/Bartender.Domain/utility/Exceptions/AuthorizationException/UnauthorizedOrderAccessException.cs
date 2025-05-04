
namespace Bartender.Domain.Utility.Exceptions;

public class UnauthorizedOrderAccessException : AuthorizationException
{
    public int OrderId { get; }
    public UnauthorizedOrderAccessException(int orderId)
        : base($"Cannot access this order")
    {
        OrderId = orderId;
    }

    public override string GetLogMessage()
    {
        return $"Unauthorized attempt to access order with ID {OrderId}";
    }
}
