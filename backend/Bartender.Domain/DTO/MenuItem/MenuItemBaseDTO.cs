using Bartender.Domain.DTO.Product;
using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.MenuItem;

public class MenuItemBaseDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Id { get; set; }
    public required ProductBaseDto Product { get; set; }
    [JsonIgnore]
    public decimal Price { get; set; }
    [JsonPropertyName("price")]
    public string FormattedPrice => Price.ToString("0.00");
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
}
