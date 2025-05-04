using Microsoft.AspNetCore.Mvc;
using Bartender.Domain.Interfaces;
using Bartender.Domain.DTO.Product;
using Microsoft.AspNetCore.Authorization;

namespace BartenderBackend.Controllers;

[Route("api/product")]
[ApiController]
[Authorize(Roles ="admin, manager, owner")]
public class ProductController(IProductService productsService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await productsService.GetByIdAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool group = false, [FromQuery] bool? exclusive = null)
    {
        if (group) { 
            return Ok(await productsService.GetAllGroupedAsync(exclusive));
        }
        return Ok(await productsService.GetAllAsync(exclusive));
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductCategories()
    {
        var categories = await productsService.GetProductCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetFilteredList([FromQuery] bool? exclusive = null, [FromQuery] string? name = null,[FromQuery] string? category = null)
    {
        var result = await productsService.GetFilteredAsync(exclusive, name, category);
        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertProductDto product)
    {

        await productsService.AddAsync(product);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertProductDto product)
    {
        await productsService.UpdateAsync(id, product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await productsService.DeleteAsync(id);
        return NoContent();
    }
}
