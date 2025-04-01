using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class GuestSessionService(
    IRepository<GuestSession> guestSessionRepo,
    IJwtService jwtService,
    ILogger<GuestSessionService> logger
) : IGuestSessionService, ITableSessionService
{
    public async Task<bool> HasActiveSessionAsync(int tableId)
    {
        var active = await guestSessionRepo.GetByKeyAsync(s =>
            s.TableId == tableId && s.ExpiresAt > DateTime.UtcNow);
        return active != null;
    }

    public async Task<bool> IsSameTokenAsActiveAsync(int tableId, string token)
    {
        var active = await guestSessionRepo.GetByKeyAsync(s =>
            s.TableId == tableId && s.ExpiresAt > DateTime.UtcNow);
        return active?.Token == token;
    }

    public async Task<bool> CanResumeExpiredSessionAsync(int tableId, string token)
    {
        var latestExpired = await guestSessionRepo.Query()
            .Where(s => s.TableId == tableId)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync();

        return latestExpired?.Token == token;
    }

    public async Task<string> CreateSessionAsync(int tableId)
    {
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var token = jwtService.GenerateGuestToken(tableId, sessionId, expiresAt);

        var session = new GuestSession
        {
            Id = sessionId,
            TableId = tableId,
            Token = token,
            ExpiresAt = expiresAt
        };

        await guestSessionRepo.AddAsync(session);
        logger.LogInformation("New guest session started: Table {TableId}, Session {SessionId}, Expires {Expires}",
            tableId, sessionId, expiresAt);

        return token;
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var session = await guestSessionRepo.GetByKeyAsync(s => s.Id == sessionId);
        if (session != null)
        {
            await guestSessionRepo.DeleteAsync(session);
            logger.LogInformation("Deleted guest session {SessionId}", session.Id);
        }
    }

    public async Task<GuestSession?> GetLatestExpiredSessionAsync(int tableId)
    {
        return await guestSessionRepo.Query()
            .Where(s => s.TableId == tableId)
            .OrderByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    public async Task<GuestSession?> GetByTokenAsync(int tableId, string token)
    {
        return await guestSessionRepo.GetByKeyAsync(s => s.TableId == tableId && s.Token == token);
    }
}

