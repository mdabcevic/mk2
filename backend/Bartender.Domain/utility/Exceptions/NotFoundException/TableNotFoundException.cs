namespace Bartender.Domain.utility.Exceptions;

public class TableNotFoundException : NotFoundException
{
    public int? TableId { get; }
    public string? Salt { get; }

    public TableNotFoundException(int tableId)
        : base($"Table was not found.")
    {
        TableId = tableId;
    }

    public TableNotFoundException(string salt)
        : base($"Table was not found.")
    {
        Salt = salt;
    }

    public override string GetLogMessage()
    {
        return TableId.HasValue ? 
            $"Table with ID {TableId} was not found." :
            $"Table with salt {Salt} was not found";
    }
}
