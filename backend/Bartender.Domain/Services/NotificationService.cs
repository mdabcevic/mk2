using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
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

    public async Task<List<TableNotification>> GetNotificationsAsync(int tableId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);

        if (!validUser)
        {
            throw new AuthorizationException("Cannot access notifications for this table.")
                .WithLogMessage($"Unauthorized attempt to read notifications for table {tableId}");
        }

        var entries = await _db.HashGetAllAsync(Key(tableId));
        var result = entries
            .Select(entry => JsonSerializer.Deserialize<TableNotification>(entry.Value!)!)
            .ToList();

        logger.LogInformation("Fetched {Count} notifications for table {TableId}", result.Count, tableId);
        return result;
    }

    public async Task MarkNotificationComplete(int tableId, string notificationId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);

        if (!validUser)
        {
            throw new AuthorizationException("Cannot update notification.")
                .WithLogMessage($"Unauthorized attempt to update notification {notificationId} on table {tableId}");
        }

        var entry = await _db.HashGetAsync(Key(tableId), notificationId);

        if (!entry.HasValue)
        {
            throw new NotFoundException("Notification not found.")
                .WithLogMessage($"Notification {notificationId} not found for table {tableId}");
        }

        var notif = JsonSerializer.Deserialize<TableNotification>(entry!)!;
        notif.Pending = false;

        await _db.HashSetAsync(Key(tableId), notificationId, JsonSerializer.Serialize(notif));

        logger.LogInformation("Notification {NotificationId} marked as complete for table {TableId}", notificationId, tableId);
    }

    public async Task ClearNotificationsAsync(int tableId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);

        if (!validUser)
        {
            throw new AuthorizationException("Cannot delete notifications for this table.")
                .WithLogMessage($"Unauthorized attempt to clear notifications for table {tableId}");
        }

        await _db.KeyDeleteAsync(Key(tableId));
        logger.LogInformation("All notifications cleared for table {TableId}", tableId);
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
