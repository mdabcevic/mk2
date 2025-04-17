using Bartender.Data;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TableNotification>>> Get(int tableId)
    {
        var notifs = await service.GetNotificationsAsync(tableId);
        return Ok(notifs);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int tableId, [FromBody] TableNotification notification)
    {
        await service.AddNotificationAsync(tableId, notification);
        return NoContent();
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead(int tableId)
    {
        await service.MarkAllAsReadAsync(tableId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(int tableId)
    {
        await service.ClearNotificationsAsync(tableId);
        return NoContent();
    }
}
