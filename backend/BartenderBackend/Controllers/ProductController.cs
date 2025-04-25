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
        return (await productsService.GetByIdAsync(id)).ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool group = false, [FromQuery] bool? exclusive = null)
    {
        if (group) { 
            return (await productsService.GetAllGroupedAsync(exclusive)).ToActionResult();
        }
        return (await productsService.GetAllAsync(exclusive)).ToActionResult();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetProductCategories()
    {
        var categories = await productsService.GetProductCategoriesAsync();
        return categories.ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetFilteredList([FromQuery] bool? exclusive = null, [FromQuery] string? name = null,[FromQuery] string? category = null)
    {
        var result = await productsService.GetFilteredAsync(exclusive, name, category);
        return result.ToActionResult();
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertProductDto product)
    {

        return (await productsService.AddAsync(product)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertProductDto product)
    {
        return (await productsService.UpdateAsync(id, product)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await productsService.DeleteAsync(id)).ToActionResult();
    }
}
