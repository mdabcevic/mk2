namespace Bartender.Domain.DTO;

public class TableDto
{
    public int PlaceId { get; set; }
    public string PlaceName { get; set; }
    public int Seats { get; set; }
    public string Status { get; set; } = "empty"; // If you want, we can convert this to TableStatus enum here
    public string Salt { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
