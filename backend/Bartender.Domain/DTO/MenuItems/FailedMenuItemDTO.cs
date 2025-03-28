
namespace Bartender.Domain.DTO.MenuItems;

public class FailedMenuItemDto
{
    public required UpsertMenuItemDto MenuItem { get; set; }
    public required string ErrorMessage { get; set; }
}
