namespace Bartender.Domain.utility.Exceptions;

public class TableNotFoundException : NotFoundException
{
    public int TableId { get; }

    public TableNotFoundException(int tableId)
        : base($"Table was not found.")
    {
        TableId = tableId;
    }

    public override string GetLogMessage()
    {
        return $"Table with ID {TableId} was not found.";
    }
}
