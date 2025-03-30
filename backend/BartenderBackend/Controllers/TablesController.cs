using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using BartenderBackend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize]
public class TablesController(ITableService tableService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await tableService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await tableService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("lookup")]
    [AllowAnonymous] // guests scan QR
    public async Task<IActionResult> GetBySalt([FromQuery] string salt)
    {
        var result = await tableService.GetBySaltAsync(salt);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Add([FromBody] UpsertTableDto dto)
    {
        var result = await tableService.AddAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertTableDto dto)
    {
        var result = await tableService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await tableService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] TableStatus status)
    {
        var result = await tableService.ChangeStatusAsync(id, status);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/resalt")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> RegenerateSalt(int id)
    {
        var result = await tableService.RegenerateSaltAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/enable")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Enable(int id)
    {
        var result = await tableService.EnableAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Disable(int id)
    {
        var result = await tableService.DisableAsync(id);
        return result.ToActionResult();
    }
}
