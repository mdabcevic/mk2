
namespace Bartender.Data;

public class TableNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string TableLabel { get; set; } = string.Empty;
    public required string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.StaffNeeded;
    public int? OrderId { get; set; }
    public bool Pending { get; set; } = true; // For UI: show as new/unread
}
