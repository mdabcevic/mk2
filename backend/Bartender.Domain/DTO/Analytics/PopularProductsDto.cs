using System.Text.Json.Serialization;

namespace Bartender.Domain.DTO.Analytics;

public class PopularProductsDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Date { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DayOfWeek { get; set; }
    public required int ProductId { get; set; }
    public required string Product {  get; set; }
    public required int Count { get; set; }
    public required decimal Earnings { get; set; }
}
