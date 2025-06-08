namespace Bartender.Domain.DTO.Analytics;

public class ProductsByDayOfWeekDto
{
    public required string WeekGroup { get; set; }
    public List<PopularProductsDto> PopularProducts { get; set; } = new List<PopularProductsDto>();
}
