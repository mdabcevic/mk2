using Bartender.Domain.DTO.Place;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/places")]
[ApiController]
public class PlacesController(IPlaceService placesService) : ControllerBase
{
    [HttpGet("{id?}")]
    public async Task<IActionResult> Get(int? id)
    {
        if (id.HasValue)
            return Ok(await placesService.GetByIdAsync(id.Value));

        return Ok(await placesService.GetAllAsync());
    }

    [HttpGet("notify-staff/{salt}")]
    public async Task<IActionResult> NotifyStaff(string salt)
    {
        await placesService.NotifyStaffAsync(salt);
        return NoContent();
    }

    [Authorize(Roles = "manager")] //switch to admin/owner maybe
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InsertPlaceDto dto)
    {
        await placesService.AddAsync(dto);
        return NoContent();
    }

    [Authorize(Roles = "manager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlaceDto dto)
    {
        await placesService.UpdateAsync(id, dto);
        return NoContent();
    }

    [Authorize(Roles = "manager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await placesService.DeleteAsync(id);
        return NoContent();
    }
}
