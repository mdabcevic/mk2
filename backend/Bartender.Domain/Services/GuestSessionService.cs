using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class GuestSessionService(
    IRepository<GuestSession> guestSessionRepo,
    IRepository<GuestSessionGroup> groupSessionRepo,
    IJwtService jwtService,
    ILogger<GuestSessionService> logger
) : IGuestSessionService, ITableSessionService
{
    public async Task<bool> HasActiveSessionAsync(int tableId)
    {
        return await GetActiveSessionAsync(tableId) is not null;
    }

    public async Task<bool> IsSameTokenAsActiveAsync(int tableId, string token)
    {
        var active = await GetActiveSessionAsync(tableId);
        return active?.Token == token;
    }

    private async Task<GuestSession?> GetActiveSessionAsync(int tableId)
    {
        return await guestSessionRepo.GetByKeyAsync(s =>
            s.TableId == tableId && s.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<bool> CanResumeExpiredSessionAsync(int tableId, string token)
    {
        var latestExpired = await GetLatestExpiredSessionAsync(tableId);
        return latestExpired?.Token == token;
    }

    public async Task<string> CreateSessionAsync(int tableId, string passphrase) // always send passphrase, on new group session and existing?
    {
        if (string.IsNullOrEmpty(passphrase))
        {
            throw new ArgumentNullException(nameof(passphrase));
        }

        // Try to find existing group for this table
        var group = await groupSessionRepo.Query()
            .Where(g => g.TableId == tableId && g.Passphrase == passphrase)
            .FirstOrDefaultAsync();

        // group doesnt exist - ensure that passphrase isn't null!!!
        if (group == null)
        {
            // First user: create a new group
            group = new GuestSessionGroup
            {
                TableId = tableId,
                Passphrase = passphrase
            };
            await groupSessionRepo.AddAsync(group);
            logger.LogInformation("New group session started: Table {TableId}, Group {GroupId}, Passphrase {Passphrase}.",
            tableId, group.Id, passphrase);
        }

        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var token = jwtService.GenerateGuestToken(tableId, sessionId, expiresAt);

        var session = new GuestSession
        {
            Id = sessionId,
            TableId = tableId,
            GroupId = group.Id,
            Token = token,
            ExpiresAt = expiresAt
        };

        await guestSessionRepo.AddAsync(session);
        logger.LogInformation("New guest joined a group session: Table {TableId}, Group {GroupId}, Session {SessionId}, Expires {Expires}",
            tableId, group.Id, sessionId, expiresAt);

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