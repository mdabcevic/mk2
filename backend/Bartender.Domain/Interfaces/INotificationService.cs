
using Bartender.Data;
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface INotificationService
{
    Task AddNotificationAsync(Tables table, TableNotification notification);
    Task<List<TableNotification>> GetNotificationsAsync(int tableId);
    Task MarkNotificationComplete(int tableId, string notificationId);
    Task ClearNotificationsAsync(int tableId);
}
