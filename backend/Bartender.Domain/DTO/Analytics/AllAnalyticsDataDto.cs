namespace Bartender.Domain.DTO.Analytics;

public class AllAnalyticsDataDto
{
    public List<ProductsByDayOfWeekDto> PopularProducts {  get; set; } = new List<ProductsByDayOfWeekDto>();
    public List<TrafficByDayOfWeekDto> DailyTraffic { get; set; } = new List<TrafficByDayOfWeekDto>();
    public List<HourlyTrafficDto> HourlyTraffic { get; set; } = new List<HourlyTrafficDto>();
    public List<PlaceTrafficDto> PlaceTraffic { get; set; } = new List<PlaceTrafficDto>();
    public List<OrdersByWeatherDto> WeatherAnalytics { get; set; } = new List<OrdersByWeatherDto>();
    public KeyValuesDto KeyValues { get; set; } 
}
