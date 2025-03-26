
namespace Bartender.Domain.DTO;

public class MenuItemDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public required string Category { get; set; } = "N/A";
}
