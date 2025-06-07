using Bartender.Data.Enums;
using Bartender.Domain.DTO.Business;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/business")]
[ApiController]
public class BusinessController(IBusinessService businessService) : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(Roles = "owner, admin, manager")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await businessService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> GetAll()
    {
        var result = await businessService.GetAllAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertBusinessDto dto)
    {
        await businessService.AddAsync(dto);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "manager, owner")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertBusinessDto dto)
    {
        await businessService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpPatch("subscription")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateSubscription([FromBody] SubscriptionTier tier)
    {
        await businessService.UpdateSubscriptionAsync(tier);
        return NoContent();
    }
}
