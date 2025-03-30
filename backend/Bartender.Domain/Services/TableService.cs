using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class TableService(
    IRepository<Tables> repository,
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
    /// Fetches a table by ID.
    /// </summary>
    /// <param name="id">Table ID managed by database.</param>
    /// <returns></returns>
    public async Task<ServiceResult<TableDto>> GetByIdAsync(int id)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult<TableDto>.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult<TableDto>.Fail("Unauthorized", ErrorType.Unauthorized);

        return ServiceResult<TableDto>.Ok(mapper.Map<TableDto>(table));
    }

    /// <summary>
    /// Fetches a table on user scan and validates session.
    /// </summary>
    /// <param name="salt">Rotation token used in QR generation.</param>
    /// <returns>Table information and JWT for guest session.</returns>
    public async Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt)
    {
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
    /// <param name="id">Database-managed ID.</param>
    /// <param name="dto">Contains data for modification.</param>
    /// <returns></returns>
    public async Task<ServiceResult> UpdateAsync(int id, UpsertTableDto dto)
    {
        //TODO: label can't get updated. Consider patch or different DTO.
        var table = await repository.GetByIdAsync(id);
        if (table is null)
        {
            logger.LogWarning("Update failed: Table {Id} not found", id);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        if (!await IsSameBusinessAsync(table.PlaceId))
        {
            logger.LogWarning("Unauthorized update attempt by user {UserId} on Table {TableId}", currentUser.UserId, id);
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);
        }

        table.Seats = dto.Seats;
        await repository.UpdateAsync(table);
        logger.LogInformation("Table {Id} updated by user {UserId}", id, currentUser.UserId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
        {
            logger.LogWarning("Attempted to delete Table {Id}, but it was not found", id);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        if (!await IsSameBusinessAsync(table.PlaceId))
        {
            logger.LogWarning("Unauthorized delete attempt on Table {Id} by User {UserId}", id, currentUser.UserId);
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);
        }

        await repository.DeleteAsync(table);
        logger.LogInformation("Table {Id} deleted by User {UserId}", id, currentUser.UserId);
        return ServiceResult.Ok();
    }

    /// <summary>
    /// Changes the current state of table.
    /// </summary>
    /// <param name="id">Database-generated table id.</param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    public async Task<ServiceResult> ChangeStatusAsync(int id, TableStatus newStatus)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
        {
            logger.LogWarning("ChangeStatus failed: Table {Id} not found", id);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        if (currentUser.IsGuest)
        {
            var token = currentUser.GetRawToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("Guest attempted to change table {Id} without token", id);
                return ServiceResult.Fail("Missing authentication token.", ErrorType.Unauthorized);
            }

            var session = await guestSessionRepo.GetByKeyAsync(s =>
                s.TableId == id && s.Token == token);

            if (session is null || session.ExpiresAt < DateTime.UtcNow)
            {
                logger.LogWarning("Invalid or expired session for guest trying to change status on Table {Id}", id);
                return ServiceResult.Fail("Unauthorized or expired session.", ErrorType.Unauthorized);
            }

            if (newStatus != TableStatus.empty)
            {
                logger.LogWarning("Guest tried to set status to {Status} on Table {Id} — only 'empty' is allowed", newStatus, id);
                return ServiceResult.Fail("Guests can only free tables.", ErrorType.Unauthorized);
            }
            logger.LogInformation("Guest freed Table {Id} via valid session", id);
        }
        else
        {
            var user = await currentUser.GetCurrentUserAsync();
            if (!await IsSameBusinessAsync(table.PlaceId))
            {
                logger.LogWarning("Unauthorized staff (User {UserId}) tried to change status of Table {Id}", user.Id, id);
                return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);
            }
            logger.LogInformation("User {UserId} changed Table {Id} status to {NewStatus}", user.Id, id, newStatus);
        }

        table.Status = newStatus;
        await repository.UpdateAsync(table);
        return ServiceResult.Ok();
    }



    public async Task<ServiceResult> RegenerateSaltAsync(int id)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);

        table.QrSalt = Guid.NewGuid().ToString("N");
        await repository.UpdateAsync(table);

        logger.LogInformation("Salt regenerated for Table {Id}", id);
        return ServiceResult.Ok();
    }


    public async Task<ServiceResult> SwitchDisabledAsync(int id, bool flag)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
        {
            logger.LogWarning("Failed to change disabled state: Table {Id} not found", id);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        if (!await IsSameBusinessAsync(table.PlaceId))
        {
            logger.LogWarning("Unauthorized attempt to change disabled state of Table {Id} by User {UserId}", id, currentUser.UserId);
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);
        }

        table.IsDisabled = flag;
        await repository.UpdateAsync(table);

        logger.LogInformation("Table {Id} set to disabled = {Flag} by User {UserId}", id, flag, currentUser.UserId);
        return ServiceResult.Ok();
    }


    private async Task<bool> IsSameBusinessAsync(int placeId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return placeId == user!.PlaceId;
    }
}
