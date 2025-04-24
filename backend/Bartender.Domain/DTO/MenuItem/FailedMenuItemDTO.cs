
namespace Bartender.Domain.DTO.MenuItem;

public class FailedMenuItemDto
{
    public required UpsertMenuItemDto MenuItem { get; set; }
    public required string ErrorMessage { get; set; }
}
