using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IGuestSessionService
{
    Task<string> CreateSessionAsync(int tableId, string passphrase);
    Task DeleteSessionAsync(Guid sessionId);
    Task<GuestSession?> GetLatestExpiredSessionAsync(int tableId);
    Task<GuestSession?> GetByTokenAsync(int tableId, string token);
}
