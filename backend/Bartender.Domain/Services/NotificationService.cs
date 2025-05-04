using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Bartender.Domain.Services;

public class NotificationService(
    IConnectionMultiplexer redis,
    IHubContext<PlaceHub> hub,
    IValidationService validationService,
    ILogger<NotificationService> logger
    ) : INotificationService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private static string Key(int tableId) => $"notifications:table:{tableId}";

    public async Task AddNotificationAsync(Table table, TableNotification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        await _db.HashSetAsync(Key(table.Id), notification.Id, json);
        logger.LogInformation("Notification added: {NotificationId} to table {TableId} (place {PlaceId})",
        notification.Id, table.Id, table.PlaceId);

        if (notification.OrderId is not null)
        {
            await MarkOrderNotificationsAsCompleteAsync(table.Id, notification.OrderId.Value);
        }

        // Notify staff group for the place
        var placeGroupKey = $"place_{table.PlaceId}_staff"; 
        await hub.Clients.Group(placeGroupKey).SendAsync("ReceiveNotification", notification);
        logger.LogInformation("Notification broadcasted to group {Group}", placeGroupKey);
    }

    public async Task<ServiceResult<List<TableNotification>>> GetNotificationsAsync(int tableId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);

        if (!validUser)
        {
            logger.LogWarning("Unauthorized attempt to read notifications for table {TableId}", tableId);
            return ServiceResult<List<TableNotification>>.Fail("Cannot access notifications for this table.", ErrorType.Unauthorized);
        }

        var entries = await _db.HashGetAllAsync(Key(tableId));
        var result = entries
            .Select(entry => JsonSerializer.Deserialize<TableNotification>(entry.Value!)!)
            .ToList();

        logger.LogInformation("Fetched {Count} notifications for table {TableId}", result.Count, tableId);
        return ServiceResult<List<TableNotification>>.Ok(result);
    }

    public async Task<ServiceResult> MarkNotificationComplete(int tableId, string notificationId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);

        if (!validUser)
        {
            logger.LogWarning("Unauthorized attempt to update notification {NotificationId} on table {TableId}", notificationId, tableId);
            return ServiceResult.Fail("Cannot update notification.", ErrorType.Unauthorized);
        }

        var entry = await _db.HashGetAsync(Key(tableId), notificationId);

        if (!entry.HasValue)
        {
            logger.LogWarning("Notification {NotificationId} not found for table {TableId}", notificationId, tableId);
            return ServiceResult.Fail("Notification not found.", ErrorType.NotFound);
        }

        var notif = JsonSerializer.Deserialize<TableNotification>(entry!)!;
        notif.Pending = false;

        await _db.HashSetAsync(Key(tableId), notificationId, JsonSerializer.Serialize(notif));

        logger.LogInformation("Notification {NotificationId} marked as complete for table {TableId}", notificationId, tableId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ClearNotificationsAsync(int tableId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);

        if (!validUser)
        {
            logger.LogWarning("Unauthorized attempt to clear notifications for table {TableId}", tableId);
            return ServiceResult.Fail("Cannot delete notifications for this table.", ErrorType.Unauthorized);
        }

        await _db.KeyDeleteAsync(Key(tableId));
        logger.LogInformation("All notifications cleared for table {TableId}", tableId);
        return ServiceResult.Ok();
    }

    private async Task MarkOrderNotificationsAsCompleteAsync(int tableId, int orderId)
    {
        var entries = await _db.HashGetAllAsync(Key(tableId));
        int updated = 0;
        foreach (var entry in entries)
        {
            var existing = JsonSerializer.Deserialize<TableNotification>(entry.Value!)!;
            if (existing.OrderId == orderId && existing.Pending)
            {
                existing.Pending = false;
                await _db.HashSetAsync(Key(tableId), existing.Id, JsonSerializer.Serialize(existing));
                updated++;
            }
        }
        logger.LogInformation("Previous {Count} notifications for Order {OrderId} marked as complete.", updated, orderId);
    }
}
