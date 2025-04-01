
using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.Products;

public class ProductBaseDto
{
    public required string Name { get; set; }
    public required string Volume { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Category { get; set; }
}
