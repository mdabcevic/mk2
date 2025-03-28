using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/business")]
[ApiController]
public class BusinessController(IBusinessService businessService) : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> GetById(int id)
    {
        var business = await businessService.GetByIdAsync(id);
        return business != null ? Ok(business) : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> GetAll()
    {
        var businesses = await businessService.GetAllAsync();
        return Ok(businesses);
    }

    [HttpPost]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> Create([FromBody] Businesses business)
    {
        await businessService.AddAsync(business);
        return CreatedAtAction(nameof(GetById), new { id = business.Id }, business);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Update(int id, [FromBody] Businesses business)
    {
        if (business.Id != id)
            return BadRequest();

        await businessService.UpdateAsync(business);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await businessService.DeleteAsync(id);
        return NoContent();
    }
}


