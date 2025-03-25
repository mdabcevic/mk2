using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "admin, manager")]
public class MenuItemsController(IMenuItemsService menuItemsService) : ControllerBase
{
    [HttpGet("{placeId}/{productId}")]
    public async Task<IActionResult> GetById(int placeId, int productId)
    {
        try
        {
            var menuItem = await menuItemsService.GetByIdAsync(placeId, productId);
            return menuItem == null ? NotFound() : Ok(menuItem);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An unexpected error occurred", Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var menuItems = await menuItemsService.GetAllAsync();
            return Ok(menuItems);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An unexpected error occurred", Error = ex.Message });
        }
    }

    [HttpGet("{placeId}")]
    public async Task<IActionResult> GetByPlaceId(int placeId)
    {
        try
        {
            var menuItems = await menuItemsService.GetByPlaceIdAsync(placeId);
            return menuItems == null ? NotFound() : Ok(menuItems);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An unexpected error occurred", Error = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetFilteredList(int placeId, string searchProduct)
    {
        try
        {
            var menuItems = await menuItemsService.GetFilteredAsync(placeId, searchProduct);
            return Ok(menuItems);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An unexpected error occurred", Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertMenuItemDTO menuItem)
    {
        try
        {
            await menuItemsService.AddAsync(menuItem);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex) when (ex is DuplicateEntryException || ex is ValidationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("multiple")]
    public async Task<IActionResult> AddMultiple([FromBody] IEnumerable<UpsertMenuItemDTO> menuItems)
    {
        var failed = await menuItemsService.AddMultipleAsync(menuItems);

        if (failed.Any())
        {
            return Ok(new { message = "Some items failed to be added.", failed });
        }

        return Ok(new { message = "All items added successfully." });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpsertMenuItemDTO menuItem)
    {
        try
        {
            await menuItemsService.UpdateAsync(menuItem);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex) when (ex is DuplicateEntryException || ex is ValidationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{placeId}/{productId}")]
    public async Task<IActionResult> Delete(int placeId, int productId)
    {
        try
        {
            await menuItemsService.DeleteAsync(placeId, productId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An unexpected error occurred", Error = ex.Message });
        }
    }
}

