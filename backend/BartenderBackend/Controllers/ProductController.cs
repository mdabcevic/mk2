using Microsoft.AspNetCore.Mvc;
using Bartender.Domain.Interfaces;
using Bartender.Domain.DTO.Products;
using Microsoft.AspNetCore.Authorization;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles ="admin, manager")]
public class ProductController(IProductService productsService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return (await productsService.GetByIdAsync(id)).ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool group = false)
    {

        if (group) { 
            return (await productsService.GetAllGroupedAsync()).ToActionResult();
        }

        return (await productsService.GetAllAsync()).ToActionResult();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetProductCategories()
    {
        var categories = await productsService.GetProductCategoriesAsync();
        return categories.ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetFilteredList([FromQuery] string? name = null,[FromQuery] string? category = null)
    {
        var result = await productsService.GetFilteredAsync(name, category);
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

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await productsService.DeleteAsync(id)).ToActionResult();
    }
}
