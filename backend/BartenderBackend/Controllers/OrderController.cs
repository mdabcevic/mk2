using Microsoft.AspNetCore.Mvc;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Bartender.Domain.DTO.Orders;

namespace BartenderBackend.Controllers;

[Route("api/order")]
[ApiController]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await orderService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetGuestOrders([FromQuery] bool userSpecific = true)
    {
        var result = await orderService.GetActiveTableOrdersForUserAsync(userSpecific);
        return result.ToActionResult();
    }

    [Authorize(Roles = "admin, owner, manager")]
    [HttpGet("closed/{placeId}")]
    public async Task<IActionResult> GetAllByPlace(int placeId, [FromQuery] int page = 1)
    {
        var result = await orderService.GetAllClosedOrdersByPlaceIdAsync(placeId, page);
        return result.ToActionResult();
    }

    [Authorize(Roles = "admin, owner, manager, regular")]
    [HttpGet("active/{placeId}")]
    public async Task<IActionResult> GetAllActiveByPlace(int placeId, [FromQuery] bool onlyWaitingForStaff = false, [FromQuery] int page = 1, [FromQuery] bool grouped = false)
    {
        if (grouped)
            return Ok(await orderService.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page, onlyWaitingForStaff));

        var result = await orderService.GetAllActiveOrdersByPlaceIdAsync(placeId, onlyWaitingForStaff);
        return result.ToActionResult();
    }

    [Authorize(Roles = "admin, owner")]
    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetAllByBusiness(int businessId)
    {
        var result = await orderService.GetAllByBusinessIdAsync(businessId);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertOrderDto order)
    {
        var result = await orderService.AddAsync(order);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertOrderDto order)
    {
        var result = await orderService.UpdateAsync(id, order);
        return result.ToActionResult();
    }


    [HttpPut("status/{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto orderStatus)
    {
        var result = await orderService.UpdateStatusAsync(id, orderStatus);
        return result.ToActionResult();
    }

    /// <summary>
    /// Only cancelled orders can be deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await orderService.DeleteAsync(id)).ToActionResult();
    }
}
