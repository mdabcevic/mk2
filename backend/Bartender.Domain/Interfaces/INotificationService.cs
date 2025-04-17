
using Bartender.Data;

namespace Bartender.Domain.Interfaces;

public interface INotificationService
{
    Task AddNotificationAsync(int tableId, TableNotification notification);
    Task<List<TableNotification>> GetNotificationsAsync(int tableId);
    //Task MarkAllAsReadAsync(int tableId);
    Task ClearNotificationsAsync(int tableId);
}
