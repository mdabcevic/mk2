
using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Order;

public class OrderDto : OrderBaseDto
{
    public required int Id { get; set; }
    public required List<OrderItemsDto> Items { get; set; }
    public string Table { get; set; } //TODO: required?
    public string? Note { get; set; }
    public required PaymentType PaymentType { get; set; }
}
