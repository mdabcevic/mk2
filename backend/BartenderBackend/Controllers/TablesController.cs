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
        var result = await tableManagementService.DeleteAsync(label);
        return result.ToActionResult();
    }

    [HttpPost("bulk-upsert")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> BulkUpsert([FromBody] List<UpsertTableDto> tables)
    {
        if (tables == null || tables.Count == 0)
            return BadRequest("No table data provided.");

        var result = await tableManagementService.BulkUpsertAsync(tables);
        return result.ToActionResult();
    }

    [HttpGet("{label}")]
    public async Task<IActionResult> GetById(string label)
    {
        var result = await tableManagementService.GetByLabelAsync(label);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await tableManagementService.GetAllAsync();
        return result.ToActionResult();
    }

    //TODO: should this be in place/tables instead?
    [HttpGet("{placeId}/all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTablesByPlaceId(int placeId)
    {
        var result = await tableManagementService.GetByPlaceId(placeId);
        return result.ToActionResult();
    }

    [HttpGet("lookup")]
    [AllowAnonymous] // guests scan QR
    public async Task<IActionResult> GetBySalt([FromQuery] string salt, [FromQuery] string? passphrase = null)
    {
        var result = await tableInteractionService.GetBySaltAsync(salt, passphrase);
        return result.ToActionResult();
    }

    [HttpPost("{label}/rotate-token")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> RegenerateSalt(string label)
    {
        var result = await tableManagementService.RegenerateSaltAsync(label);
        return result.ToActionResult();
    }

    [HttpPatch("{label}/toggle-disabled")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> SetDisabled(string label, [FromBody] bool disable)
    {
        var result = await tableManagementService.SwitchDisabledAsync(label, disable);
        return result.ToActionResult();
    }

    [AllowAnonymous] // TODO: check if this is needed
    [HttpPatch("{token}/status")]
    public async Task<IActionResult> ChangeStatus(string token, [FromBody] TableStatus status)
    {
        var result = await tableInteractionService.ChangeStatusAsync(token, status);
        return result.ToActionResult();
    }
}
