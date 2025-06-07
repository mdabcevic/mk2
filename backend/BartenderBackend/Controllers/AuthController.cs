using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BartenderBackend.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(
    IAuthService authService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LoginStaffDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }
}
