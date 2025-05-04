
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.utility.Exceptions;

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
