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
    public async Task<bool> HasActiveSessionAsync(int tableId, string token)
    {
        var session = await guestSessionRepo.GetByKeyAsync(s =>
            s.TableId == tableId && s.Token == token && s.IsValid);
        return session is not null;
    }

    public async Task<GuestSession?> GetConflictingSessionAsync(string token, int tableId)
    {
        var session = await guestSessionRepo.GetByKeyAsync(s =>
            s.Token == token && s.IsValid && s.TableId != tableId);
        return session;
    }

    //TODO: refactor this one a bit
    public async Task<string> CreateSessionAsync(int tableId, string passphrase)
    {
        if (string.IsNullOrEmpty(passphrase))
        {
            throw new ArgumentNullException(nameof(passphrase));
        }

        // Try to find existing group for this table
        var group = await groupSessionRepo.Query()
            .Where(g => g.TableId == tableId)
            .FirstOrDefaultAsync();

        if (group != null && group.Passphrase != passphrase)
        {
            logger.LogWarning("Attempted to join Table {TableId} with incorrect passphrase.", tableId);
            throw new InvalidOperationException("Incorrect passphrase for this table.");
        }

        // group doesnt exist
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
        var expiresAt = DateTime.UtcNow.AddMinutes(120); //TODO: remove this...
        var token = jwtService.GenerateGuestToken(tableId, sessionId, expiresAt, passphrase);

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

    public async Task RevokeSessionAsync(Guid sessionId)
    {
        var session = await guestSessionRepo.GetByKeyAsync(s => s.Id == sessionId);
        if (session is not null && session.IsValid)
        {
            session.IsValid = false;
            await guestSessionRepo.UpdateAsync(session);
            logger.LogInformation("Session {SessionId} revoked", session.Id);
        }
    }

    public async Task RevokeAllSessionsForTableAsync(int tableId)
    {
        var sessions = await guestSessionRepo.Query()
            .Where(s => s.TableId == tableId && s.IsValid)
            .ToListAsync();

        if (sessions.Count == 0)
        {
            logger.LogInformation("No active sessions to revoke for Table {TableId}", tableId);
            return;
        }

        foreach (var session in sessions)
        {
            session.IsValid = false;
            //TODO: horrible fix - find a way to deal with postgres timestamps.
            session.CreatedAt = DateTime.SpecifyKind(session.CreatedAt, DateTimeKind.Utc);
            session.ExpiresAt = DateTime.SpecifyKind(session.ExpiresAt, DateTimeKind.Utc);
        }
            
        await guestSessionRepo.UpdateRangeAsync(sessions);

        logger.LogInformation("Revoked {Count} sessions for Table {TableId}", sessions.Count, tableId);
    }

    public async Task<GuestSession?> GetByTokenAsync(int tableId, string token)
    {
        return await guestSessionRepo.GetByKeyAsync(s => s.TableId == tableId && s.Token == token);
    }

    public async Task EndGroupSessionAsync(int tableId)
    {
        // 1. Revoke all valid sessions
        await RevokeAllSessionsForTableAsync(tableId);

        // 2. Delete the group session
        var group = await groupSessionRepo.Query()
            .FirstOrDefaultAsync(g => g.TableId == tableId);

        if (group is not null)
        {
            await groupSessionRepo.DeleteAsync(group);
            logger.LogInformation("Deleted group session {GroupId} for Table {TableId}", group.Id, tableId);
        }
    }
}