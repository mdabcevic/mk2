using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;

namespace Bartender.Domain.Services;

public class NotificationService(
    IConnectionMultiplexer redis,
    IHubContext<PlaceHub> hub
    ) : INotificationService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private static string Key(int tableId) => $"notifications:table:{tableId}";

    //TODO: any time notification is added, forward it to frontend via signalR hub?
    public async Task AddNotificationAsync(Tables table, TableNotification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        await _db.ListRightPushAsync(Key(table.Id), json);

        // 🔔 Notify staff group for the place
        var placeGroupKey = $"place_{table.PlaceId}_staff"; // optional: pass placeId instead if you store it
        await hub.Clients.Group(placeGroupKey).SendAsync("ReceiveNotification", notification);
    }

    public async Task<List<TableNotification>> GetNotificationsAsync(int tableId)
    {
        var items = await _db.ListRangeAsync(Key(tableId));
        return [.. items.Select(i => JsonSerializer.Deserialize<TableNotification>(i!)!)];
    }

    public Task ClearNotificationsAsync(int tableId) =>
        _db.KeyDeleteAsync(Key(tableId));
}
