using Microsoft.AspNetCore.Mvc;
using Bartender.Domain.Interfaces;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductsService productsService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { 
            var product = await productsService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
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
    public async Task<IActionResult> GetAll(bool group = false)
    {
        var products = await productsService.GetAllAsync(group);
        return Ok(products);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetProductCategories()
    {
        var categories = await productsService.GetProductCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("filtered")]
    public async Task<IActionResult> GetFilteredList(string? name = null, string? category = null)
    {
        try
        {
            var products = await productsService.GetFilteredAsync(name, category);
            return Ok(products);
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
    public async Task<IActionResult> Create([FromBody] UpsertProductDTO product)
    {
        try
        {
            await productsService.AddAsync(product);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertProductDTO product)
    {
        try { 
            await productsService.UpdateAsync(id, product);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try {
            await productsService.DeleteAsync(id);
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
