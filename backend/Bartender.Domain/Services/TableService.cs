using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Domain.Services;

public class TableService(
    IRepository<Tables> repository, //TODO: consider separate repository for table - many queries and validation.
    IRepository<GuestSession> guestSessionRepo,
    ILogger<TableService> logger,
    IJwtService jwtService,
    ICurrentUserContext currentUser,
    IMapper mapper
) : ITableService
{
    /// <summary>
    /// Gets tables for current user’s place
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<List<TableDto>>> GetAllAsync()
    {
        var user = await currentUser.GetCurrentUserAsync();
        var tables = await repository.GetAllAsync();

        var filtered = tables
            .Where(t => t.PlaceId == user!.PlaceId)
            .Select(t => mapper.Map<TableDto>(t))
            .ToList();

        return ServiceResult<List<TableDto>>.Ok(filtered);
    }

    /// <summary>
    /// Fetches a table by label assigned by staff (unique per place).
    /// </summary>
    /// <param name="label">Staff assigned label for table in a place.</param>
    /// <returns></returns>
    public async Task<ServiceResult<TableDto>> GetByLabelAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var table = await repository.GetByKeyAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));

        if (table is null)
        {
            logger.LogWarning("Table with label '{Label}' not found for Place {PlaceId}", label, user!.PlaceId);
            return ServiceResult<TableDto>.Fail("Table not found", ErrorType.NotFound);
        }
        return ServiceResult<TableDto>.Ok(mapper.Map<TableDto>(table));
    }

    /// <summary>
    /// Fetches a table on user scan and validates session.
    /// </summary>
    /// <param name="salt">Rotation token used in QR generation.</param>
    /// <returns>Table information and JWT for guest session.</returns>
    public async Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt) //TODO: staff has to make an order instead of user? 
    {
        //on manager scan - mark as empty + occupy? to clear out latest session? mark order as staff-placed?
        // look whether Token from QR matches the table.
        var table = await repository.GetByKeyAsync(t => t.QrSalt == salt && !t.IsDisabled);
        if (table is null)
        {
            logger.LogWarning("QR lookup failed: Stale Token was used: {Token}", salt);
            return ServiceResult<TableScanDto>.Fail("Invalid QR code", ErrorType.NotFound);
        }

        if (table.IsDisabled)
        {
            logger.LogWarning("QR scan blocked for disabled Table {TableId}", table.Id);
            return ServiceResult<TableScanDto>.Fail("QR for this table is currently unavailable. Waiter is coming.", ErrorType.Unauthorized);
        }

        // if table is "still" in use, stop new requests from "hijacking" the table.
        if (table.Status == TableStatus.occupied)
        {
            var activeSession = await guestSessionRepo.GetByKeyAsync(s =>
                s.TableId == table.Id && s.ExpiresAt > DateTime.UtcNow);

            // for simplicity purpose - only 1 session per table is allowed.
            if (activeSession is not null)
            {
                logger.LogWarning("QR scan denied for Table {TableId} — session already active (until {Expires})", table.Id, activeSession.ExpiresAt);
                return ServiceResult<TableScanDto>.Fail("This table is currently in use.", ErrorType.Conflict);
            }

            // Table is marked as occupied, but no active session — might be expired.
            var latestExpired = await guestSessionRepo.Query()
                .Where(s => s.TableId == table.Id)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync();

            var presentedToken = currentUser.GetRawToken();
            if (latestExpired != null && latestExpired.Token == presentedToken)
            {
                logger.LogInformation("Guest at Table {TableId} is resuming session after expiry", table.Id);
                // Allow session to resume — continue below
            }
            else
            {
                logger.LogWarning("QR scan denied — expired session exists but token does not match. Table {TableId}", table.Id);
                return ServiceResult<TableScanDto>.Fail("Table is still in use.", ErrorType.Conflict);
            }
        }

        // occupy table immediately on scan - prevents other people from accessing it.
        table.Status = TableStatus.occupied;
        await repository.UpdateAsync(table);

        // start new session for user who scanned the QR
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var token = jwtService.GenerateGuestToken(table.Id, sessionId, expiresAt); // string returned to user - session validation

        var session = new GuestSession
        {
            Id = sessionId,
            TableId = table.Id,
            Token = token,
            ExpiresAt = expiresAt
        };

        await guestSessionRepo.AddAsync(session);
        logger.LogInformation("New guest session started: Table {TableId}, SessionId {SessionId}, ExpiresAt {ExpiresAt}", table.Id, sessionId, expiresAt);

        // return info needed on happy scan path
        var dto = mapper.Map<TableScanDto>(table);
        dto.GuestToken = token;
        return ServiceResult<TableScanDto>.Ok(dto);
    }

    public async Task<ServiceResult> AddAsync(UpsertTableDto dto)
    {
        //TODO: label should be unique per place.
        var user = await currentUser.GetCurrentUserAsync();
        if (await repository.ExistsAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(dto.Label, StringComparison.CurrentCultureIgnoreCase)))
        {
            return ServiceResult.Fail("This table label is already in use for your place.", ErrorType.Conflict);
        }

        var entity = mapper.Map<Tables>(dto);

        entity.PlaceId = user!.PlaceId;
        entity.Status = TableStatus.empty;
        entity.QrSalt = Guid.NewGuid().ToString("N");

        await repository.AddAsync(entity);
        logger.LogInformation("New table added by user {UserId}. Currently active token: {Token}", user.Id, entity.QrSalt);
        return ServiceResult.Ok();
    }

    /// <summary>
    /// Updates a table info based on provided details.
    /// </summary>
    /// <param name="label">label assigned by staff (unique per place).</param>
    /// <param name="dto">Contains data for modification.</param>
    /// <returns></returns>
    public async Task<ServiceResult> UpdateAsync(string label, UpsertTableDto dto)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByKeyAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));

        if (table is null)
        {
            logger.LogWarning("Update failed: Table '{Label}' not found for Place {PlaceId}", label, user!.PlaceId);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }
        table.Seats = dto.Seats;
        await repository.UpdateAsync(table);
        logger.LogInformation("Table '{Label}' updated by User {UserId}", label, user!.Id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var table = await repository.GetByKeyAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));

        if (table is null)
        {
            logger.LogWarning("Delete failed: Table '{Label}' not found for Place {PlaceId}", label, user!.PlaceId);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }
        await repository.DeleteAsync(table);
        logger.LogInformation("Table '{Label}' deleted by User {UserId}", label, user!.Id);
        return ServiceResult.Ok();
    }

    /// <summary>
    /// Changes the current state of table.
    /// </summary>
    /// <param name="token">Currently active token for accessing table.</param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    public async Task<ServiceResult> ChangeStatusAsync(string token, TableStatus newStatus) //TODO: consider separating api for user to free table.
    {
        var table = await repository.GetByKeyAsync(t => t.QrSalt == token);
        if (table is null)
        {
            logger.LogWarning("ChangeStatus failed: Table {Token} not found", token);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        if (currentUser.IsGuest)
        {
            var accesstoken = currentUser.GetRawToken();
            if (string.IsNullOrWhiteSpace(accesstoken))
            {
                logger.LogWarning("Guest attempted to change table {Id} without token", token);
                return ServiceResult.Fail("Missing authentication token.", ErrorType.Unauthorized);
            }

            var session = await guestSessionRepo.GetByKeyAsync(s =>
                s.TableId == table.Id && s.Token == accesstoken);

            if (session is null || session.ExpiresAt < DateTime.UtcNow)
            {
                logger.LogWarning("Invalid or expired session for guest trying to change status on Table {Id}", table.Id);
                return ServiceResult.Fail("Unauthorized or expired session.", ErrorType.Unauthorized);
            }

            if (newStatus != TableStatus.empty)
            {
                logger.LogWarning("Guest tried to set status to {Status} on Table {Id} — only 'empty' is allowed", newStatus, table.Id);
                return ServiceResult.Fail("Guests can only free tables.", ErrorType.Unauthorized);
            }
            logger.LogInformation("Guest freed Table {Id} via valid session", table.Id);
            await guestSessionRepo.DeleteAsync(session);
            logger.LogInformation("Session {Id} was deleted.", session.Id);
        }
        else
        {
            var user = await currentUser.GetCurrentUserAsync();
            if (!await IsSameBusinessAsync(table.PlaceId))
            {
                logger.LogWarning("Unauthorized staff (User {UserId}) tried to change status of Table {Id}", user!.Id, table.Id);
                return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);
            }
            logger.LogInformation("User {UserId} changed Table {Id} status to {NewStatus}", user!.Id, table.Id, newStatus);
        }

        table.Status = newStatus;
        await repository.UpdateAsync(table);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RegenerateSaltAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByKeyAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));

        if (table is null)
        {
            logger.LogWarning("Resalt failed: Table '{Label}' not found for Place {PlaceId}", label, user!.PlaceId);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        table.QrSalt = Guid.NewGuid().ToString("N");
        await repository.UpdateAsync(table);
        logger.LogInformation("Salt rotated for Table '{Label}' by User {UserId}", label, user!.Id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SwitchDisabledAsync(string label, bool flag)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var table = await repository.GetByKeyAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));

        if (table is null)
        {
            logger.LogWarning("Disable toggle failed: Table '{Label}' not found for Place {PlaceId}", label, user!.PlaceId);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        table.IsDisabled = flag;
        await repository.UpdateAsync(table);
        logger.LogInformation("Table '{Label}' disabled state set to {Flag} by User {UserId}", label, flag, user!.Id);
        return ServiceResult.Ok();
    }

    //TODO: have an API /call-waiter that pushes a notification to SignalR group of entire place for waiters to see.

    private async Task<bool> IsSameBusinessAsync(int placeId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return placeId == user!.PlaceId;
    }
}
