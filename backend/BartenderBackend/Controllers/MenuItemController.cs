using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/menu")]
[ApiController]
public class MenuItemController(IMenuItemService menuItemsService) : ControllerBase
{
    [HttpGet("{placeId}/{productId}")]
    public async Task<IActionResult> GetById(int placeId, int productId)
    {
        var result = await menuItemsService.GetByIdAsync(placeId, productId);
        return Ok(result);
    }

    [Authorize(Roles = "owner")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await menuItemsService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{placeId}")]
    public async Task<IActionResult> GetByPlaceId(
        int placeId, 
        [FromQuery] bool onlyAvailable = false,
        [FromQuery] bool groupByCategory = false)
    {
        if (groupByCategory)
        {
            var resultGrouped = await menuItemsService.GetByPlaceIdGroupedAsync(placeId, onlyAvailable);
            return Ok(resultGrouped);
        }

        var result = await menuItemsService.GetByPlaceIdAsync(placeId, onlyAvailable);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetFilteredList(int placeId, [FromQuery] string searchProduct)
    {
        var result = await menuItemsService.GetFilteredAsync(placeId, searchProduct);
        return Ok(result);
    }

    /*[HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertMenuItemDTO menuItem)
    {
        var result = await menuItemsService.AddAsync(menuItem);
        return result.ToActionResult();
    }*/

    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] List<UpsertMenuItemDto> menuItems)
    {
        await menuItemsService.AddMultipleAsync(menuItems);
        return NoContent();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPost("{fromPlaceId}/{toPlaceId}/copy")]
    public async Task<IActionResult> CreateCopy(int fromPlaceId, int toPlaceId)
    {
        await menuItemsService.CopyMenuAsync(fromPlaceId, toPlaceId);
        return NoContent();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpsertMenuItemDto menuItem)
    {
        await menuItemsService.UpdateAsync(menuItem);
        return NoContent();
    }

    [Authorize(Roles = "admin, manager, regular")]
    [HttpPut("{placeId}/{productId}/availability")]
    public async Task<IActionResult> UpdateAvailability(int placeId, int productId, [FromBody] bool isAvailable)
    {
        await menuItemsService.UpdateItemAvailabilityAsync(placeId, productId, isAvailable);
        return NoContent();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpDelete("{placeId}/{productId}")]
    public async Task<IActionResult> Delete(int placeId, int productId)
    {
        await menuItemsService.DeleteAsync(placeId, productId);
        return NoContent();
    }
}

