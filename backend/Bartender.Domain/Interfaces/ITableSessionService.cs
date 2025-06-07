using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface ITableSessionService
{
    Task<bool> HasActiveSessionAsync(int tableId, string token);
    Task<GuestSession?> GetConflictingSessionAsync(string token, int tableId);
}
