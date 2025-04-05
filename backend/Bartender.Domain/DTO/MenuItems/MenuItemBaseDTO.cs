using Bartender.Domain.DTO.Products;
using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.MenuItems;

public class MenuItemBaseDto
{
    public int? id { get; set; }
    public required ProductBaseDto Product { get; set; }
    [JsonIgnore]
    public decimal Price { get; set; }
    [JsonPropertyName("price")]
    public string FormattedPrice => Price.ToString("0.00");
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
}
