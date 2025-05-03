namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class MenuItemNotFoundException : NotFoundException
{
    public int MenuItemId { get; }

    public MenuItemNotFoundException(int menuItemId)
        : base($"Menu item was not found.")
    {
        MenuItemId = menuItemId;
    }

    public override string GetLogMessage()
    {
        return $"Menu item with ID {MenuItemId} was not found.";
    }
}
