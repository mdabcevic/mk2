using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.DTO.MenuItem;

public class MenuItemDto : MenuItemBaseDto
{
    public required PlaceDto Place { get; set; }
}
