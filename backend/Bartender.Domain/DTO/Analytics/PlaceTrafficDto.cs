
namespace Bartender.Domain.DTO.Analytics;

public class PlaceTrafficDto
{
    public required int PlaceId { get; set; }
    public required string Address { get; set; }
    public required string CityName { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Long { get; set; }
    public required int Count { get; set; }
    public required decimal Earnings { get; set; }
}
