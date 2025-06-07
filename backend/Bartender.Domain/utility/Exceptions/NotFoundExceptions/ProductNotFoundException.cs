namespace Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

public class ProductNotFoundException(int productId) : NotFoundException($"Product was not found.")
{
    public int ProductId { get; } = productId;

    public override string GetLogMessage()
    {
        return $"Product with ID {ProductId} was not found.";
    }
}
