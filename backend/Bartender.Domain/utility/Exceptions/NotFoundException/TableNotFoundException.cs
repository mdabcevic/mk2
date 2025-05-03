namespace Bartender.Domain.utility.Exceptions.NotFoundException;

public class TableNotFoundException : NotFoundException
{
    public int TableId { get; }

    public TableNotFoundException(int tableId, object? data = null)
        : base($"Table was not found.", data)
    {
        TableId = tableId;
    }

    public override string GetLogMessage()
    {
        return $"Table with ID {TableId} was not found.";
    }
}
