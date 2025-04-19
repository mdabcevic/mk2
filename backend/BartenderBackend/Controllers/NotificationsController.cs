using Bartender.Data;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(INotificationService service) : ControllerBase
{
    [Authorize(Roles = "manager, employee")]
    [HttpGet]
    public async Task<ActionResult<List<TableNotification>>> Get(int tableId)
    {
        var notifs = await service.GetNotificationsAsync(tableId);
        return Ok(notifs);
    }

    [Authorize(Roles = "manager, employee")]
    [HttpDelete]
    public async Task<IActionResult> Clear(int tableId)
    {
        await service.ClearNotificationsAsync(tableId);
        return NoContent();
    }

    [Authorize(Roles = "manager, employee")]
    [HttpPatch("{notificationId}/mark-complete")]
    public async Task<IActionResult> MarkAsRead(int tableId, string notificationId)
    {
        await service.MarkNotificationComplete(tableId, notificationId);
        return NoContent();
    }
}
