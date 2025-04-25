using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class AuthService(
    IRepository<Staff> staffRepo,
    ILogger<AuthService> logger,
    IJwtService jwtService
    ) : IAuthService
{
    public async Task<ServiceResult<string>> LoginAsync(LoginStaffDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            return ServiceResult<string>.Fail("Username and password are required.", ErrorType.Validation);

        var staff = await staffRepo.GetByKeyAsync(s => s.Username == loginDto.Username);
        if (staff is null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.Password))
        {
            logger.LogWarning("Invalid login for username: {Username}", loginDto.Username);
            return ServiceResult<string>.Fail("Invalid username or password.", ErrorType.Validation);
        }

        var token = jwtService.GenerateStaffToken(staff);
        logger.LogInformation("Login success: {Username} (PlaceId: {PlaceId}, Role: {Role})",
            staff!.Username, staff.PlaceId, staff.Role);
        return ServiceResult<string>.Ok(token);
    }
}
