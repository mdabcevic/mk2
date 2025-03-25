using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "manager")] // Ensure only managers access this controller
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
    public async Task<IActionResult> Create([FromBody] UpsertStaffDto staff)
    {
        await staffService.AddAsync(staff);
        return CreatedAtAction(nameof(Get), new { id = staff.Id }, staff);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertStaffDto staff)
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
