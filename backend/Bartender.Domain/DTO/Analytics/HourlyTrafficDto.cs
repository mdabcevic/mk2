
namespace Bartender.Domain.DTO.Analytics;

public class HourlyTrafficDto
{
    public required string DayOfWeek { get; set; }
    public List<HourCountDto> HourCounts { get; set; } = new();
}

public class HourCountDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
    public decimal AverageRevenue { get; set; }
}
