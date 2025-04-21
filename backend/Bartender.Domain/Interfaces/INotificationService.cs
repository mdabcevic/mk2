
using Bartender.Data;
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface INotificationService
{
    Task AddNotificationAsync(Tables table, TableNotification notification);
    Task<ServiceResult<List<TableNotification>>> GetNotificationsAsync(int tableId);
    Task<ServiceResult> MarkNotificationComplete(int tableId, string notificationId);
    Task<ServiceResult> ClearNotificationsAsync(int tableId);
}
