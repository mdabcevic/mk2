using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bartender.Domain.Services;

public class CurrentUserContext(
    IHttpContextAccessor httpContextAccessor,
    IRepository<Staff> staffRepo,
    IRepository<Places> placeRepo) : ICurrentUserContext
{
    private Staff? _cachedStaff;

    public int UserId
    {
        get
        {
            var claims = httpContextAccessor.HttpContext?.User.Claims;
            foreach (var claim in claims!)
            {
                Console.WriteLine($"CLAIM: {claim.Type} = {claim.Value}");
            }

            return int.Parse(
                httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token."));
        }
    }

    public async Task<Staff> GetCurrentUserAsync()
    {
        return _cachedStaff ??= await staffRepo.GetByIdAsync(UserId, includeNavigations: true)
            ?? throw new UnauthorizedAccessException("User not found.");
    }

    public async Task<int> GetUserPlaceIdAsync() => (await GetCurrentUserAsync()).PlaceId;

    public async Task<int> GetUserBusinessIdAsync()
    {
        var place = await placeRepo.GetByIdAsync(await GetUserPlaceIdAsync());
        return place?.BusinessId ?? throw new UnauthorizedAccessException("User's place or business not found.");
    }
}
