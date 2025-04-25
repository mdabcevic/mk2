
namespace Bartender.Domain.DTO.MenuItem;

public class GroupedCategoryMenuDto
{
    public required string Category { get; set; }
    public List<MenuItemBaseDto>? Items { get; set; }
}
