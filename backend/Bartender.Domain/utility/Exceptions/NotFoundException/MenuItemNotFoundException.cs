namespace Bartender.Domain.utility.Exceptions;

public class MenuItemNotFoundException : NotFoundException
{
    public int? MenuItemId { get; }
    public int? ProductId { get; }
    public int? PlaceId { get; }

    public MenuItemNotFoundException(int placeId, int productId)
        : base($"Menu item was not found.")
    {
        ProductId = productId;
        PlaceId = placeId;
    }

    public MenuItemNotFoundException(int menuItemId)
        : base($"Menu item was not found.")
    {
        MenuItemId = menuItemId;
    }

    public override string GetLogMessage()
    {
        return MenuItemId.HasValue ?
            $"Menu item with ID {MenuItemId} was not found." :
            $"MenuItem with place id {PlaceId} and product id {ProductId} not found";
    }
}
