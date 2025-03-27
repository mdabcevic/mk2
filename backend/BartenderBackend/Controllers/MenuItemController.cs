using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuItemController(IMenuItemService menuItemsService) : ControllerBase
{
    [HttpGet("{placeId}/{productId}")]
    public async Task<IActionResult> GetById(int placeId, int productId)
    {
        var result = await menuItemsService.GetByIdAsync(placeId, productId);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await menuItemsService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{placeId}")]
    public async Task<IActionResult> GetByPlaceId(int placeId, bool onlyAvailable = false, bool groupByCategory = false)
    {
        if (groupByCategory)
        {
            var resultGrouped = await menuItemsService.GetByPlaceIdGroupedAsync(placeId, onlyAvailable);
            return resultGrouped.ToActionResult();
        }

        var result = await menuItemsService.GetByPlaceIdAsync(placeId, onlyAvailable);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetFilteredList(int placeId, string searchProduct)
    {
        var result = await menuItemsService.GetFilteredAsync(placeId, searchProduct);
        return result.ToActionResult();
    }

    /*[HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertMenuItemDTO menuItem)
    {
        var result = await menuItemsService.AddAsync(menuItem);
        return result.ToActionResult();
    }*/

    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IEnumerable<UpsertMenuItemDTO> menuItems)
    {
        var result = await menuItemsService.AddMultipleAsync(menuItems);
        return result.ToActionResult();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpsertMenuItemDTO menuItem)
    {
        var result = await menuItemsService.UpdateAsync(menuItem);
        return result.ToActionResult();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpDelete("{placeId}/{productId}")]
    public async Task<IActionResult> Delete(int placeId, int productId)
    {
        var result = await menuItemsService.DeleteAsync(placeId, productId);
        return result.ToActionResult();
    }
}

