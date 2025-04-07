
using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Orders;

public class OrderDto : OrderBaseDto
{
    public required int Id { get; set; }
    public required List<OrderItemsDto> Items { get; set; }
    public string Table { get; set; }
    public string? Note { get; set; }
    public required PaymentType PaymentType { get; set; }
}
