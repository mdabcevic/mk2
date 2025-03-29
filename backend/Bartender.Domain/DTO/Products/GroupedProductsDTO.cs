
namespace Bartender.Domain.DTO.Products;

public class GroupedProductsDto
{
    public required string Category { get; set; }
    public List<ProductBaseDto>? Products { get; set; }
}
