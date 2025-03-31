
namespace Bartender.Domain.DTO.MenuItems;

public class MenuItemDto : MenuItemBaseDto
{
    public required PlaceDto Place { get; set; }
}
