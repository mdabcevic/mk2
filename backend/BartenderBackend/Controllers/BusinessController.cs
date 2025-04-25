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
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> GetAll()
    {
        var result = await businessService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertBusinessDto dto)
    {
        var result = await businessService.AddAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "manager,owner")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertBusinessDto dto)
    {
        var result = await businessService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpPatch("subscription")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateSubscription([FromBody] SubscriptionTier tier)
    {
        var result = await businessService.UpdateSubscriptionAsync(tier);
        return result.ToActionResult();
    }

    //[HttpDelete("{id}")]
    //[Authorize(Roles = "owner")]
    //public async Task<IActionResult> Delete(int id)
    //{
    //    var result = await businessService.DeleteAsync(id);
    //    return result.ToActionResult();
    //}
}
