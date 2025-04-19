using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;

namespace Bartender.Domain.Services;

public class NotificationService(
    IConnectionMultiplexer redis,
    IHubContext<PlaceHub> hub,
    ICurrentUserContext currentUser,
    IValidationService validationService
    ) : INotificationService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private static string Key(int tableId) => $"notifications:table:{tableId}";

    public async Task AddNotificationAsync(Tables table, TableNotification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        await _db.HashSetAsync(Key(table.Id), notification.Id, json);

        // Notify staff group for the place
        var placeGroupKey = $"place_{table.PlaceId}_staff"; 
        await hub.Clients.Group(placeGroupKey).SendAsync("ReceiveNotification", notification);
    }

    public async Task<ServiceResult<List<TableNotification>>> GetNotificationsAsync(int tableId)
    {
        var validUser = await validationService.VerifyUserGuestAccess(tableId);
        if (validUser.Success)
        {
            var entries = await _db.HashGetAllAsync(Key(tableId));
            return ServiceResult<List<TableNotification>>.Ok([.. entries.Select(entry => JsonSerializer.Deserialize<TableNotification>(entry.Value!)!)]);
        }
        return ServiceResult<List<TableNotification>>.Fail("Cannot access notifications for this table.", ErrorType.Unauthorized);
    }

    public async Task MarkNotificationComplete(int tableId, string notificationId)
    {
        var entry = await _db.HashGetAsync(Key(tableId), notificationId);
        var notif = JsonSerializer.Deserialize<TableNotification>(entry!);
        notif.Pending = false;
        await _db.HashSetAsync(Key(tableId), notificationId, JsonSerializer.Serialize(notif));
    }

    public Task ClearNotificationsAsync(int tableId) =>
        _db.KeyDeleteAsync(Key(tableId));
}
