using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/staff")]
[ApiController]
[Authorize(Roles = "manager")]
public class StaffController(IStaffService staffService) : ControllerBase
{
    [HttpGet("{id?}")]
    public async Task<IActionResult> Get(int? id)
    {
        if (id.HasValue)
            return Ok(await staffService.GetByIdAsync(id.Value));

        return Ok(await staffService.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertStaffDto staff)
    {
        await staffService.AddAsync(staff);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertStaffDto staff)
    {
        if (staff.Id != id)
            return BadRequest(new { error = "Mismatched ID" });

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
