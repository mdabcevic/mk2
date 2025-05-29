using Microsoft.AspNetCore.Mvc;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Bartender.Domain.DTO.Order;

namespace BartenderBackend.Controllers;

[Route("api/order")]
[ApiController]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await orderService.GetByIdAsync(id,false);
        return Ok(result);
    }

    [Authorize(Roles = "admin, owner, manager, regular")]
    [HttpGet("table-orders/{tableLabel}")]
    public async Task<IActionResult> GetCurrentOrdersByTable(string tableLabel)
    {
        var result = await orderService.GetCurrentOrdersByTableLabelAsync(tableLabel);
        return Ok(result);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetGuestOrders([FromQuery] bool userSpecific = true)
    {
        var result = await orderService.GetActiveTableOrdersForUserAsync(userSpecific);
        return Ok(result);
    }

    [Authorize(Roles = "admin, owner, manager")]
    [HttpGet("closed/{placeId}")]
    public async Task<IActionResult> GetAllByPlace(int placeId, [FromQuery] int page = 1, [FromQuery] int size = 30)
    {
        var result = await orderService.GetAllClosedOrdersByPlaceIdAsync(placeId, page,size);
        return Ok(result);
    }

    [Authorize(Roles = "admin, owner, manager, regular")]
    [HttpGet("active/{placeId}")]
    public async Task<IActionResult> GetAllActiveByPlace(int placeId, [FromQuery] bool onlyWaitingForStaff = false, [FromQuery] int page = 1, [FromQuery] bool grouped = false, [FromQuery] int size = 15)
    {
        if (grouped)
        {
            var groupedResult = await orderService.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page, size, onlyWaitingForStaff);
            return Ok(groupedResult);
        }

        var result = await orderService.GetAllActiveOrdersByPlaceIdAsync(placeId, onlyWaitingForStaff);
        return Ok(result);
    }

    [Authorize(Roles = "admin, owner")]
    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetAllByBusiness(int businessId)
    {
        var result = await orderService.GetAllByBusinessIdAsync(businessId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertOrderDto order)
    {
        await orderService.AddAsync(order);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertOrderDto order)
    {
        await orderService.UpdateAsync(id, order);
        return NoContent();
    }


    [HttpPut("status/{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto orderStatus)
    {
        await orderService.UpdateStatusAsync(id, orderStatus);
        return NoContent();
    }

    /// <summary>
    /// Only cancelled orders can be deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await orderService.DeleteAsync(id);
        return NoContent();
    }
}
