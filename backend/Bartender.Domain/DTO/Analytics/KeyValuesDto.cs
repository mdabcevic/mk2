
namespace Bartender.Domain.DTO.Analytics;

public class KeyValuesDto
{
    public required decimal Revenue { get; set; }
    public required decimal AverageOrder { get; set; }
    public required int TotalOrders { get; set; }
    public required string FirstEverOrderDate { get; set; }
    public required string MostPopularProduct {  get; set; }
    public required DateTime FirstOrderDate { get; set; }
    public required DateTime LastOrderDate { get; set; }
}
