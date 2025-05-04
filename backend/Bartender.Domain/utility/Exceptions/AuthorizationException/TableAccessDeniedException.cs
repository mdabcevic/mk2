namespace Bartender.Domain.utility.Exceptions;

public class TableAccessDeniedException : AuthorizationException
{
    public int TableId { get; }
    public int? UserId { get; }
    public string? Token { get; }
    public TableAccessDeniedException(int tableId)
        : base($"You don't have access to manage orders for this table.")
    {
        TableId = tableId;
    }
    public TableAccessDeniedException(int tableId, string? token)
        : base($"You don't have access to manage orders for this table.")
    {
        TableId = tableId;
        Token = token; 
    }

    public TableAccessDeniedException(int tableId, int? userId)
        : base($"You don't have access to manage orders for this table.")
    {
        TableId = tableId;
        UserId = userId;
    }

    public override string GetLogMessage()
    {
        return !string.IsNullOrEmpty(Token) ?
            $"Access denied for TableId: {TableId}, Token: {Token}" :
            UserId.HasValue ? $"Access denied for TableId: {TableId}, UserId: {UserId}" :
            $"Access denied for TableId: {TableId}";

    }
}
