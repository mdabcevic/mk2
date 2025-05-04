namespace Bartender.Domain.Utility.Exceptions;

public class TableNotFoundException : NotFoundException
{
    public int? TableId { get; }
    public string? Salt { get; }
    public string? Label { get; }

    public TableNotFoundException(int tableId)
        : base($"Table was not found.")
    {
        TableId = tableId;
    }

    public TableNotFoundException(string? salt = null, string? label = null)
        : base($"Table was not found.")
    {
        Salt = salt;
        Label = label;
    }

    public override string GetLogMessage()
    {
        return TableId.HasValue ? 
            $"Table with ID {TableId} was not found." :
            string.IsNullOrEmpty(Label) ? $"Table with salt {Salt} was not found" :
            $"Table with label {Label} was not found";
    }
}
