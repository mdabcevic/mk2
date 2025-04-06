
using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Orders;

public class GroupedOrderStatusDto
{
    public required OrderStatus Status { get; set; }
    public List<OrderBaseDto> Orders { get; set; }
}
