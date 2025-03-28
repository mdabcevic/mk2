using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/places")]
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

    [Authorize(Roles = "manager")] //switch to admin/owner maybe
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InsertPlaceDto dto)
    {
        return (await placesService.AddAsync(dto)).ToActionResult();
    }

    [Authorize(Roles = "manager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlaceDto dto)
    {
        return (await placesService.UpdateAsync(id, dto)).ToActionResult();
    }

    [Authorize(Roles = "manager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return (await placesService.DeleteAsync(id)).ToActionResult();
    }
}
