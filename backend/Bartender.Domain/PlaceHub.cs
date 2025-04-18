using Microsoft.AspNetCore.SignalR;

namespace Bartender.Domain;

public class PlaceHub : Hub
{
    public async Task JoinPlaceGroup(int placeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"place_{placeId}_staff");
    }

    public async Task LeavePlaceGroup(int placeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"place_{placeId}_staff");
    }
}
