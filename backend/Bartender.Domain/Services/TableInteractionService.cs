using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class TableInteractionService(
    IRepository<Tables> repository, 
    IGuestSessionService guestSessionService,
    ITableSessionService tableSessionService,
    ILogger<TableInteractionService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
) : ITableInteractionService
{
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
    public async Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt)
    {
        var table = await repository.GetByKeyAsync(t => t.QrSalt == salt && !t.IsDisabled);
        if (table is null)
        {
            logger.LogWarning("QR lookup failed: Stale Token was used: {Token}", salt);
            return ServiceResult<TableScanDto>.Fail("Invalid QR code", ErrorType.NotFound);
        }

        // Case 1: Staff scan
        if (!currentUser.IsGuest)
        {
            table.Status = TableStatus.occupied;
            await repository.UpdateAsync(table);
            logger.LogInformation("Table {Id} marked as occupied by staff.", table.Id); //TODO: logic for shared passkey?

            var dto = mapper.Map<TableScanDto>(table);
            return ServiceResult<TableScanDto>.Ok(dto);
        }

        // Case 2: Guest scan
        var token = currentUser.GetRawToken(); //returns null if fresh situation

        if (table.IsDisabled)
        {
            logger.LogWarning("QR scan blocked for disabled Table {TableId}", table.Id);
            return ServiceResult<TableScanDto>.Fail("QR for this table is currently unavailable. Waiter is coming.", ErrorType.Unauthorized);
        }

        if (table.Status == TableStatus.occupied)
        {
            if (await tableSessionService.HasActiveSessionAsync(table.Id))
            {
                if (await tableSessionService.IsSameTokenAsActiveAsync(table.Id, token))
                {
                    logger.LogInformation("Guest resumed active session for Table {TableId}", table.Id);
                    var dto = mapper.Map<TableScanDto>(table);
                    dto.GuestToken = token;
                    return ServiceResult<TableScanDto>.Ok(dto);
                }

                logger.LogWarning("QR scan denied for Table {TableId} — session already active for another user", table.Id);
                return ServiceResult<TableScanDto>.Fail("This table is currently in use.", ErrorType.Conflict);
            }

            if (!await tableSessionService.CanResumeExpiredSessionAsync(table.Id, token))
            {
                logger.LogWarning("QR scan denied — expired session exists but token does not match. Table {TableId}", table.Id);
                return ServiceResult<TableScanDto>.Fail("Table is still in use.", ErrorType.Conflict);
            }

            logger.LogInformation("Guest at Table {TableId} is resuming session after expiry", table.Id);
        }

        // Proceed with creating a new session
        table.Status = TableStatus.occupied;
        await repository.UpdateAsync(table);

        var newToken = await guestSessionService.CreateSessionAsync(table.Id);

        var resultDto = mapper.Map<TableScanDto>(table);
        resultDto.GuestToken = newToken;
        return ServiceResult<TableScanDto>.Ok(resultDto);
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
                logger.LogWarning("Guest attempted to change table {Id} without token", token); // sanity check
                return ServiceResult.Fail("Missing authentication token.", ErrorType.Unauthorized);
            }

            var session = await guestSessionService.GetByTokenAsync(table.Id, accesstoken);

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

            if (table.Status == TableStatus.empty)
            {
                logger.LogInformation("Guest attempted to free an already empty Table {Id}", table.Id);
                return ServiceResult.Ok(); // or a NoOp result
            }

            logger.LogInformation("Guest freed Table {Id} via valid session", table.Id);
            await guestSessionService.DeleteSessionAsync(session.Id);
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

    //TODO: have an API /call-waiter that pushes a notification to SignalR group of entire place for waiters to see.

    private async Task<bool> IsSameBusinessAsync(int placeId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return placeId == user!.PlaceId;
    }
}
