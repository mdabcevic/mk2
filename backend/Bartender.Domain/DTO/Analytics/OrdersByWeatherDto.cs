using Bartender.Data.Enums;

namespace Bartender.Domain.DTO.Analytics;

public class OrdersByWeatherDto
{
    public string WeekGroup { get; set; }
    public WeatherType WeatherType { get; set; }
    public double AverageOrdersPerHour { get; set; }
}
