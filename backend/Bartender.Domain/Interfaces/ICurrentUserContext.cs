using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface ICurrentUserContext
{
    int? UserId { get; }
    Task<Staff?> GetCurrentUserAsync();
    bool IsGuest { get; }
    string? GetRawToken();
}
