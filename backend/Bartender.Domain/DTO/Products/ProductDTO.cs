
namespace Bartender.Domain.DTO.Products;

public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Volume { get; set; }
    public ProductCategoryDto? Category { get; set; }
}
