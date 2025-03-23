using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController(IStaffService staffService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var staff = await staffService.GetByIdAsync(id);
            return staff != null ? Ok(staff) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var staff = await staffService.GetAllAsync();
            return Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Staff staff)
        {
            await staffService.AddAsync(staff);
            return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Staff staff)
        {
            if (staff.Id != id)
                return BadRequest();

            await staffService.UpdateAsync(staff);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await staffService.DeleteAsync(id);
            return NoContent();
        }
    }
}
