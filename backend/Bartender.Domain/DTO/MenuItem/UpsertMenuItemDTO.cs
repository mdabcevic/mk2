
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.DTO.MenuItem;

public class UpsertMenuItemDto
{
    [Required]
    public int PlaceId { get; set; }
    [Required]
    public int ProductId { get; set; }
    public decimal? Price { get; set; } = 0;
    [Required]
    public bool IsAvailable { get; set; }
    public string? Description { get; set; }
}
