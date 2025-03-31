
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.DTO;

public class PlaceWithMenuDto
{
    public required string BusinessName { get; set; }
    public required string Address { get; set; }
    public required string CityName { get; set; }
    public required string WorkHours { get; set; }

    public List<MenuItemDto> MenuItems { get; set; } = [];
}
