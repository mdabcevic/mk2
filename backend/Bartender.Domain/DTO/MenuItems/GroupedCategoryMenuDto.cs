
namespace Bartender.Domain.DTO.MenuItems;

public class GroupedCategoryMenuDto
{
    public required string Category { get; set; }
    public List<MenuItemBaseDto>? Items { get; set; }
}
