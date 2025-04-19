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
    public async Task<IActionResult> Get(int tableId)
    {
        var result = await service.GetNotificationsAsync(tableId);
        return result.ToActionResult();
    }

    [Authorize(Roles = "manager, employee")]
    [HttpDelete]
    public async Task<IActionResult> Clear(int tableId)
    {
        var result = await service.ClearNotificationsAsync(tableId);
        return result.ToActionResult();
    }

    [Authorize(Roles = "manager, employee")]
    [HttpPatch("{notificationId}/mark-complete")]
    public async Task<IActionResult> MarkAsRead(int tableId, string notificationId)
    {
        var result = await service.MarkNotificationComplete(tableId, notificationId);
        return result.ToActionResult();
    }
}
