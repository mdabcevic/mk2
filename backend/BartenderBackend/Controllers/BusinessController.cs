using BartenderBackend.Models;
using BartenderBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var business = await _businessService.GetByIdAsync(id);
        return business != null ? Ok(business) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var businesses = await _businessService.GetAllAsync();
        return Ok(businesses);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Business business)
    {
        await _businessService.AddAsync(business);
        return CreatedAtAction(nameof(GetById), new { id = business.Id }, business);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Business business)
    {
        if (business.Id != id)
            return BadRequest();

        await _businessService.UpdateAsync(business);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _businessService.DeleteAsync(id);
        return NoContent();
    }
}


