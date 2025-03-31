
namespace Bartender.Domain.DTO;

public  class PlaceDto
{
    public string? BusinessName { get; set; }
    public required string Address { get; set; }
    public required string CityName { get; set; }
    public required string WorkHours { get; set; } 
}
