using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlacesController(IPlacesService placesService) : ControllerBase
{
    [HttpGet("{id?}")]
    public async Task<IActionResult> Get(int? id)
    {
        if (id.HasValue)
            return (await placesService.GetByIdAsync(id.Value)).ToActionResult();

        return (await placesService.GetAllAsync()).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertPlaceDto place)
    {
        return (await placesService.AddAsync(place)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertPlaceDto place)
    {
        if (place.Id != id)
            return BadRequest(new { error = "Mismatched ID" });

        return (await placesService.UpdateAsync(id, place)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await placesService.DeleteAsync(id)).ToActionResult();
    }
}
