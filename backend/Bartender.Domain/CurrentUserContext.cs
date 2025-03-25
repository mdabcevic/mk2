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

    public int UserId
    {
        get
        {
            return int.Parse(
                httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token."));
        }
    }

    public async Task<Staff> GetCurrentUserAsync()
    {
        return _cachedStaff ??= await staffRepo.GetByIdAsync(UserId, true)
            ?? throw new UnauthorizedAccessException("User not found.");
    }
}
