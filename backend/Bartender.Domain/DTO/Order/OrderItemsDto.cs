namespace Bartender.Domain.DTO.Order;

public class OrderItemsDto
{
    public required string MenuItem { get; set; } = string.Empty;
    public required decimal Price { get; set; }
    public required decimal Discount { get; set; }
    public required int Count { get; set; }
}
