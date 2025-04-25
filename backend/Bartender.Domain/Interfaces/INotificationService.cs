
using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface INotificationService
{
    Task AddNotificationAsync(Table table, TableNotification notification);
    Task<ServiceResult<List<TableNotification>>> GetNotificationsAsync(int tableId);
    Task<ServiceResult> MarkNotificationComplete(int tableId, string notificationId);
    Task<ServiceResult> ClearNotificationsAsync(int tableId);
}
