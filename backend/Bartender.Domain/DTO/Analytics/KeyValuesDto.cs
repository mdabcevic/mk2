
namespace Bartender.Domain.DTO.Analytics;

public class KeyValuesDto
{
    public required decimal Earnings { get; set; }
    public required decimal AverageOrder { get; set; }
    public required int TotalOrders { get; set; }
    public required string FirstEverOrderDate { get; set; }
    public required DateTime FirstOrderDate { get; set; }
    public required DateTime LastOrderDate { get; set; }
}
