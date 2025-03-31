using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bartender.Domain;

public class CurrentUserContext(
    IHttpContextAccessor httpContextAccessor,
    IRepository<Staff> staffRepo
    ) : ICurrentUserContext
{
    private Staff? _cachedStaff;

    public int? UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userId, out var id) ? id : null;
        }
    }

    public async Task<Staff?> GetCurrentUserAsync()
    {
        if (IsGuest)
            return null;

        return _cachedStaff ??= await staffRepo.GetByIdAsync(UserId!.Value, true)
            ?? throw new UnauthorizedAccessException("User not found.");
    }

    public bool IsGuest
    {
        get
        {
            var role = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return string.IsNullOrEmpty(role) || role.Equals("guest", StringComparison.OrdinalIgnoreCase);
        }
    }

    public string? GetRawToken()
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        return authHeader?.Replace("Bearer ", "");
    }
}
