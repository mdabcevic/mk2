using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.Orders;

public class UpsertOrderMenuItemDto
{
    [Required]
    public required int MenuItemId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Item count must be at least 1")]
    public required int Count { get; set; }
    public decimal? Discount { get; set; }
}
