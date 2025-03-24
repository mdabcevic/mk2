using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface ICurrentUserContext
{
    int UserId { get; }
    Task<Staff> GetCurrentUserAsync();        // Optionally include Place & Business navigation
    Task<int> GetUserPlaceIdAsync();
    Task<int> GetUserBusinessIdAsync();
}
