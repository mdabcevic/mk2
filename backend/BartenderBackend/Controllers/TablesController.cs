using Bartender.Data.Enums;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize]
public class TablesController(
    ITableInteractionService tableInteractionService,
    ITableManagementService tableManagementService) : ControllerBase
{
    [HttpDelete("{label}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Delete(string label)
    {
        await tableManagementService.DeleteAsync(label);
        return NoContent();
    }

    [HttpPost("bulk-upsert")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> BulkUpsert([FromBody] List<UpsertTableDto> tables)
    {
        if (tables == null || tables.Count == 0)
            return BadRequest("No table data provided.");

        await tableManagementService.BulkUpsertAsync(tables);
        return NoContent();
    }

    [HttpGet("{label}")]
    public async Task<IActionResult> GetById(string label)
    {
        var result = await tableManagementService.GetByLabelAsync(label);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await tableManagementService.GetAllAsync();
        return Ok(result);
    }

    //TODO: should this be in place/tables instead?
    [HttpGet("{placeId}/all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTablesByPlaceId(int placeId)
    {
        var result = await tableManagementService.GetByPlaceId(placeId);
        return Ok(result);
    }

    [HttpGet("lookup")]
    [AllowAnonymous] // guests scan QR
    public async Task<IActionResult> GetBySalt([FromQuery] string salt, [FromQuery] string? passphrase = null)
    {
        var result = await tableInteractionService.GetBySaltAsync(salt, passphrase);
        return Ok(result);
    }

    [HttpPost("{label}/rotate-token")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> RegenerateSalt(string label)
    {
        var result = await tableManagementService.RegenerateSaltAsync(label);
        return Ok(result);
    }

    [HttpPatch("{label}/toggle-disabled")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> SetDisabled(string label, [FromBody] bool disable)
    {
        await tableManagementService.SwitchDisabledAsync(label, disable);
        return NoContent();
    }

    [AllowAnonymous] // TODO: check if this is needed
    [HttpPatch("{token}/status")]
    public async Task<IActionResult> ChangeStatus(string token, [FromBody] TableStatus status)
    {
        await tableInteractionService.ChangeStatusAsync(token, status);
        return NoContent();
    }
}
