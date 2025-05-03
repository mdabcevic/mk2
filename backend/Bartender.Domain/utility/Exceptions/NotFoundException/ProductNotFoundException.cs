namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class ProductNotFoundException : NotFoundException
{
    public int ProductId { get; }

    public ProductNotFoundException(int productId, object? data)
        : base($"Product was not found.", data)
    {
        ProductId = productId;
    }

    public override string GetLogMessage()
    {
        return $"Product with ID {ProductId} was not found.";
    }
}
