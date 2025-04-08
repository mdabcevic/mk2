using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IGuestSessionService
{
    Task<string> CreateSessionAsync(int tableId, string? passphrase = null);
    Task DeleteSessionAsync(Guid sessionId);
    Task<GuestSession?> GetLatestExpiredSessionAsync(int tableId);
    Task<GuestSession?> GetByTokenAsync(int tableId, string token);
}
