namespace Bartender.Domain.Utility.Exceptions;

public class TableAccessDeniedException : AuthorizationException
{
    public int TableId { get; }
    public int? UserId { get; }
    public string? Token { get; }
    public string? TableLabel { get; }

    public TableAccessDeniedException(int tableId, string? token = null, int? userId = null)
        : base($"Access to this table denied")
    {
        TableId = tableId;
        Token = token;
        UserId = userId;
    }
    public TableAccessDeniedException(string tableLabel)
        : base($"Access to this table denied")
    {
        TableLabel = tableLabel;
    }

    public override string GetLogMessage()
    {
        return !string.IsNullOrEmpty(Token) ? $"Access denied for TableId: {TableId}, Token: {Token}" :
            UserId.HasValue ? $"Access denied for TableId: {TableId}, UserId: {UserId}" :
            !string.IsNullOrEmpty(TableLabel) ? $"Access denied for Table with Label: {TableLabel}" :
            $"Access denied for TableId: {TableId}";

    }
}
