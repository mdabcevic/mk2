using Bartender.Data;
using Bartender.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Bartender.Domain.Services;

public class NotificationService(IConnectionMultiplexer redis) : INotificationService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private static string Key(int tableId) => $"notifications:table:{tableId}";

    public async Task AddNotificationAsync(int tableId, TableNotification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        await _db.ListRightPushAsync(Key(tableId), json);
    }

    public async Task<List<TableNotification>> GetNotificationsAsync(int tableId)
    {
        var items = await _db.ListRangeAsync(Key(tableId));
        return [.. items.Select(i => JsonSerializer.Deserialize<TableNotification>(i!)!)];
    }

    //public async Task MarkAllAsReadAsync(int tableId)
    //{
    //    var key = Key(tableId);
    //    var items = await _db.ListRangeAsync(key);
    //    var updated = items
    //        .Select(i => JsonSerializer.Deserialize<TableNotification>(i!)!)
    //        .ToList();

    //    foreach (var item in updated)
    //        item.Pending = false;

    //    await _db.KeyDeleteAsync(key);
    //    foreach (var updatedItem in updated)
    //    {
    //        await _db.ListRightPushAsync(key, JsonSerializer.Serialize(updatedItem));
    //    }
    //}

    public Task ClearNotificationsAsync(int tableId) =>
        _db.KeyDeleteAsync(Key(tableId));
}
