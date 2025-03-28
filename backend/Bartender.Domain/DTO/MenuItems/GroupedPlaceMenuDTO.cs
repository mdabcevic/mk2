
namespace Bartender.Domain.DTO.MenuItems;

public class GroupedPlaceMenuDto
{
    public required PlaceDto Place {  get; set; }
    public List<MenuItemBaseDto>? Items { get; set; }
}
