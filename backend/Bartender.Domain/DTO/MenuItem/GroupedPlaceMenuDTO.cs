
using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.DTO.MenuItem;

public class GroupedPlaceMenuDto
{
    public required PlaceDto Place {  get; set; }
    public List<MenuItemBaseDto>? Items { get; set; }
}
