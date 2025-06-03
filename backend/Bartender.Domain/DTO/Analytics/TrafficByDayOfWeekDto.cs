
namespace Bartender.Domain.DTO.Analytics;

public class TrafficByDayOfWeekDto
{
    public required string DayOfWeek { get; set; }
    public required int Count { get; set; }
    public required decimal Earnings { get; set; }
}
