using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

//TODO: only managers should be able to access these endpoints.
[Route("api/[controller]")]
[ApiController]
public class StaffController(IStaffService staffService) : ControllerBase
{
    [HttpGet("{id?}")]
    public async Task<IActionResult> Get(int? id)
    {
        if (id.HasValue)
        {
            var staffMember = await staffService.GetByIdAsync(id.Value);
            return staffMember != null ? Ok(staffMember) : NotFound();
        }

        var staffList = await staffService.GetAllAsync();
        return Ok(staffList);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Staff staff)
    {
        await staffService.AddAsync(staff);
        return CreatedAtAction(nameof(Get), new { id = staff.Id }, staff);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Staff staff)
    {
        if (staff.Id != id)
            return BadRequest();

        await staffService.UpdateAsync(id, staff);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await staffService.DeleteAsync(id);
        return NoContent();
    }
}
