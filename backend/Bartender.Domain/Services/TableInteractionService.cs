using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.DTO.Table;
using Bartender.Data;

namespace Bartender.Domain.Services;

public class TableInteractionService(
    IRepository<Tables> repository,
    IGuestSessionService guestSessionService,
    ITableSessionService tableSessionService,
    IOrderRepository orderRepository,
    INotificationService notificationService,
    ILogger<TableInteractionService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
) : ITableInteractionService
{
    /// <summary>
    /// Starting point for table management upon QR Scan.
    /// </summary>
    /// <param name="salt">Rotating token used for QR - also unique identifier for table.</param>
    /// <returns></returns>
    public async Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt, string? passphrase = null)
    {
        var table = await repository.GetByKeyAsync(t => t.QrSalt == salt);
        if (table is null)
        {
            logger.LogWarning("QR lookup failed: Stale or invalid Token was used: {Token}", salt);
            return ServiceResult<TableScanDto>.Fail("Invalid QR code", ErrorType.NotFound);
        }

        return currentUser.IsGuest
            ? await HandleGuestScanAsync(table, passphrase)
            : await HandleStaffScanAsync(table);
    }

    public async Task<ServiceResult> ChangeStatusAsync(string token, TableStatus newStatus)
    {
        // Should it also use GetValidTableBySalt
        var table = await repository.GetByKeyAsync(t => t.QrSalt == token);
        if (table is null)
        {
            logger.LogWarning("ChangeStatus failed: Table {Token} not found", token);
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        return currentUser.IsGuest
            ? await HandleGuestStatusChangeAsync(table, newStatus, currentUser.GetRawToken()!) // not table access token, user access token for session...
            : await HandleStaffStatusChangeAsync(table, newStatus);
    }

    private async Task<ServiceResult<TableScanDto>> HandleGuestScanAsync(Tables table, string? passphrase)
    {
        if (table.IsDisabled)
        {
            logger.LogWarning("QR scan blocked for disabled Table {TableId}", table.Id);

            await notificationService.AddNotificationAsync(table,
                NotificationFactory.ForTableStatus(table, $"Staff attention needed at Disabled table {table.Label}.", NotificationType.StaffNeeded));

            return ServiceResult<TableScanDto>.Fail("QR for this table is currently unavailable. Waiter is coming.", ErrorType.Unauthorized);
        }

        // 1. Resume if guest already has a valid session on THIS table
        var token = currentUser.GetRawToken();
        if (!string.IsNullOrWhiteSpace(token) && await tableSessionService.HasActiveSessionAsync(table.Id, token))
        {
            logger.LogInformation("Guest resumed session {Session} on Table {TableId}", token, table.Id);
            var dto = mapper.Map<TableScanDto>(table);
            dto.GuestToken = token;
            dto.IsSessionEstablished = true;
            return ServiceResult<TableScanDto>.Ok(dto);
        }

        // 2. Prevent starting a session if guest is active elsewhere
        if (!string.IsNullOrWhiteSpace(token))
        {
            var activeSession = await tableSessionService.GetConflictingSessionAsync(token, table.Id);

            if (activeSession != null)
            {
                logger.LogWarning("Guest tried to join Table {CurrentTableId} while already active on Table {OtherTableId}.", table.Id, activeSession.TableId);
                return ServiceResult<TableScanDto>.Fail("You already have an active session on another table. Mark it as complete before next attempt.", ErrorType.Conflict);
            }
        }

        // 3. First-time scan
        if (table.Status == TableStatus.empty)
            return await StartFirstSession(table);

        // 4. Join existing session
        return await TryJoinExistingSession(table, passphrase);
    }

    //TODO: add check for unpaid receipts before emptying table.
    private async Task<ServiceResult> HandleGuestStatusChangeAsync(Tables table, TableStatus newStatus, string token)
    {
        var accessToken = currentUser.GetRawToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogWarning("Guest attempted to change table {Id} without token", token);
            return ServiceResult.Fail("Missing authentication token.", ErrorType.Unauthorized);
        }

        if (!await tableSessionService.HasActiveSessionAsync(table.Id, token))
        {
            logger.LogWarning("Invalid session for guest trying to change status on Table {Id}", table.Id);
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
            return ServiceResult.Ok();
        }

        await ApplyEmptyStatusAsync(table); //TODO: look into applying orders as complete
        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> HandleStaffStatusChangeAsync(Tables table, TableStatus newStatus)
    {
        var user = await currentUser.GetCurrentUserAsync();
        if (!await IsSameBusinessAsync(table.PlaceId))
        {
            logger.LogWarning("Unauthorized staff (User {UserId}) tried to change status of Table {Id}", user!.Id, table.Id);
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);
        }

        if (newStatus == TableStatus.empty)
        {
            await ApplyEmptyStatusAsync(table);
        }

        logger.LogInformation("User {UserId} changed Table {Id} status to {NewStatus}", user!.Id, table.Id, newStatus);
        table.Status = newStatus;
        await repository.UpdateAsync(table);
        return ServiceResult.Ok();
    }

    private async Task<ServiceResult<TableScanDto>> HandleStaffScanAsync(Tables table)
    {
        table.Status = TableStatus.occupied;
        await repository.UpdateAsync(table);
        logger.LogInformation("Table {Id} marked as occupied by staff.", table.Id);
        return ServiceResult<TableScanDto>.Ok(mapper.Map<TableScanDto>(table));
    }

    private async Task<bool> IsSameBusinessAsync(int placeId) //TODO: use validation service
    {
        var user = await currentUser.GetCurrentUserAsync();
        return placeId == user!.PlaceId;
    }

    private static string GeneratePassphrase(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }

    private async Task<ServiceResult<TableScanDto>> StartFirstSession(Tables table)
    {
        table.Status = TableStatus.occupied; //TODO: background job that empties tables with no active group sessions? Or wrap into transaction?
        await repository.UpdateAsync(table);

        var passphrase = GeneratePassphrase(); // 6-char alphanum
        var token = await guestSessionService.CreateSessionAsync(table.Id, passphrase);

        var dto = mapper.Map<TableScanDto>(table);
        dto.GuestToken = token;
        dto.IsSessionEstablished = true;

        logger.LogInformation("Table {TableId} is now occupied. First session started with passphrase {Passphrase}", table.Id, passphrase);

        await notificationService.AddNotificationAsync(table,
            NotificationFactory.ForTableStatus(table, $"New guest at table {table.Label}.", NotificationType.GuestJoinedTable));

        return ServiceResult<TableScanDto>.Ok(dto);
    }

    private async Task<ServiceResult<TableScanDto>> TryJoinExistingSession(Tables table, string? submittedPassphrase)
    {
        if (string.IsNullOrWhiteSpace(submittedPassphrase))
        {
            var dto = mapper.Map<TableScanDto>(table);
            dto.Message = "This table is currently occupied. Enter the passphrase to join.";
            dto.IsSessionEstablished = false;
            return ServiceResult<TableScanDto>.Ok(dto); //Token should be null / empty here i think.
        }
           
        try
        {
            var token = await guestSessionService.CreateSessionAsync(table.Id, submittedPassphrase);
            var dto = mapper.Map<TableScanDto>(table);
            dto.GuestToken = token;
            dto.IsSessionEstablished = true;
            logger.LogInformation("New guest joined table {TableId} using passphrase {Passphrase}", table.Id, submittedPassphrase);

            await notificationService.AddNotificationAsync(table,
                NotificationFactory.ForTableStatus(table, $"New guest at table {table.Label}.", NotificationType.GuestJoinedTable));

            return ServiceResult<TableScanDto>.Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Join failed: incorrect passphrase for table {TableId}, exception: {Ex}.", table.Id, ex);
            return ServiceResult<TableScanDto>.Fail("Incorrect passphrase. Please ask someone at the table.", ErrorType.Unauthorized);
        }
    }

    private async Task ApplyEmptyStatusAsync(Tables table)
    {
        await guestSessionService.EndGroupSessionAsync(table.Id);
        await orderRepository.SetTableOrdersAsClosedAsync(table.Id);

        table.Status = TableStatus.empty;
        await repository.UpdateAsync(table);

        await notificationService.AddNotificationAsync(table,
            NotificationFactory.ForTableStatus(table, $"Guests have left table {table.Label}.", NotificationType.GuestLeftTable));
        await notificationService.ClearNotificationsAsync(table.Id);
        logger.LogInformation("Table {TableId} set to empty and all sessions/orders cleared.", table.Id);
    }
}
