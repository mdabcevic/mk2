using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class AuthService(
    IRepository<Staff> staffRepo,
    ILogger<AuthService> logger,
    IJwtService jwtService
    ) : IAuthService
{
    public async Task<string> LoginAsync(LoginStaffDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
            throw new AppValidationException("Username and password are required.");

        var staff = await staffRepo.GetByKeyAsync(s => s.Username == loginDto.Username);
        if (staff is null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.Password))
        {
            logger.LogWarning("Invalid login for username: {Username}", loginDto.Username);
            throw new AppValidationException("Invalid username or password.");
        }

        var token = jwtService.GenerateStaffToken(staff);
        logger.LogInformation("Login success: {Username} (PlaceId: {PlaceId}, Role: {Role})",
            staff!.Username, staff.PlaceId, staff.Role);
        return token;
    }
}
