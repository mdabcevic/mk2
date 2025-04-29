using Bartender.Domain.DTO.MenuItem;

namespace Bartender.Domain.DTO.Place;

public class PlaceWithMenuDto
{
    public required string BusinessName { get; set; }
    public required string Address { get; set; }
    public required string CityName { get; set; }
    public string? Description { get; set; }
    public required string WorkHours { get; set; }
    public int FreeTablesCount { get; set; }

    public List<MenuItemBaseDto> Menu { get; set; } = [];
}
