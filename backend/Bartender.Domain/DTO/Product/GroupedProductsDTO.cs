namespace Bartender.Domain.DTO.Product;

public class GroupedProductsDto
{
    public required string Category { get; set; }
    public List<ProductBaseDto>? Products { get; set; }
}
