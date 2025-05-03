
using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Order;

public class GroupedOrderStatusDto
{
    public required OrderStatus Status { get; set; }
    public List<OrderDto>? Orders { get; set; }
}