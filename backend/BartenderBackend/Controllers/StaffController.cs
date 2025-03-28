using Bartender.Domain.DTO;
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
            return (await staffService.GetByIdAsync(id.Value)).ToActionResult();

        return (await staffService.GetAllAsync()).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertStaffDto staff)
    {
        return (await staffService.AddAsync(staff)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertStaffDto staff)
    {
        if (staff.Id != id)
            return BadRequest(new { error = "Mismatched ID" });

        return (await staffService.UpdateAsync(id, staff)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await staffService.DeleteAsync(id)).ToActionResult();
    }
}
