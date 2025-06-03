using Bartender.Data.Enums;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BartenderBackend.Controllers;

[Route("api/analytics")]
[ApiController]
[Authorize(Roles = "admin, owner, manager")]
public class AnalyticsController(IAnalyticsServer analyticsServer) : ControllerBase
{
    [HttpGet("products/{placeId?}")]
    public async Task<IActionResult> GetPopularProducts(int? placeId, [FromQuery] int? month, [FromQuery]int? year)
    {
        var result = await analyticsServer.GetPopularProductsByDayOfWeek(placeId, month, year);
        return Ok(result);
    }

    [HttpGet("traffic/daily/{placeId?}")]
    public async Task<IActionResult> GetWeeklyTraffic(int? placeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await analyticsServer.GetTrafficByDayOfWeek(placeId, month, year);
        return Ok(result);
    }

    [HttpGet("traffic/hourly/{placeId?}")]
    public async Task<IActionResult> GetHourlyTraffic(int? placeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await analyticsServer.GetHourlyTraffic(placeId, month, year);
        return Ok(result);
    }

    [HttpGet("traffic/table/{placeId}")]
    public async Task<IActionResult> GetTableTraffic(int placeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await analyticsServer.GetTableTraffic(placeId, month, year);
        return Ok(result);
    }

    [HttpGet("traffic")]
    public async Task<IActionResult> GetAllPlacesTraffic([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await analyticsServer.GetAllPlacesTraffic(month, year);
        return Ok(result);
    }

    /*[HttpGet("earnings/{placeId?}")]
    public async Task<IActionResult> GetTotalEarnings(int? placeId, [FromQuery] DateTime? dateTime, [FromQuery] TimeFilter? timeFilter)
    {
        var result = await analyticsServer.GetTotalEarnings(dateTime, timeFilter, placeId);
        return Ok(result);
    }*/

    [HttpGet("key-values/{placeId?}")]
    public async Task<IActionResult> GetKeyValues(int? placeId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await analyticsServer.GetAllInfo(placeId, month, year);
        return Ok(result);
    }
}
