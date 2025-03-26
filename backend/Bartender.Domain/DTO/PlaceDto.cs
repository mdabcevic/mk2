
namespace Bartender.Domain.DTO;

public  class PlaceDto
{
    public required string BusinessName { get; set; }
    public required string Address { get; set; }
    public required string CityName { get; set; }
    public required string WorkHours { get; set; } 
    //public TimeOnly OpensAt { get; set; }
    //public TimeOnly ClosesAt { get; set; }

    public List<MenuItemDto> MenuItems { get; set; } = [];
}
