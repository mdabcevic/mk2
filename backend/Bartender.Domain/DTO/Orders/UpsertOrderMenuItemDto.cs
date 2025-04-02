using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.Orders;

public class UpsertOrderMenuItemDto
{
    public int? OrderId { get; set; }
    [Required]
    public required int MenuItemId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Item count must be at least 1")]
    public required int Count { get; set; }
}
