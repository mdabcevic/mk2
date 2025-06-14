﻿using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.DTO.Table;
using Bartender.Data;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;

namespace Bartender.Domain.Services.Data;

public class TableInteractionService(
    IRepository<Table> repository,
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
    public async Task<TableScanDto> GetBySaltAsync(string salt, string? passphrase = null)
    {
        var table = await repository.GetByKeyAsync(t => t.QrSalt == salt) ?? throw new NotFoundException("Invalid QR code")
                .WithLogMessage($"lookup failed: Stale or invalid Token was used: {salt}");

        return currentUser.IsGuest
            ? await HandleGuestScanAsync(table, passphrase)
            : await HandleStaffScanAsync(table);
    }

    public async Task ChangeStatusAsync(string token, TableStatus newStatus)
    {
        // Should it also use GetValidTableBySalt
        var table = await repository.GetByKeyAsync(t => t.QrSalt == token) ?? throw new TableNotFoundException(token);

        if (currentUser.IsGuest)
            await HandleGuestStatusChangeAsync(table, newStatus, currentUser.GetRawToken()!);  // not table access token, user access token for session...
        
        else
            await HandleStaffStatusChangeAsync(table, newStatus);
    }

    private async Task<TableScanDto> HandleGuestScanAsync(Table table, string? passphrase)
    {
        if (table.IsDisabled)
        {
            logger.LogWarning("QR scan blocked for disabled Table {TableId}", table.Id);

            await notificationService.AddNotificationAsync(table,
                NotificationFactory.ForTableStatus(table, $"Staff attention needed at Disabled table {table.Label}.", NotificationType.StaffNeeded));

            throw new UnauthorizedAccessException("QR for this table is currently unavailable. Waiter is coming.");
        }

        // 1. Resume if guest already has a valid session on THIS table
        var token = currentUser.GetRawToken();
        if (!string.IsNullOrWhiteSpace(token) && await tableSessionService.HasActiveSessionAsync(table.Id, token))
        {
            logger.LogInformation("Guest resumed session {Session} on Table {TableId}", token, table.Id);
            var dto = mapper.Map<TableScanDto>(table);
            dto.GuestToken = token;
            dto.IsSessionEstablished = true;
            return dto;
        }

        // 2. Prevent starting a session if guest is active elsewhere
        if (!string.IsNullOrWhiteSpace(token))
        {
            var activeSession = await tableSessionService.GetConflictingSessionAsync(token, table.Id);

            if (activeSession != null)
                throw new ConflictException("You already have an active session on another table. Mark it as complete before next attempt.")
                    .WithLogMessage($"Guest tried to join Table {table.Id} while already active on Table {activeSession.TableId}.");
        }

        // 3. First-time scan
        if (table.Status == TableStatus.empty)
            return await StartFirstSession(table);

        // 4. Join existing session
        return await TryJoinExistingSession(table, passphrase);
    }

    //TODO: add check for unpaid receipts before emptying table.
    private async Task HandleGuestStatusChangeAsync(Table table, TableStatus newStatus, string token)
    {
        var accessToken = currentUser.GetRawToken();
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new AuthorizationException("Missing authentication token.")
                .WithLogMessage($"Guest attempted to change table {token} without token");

        if (!await tableSessionService.HasActiveSessionAsync(table.Id, token))
            throw new AuthorizationException("Unauthorized or expired session.")
                .WithLogMessage($"Invalid session for guest trying to change status on Table {table.Id}");

        if (newStatus != TableStatus.empty)
            throw new AuthorizationException("Guests can only free tables.")
                .WithLogMessage($"Guest tried to set status to {newStatus} on Table {table.Id} — only 'empty' is allowed");

        if (table.Status == TableStatus.empty)
        {
            logger.LogInformation("Guest attempted to free an already empty Table {Id}", table.Id);
            return;
        }

        await ApplyEmptyStatusAsync(table);
    }

    private async Task HandleStaffStatusChangeAsync(Table table, TableStatus newStatus)
    {
        var user = await currentUser.GetCurrentUserAsync();
        if (!await IsSameBusinessAsync(table.PlaceId))
            throw new UnauthorizedBusinessAccessException()
                .WithLogMessage($"Unauthorized staff (User {user!.Id}) tried to change status of Table {table.Id}");

        if (newStatus == TableStatus.empty)
            await ApplyEmptyStatusAsync(table);

        logger.LogInformation("User {UserId} changed Table {Id} status to {NewStatus}", user!.Id, table.Id, newStatus);
        table.Status = newStatus;
        await repository.UpdateAsync(table);
    }

    private async Task<TableScanDto> HandleStaffScanAsync(Table table)
    {
        table.Status = TableStatus.occupied;
        await repository.UpdateAsync(table);
        logger.LogInformation("Table {Id} marked as occupied by staff.", table.Id);
        return mapper.Map<TableScanDto>(table);
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

    private async Task<TableScanDto> StartFirstSession(Table table)
    {
        table.Status = TableStatus.occupied;
        await repository.UpdateAsync(table);

        var passphrase = GeneratePassphrase(); // 6-char alphanum
        var token = await guestSessionService.CreateSessionAsync(table.Id, passphrase);

        var dto = mapper.Map<TableScanDto>(table);
        dto.GuestToken = token;
        dto.IsSessionEstablished = true;

        logger.LogInformation("Table {TableId} is now occupied. First session started with passphrase {Passphrase}", table.Id, passphrase);

        await notificationService.AddNotificationAsync(table,
            NotificationFactory.ForTableStatus(table, $"New guest at table {table.Label}.", NotificationType.GuestJoinedTable));

        return dto;
    }

    private async Task<TableScanDto> TryJoinExistingSession(Table table, string? submittedPassphrase)
    {
        if (string.IsNullOrWhiteSpace(submittedPassphrase))
        {
            var dto = mapper.Map<TableScanDto>(table);
            dto.Message = "This table is currently occupied. Enter the passphrase to join.";
            dto.IsSessionEstablished = false;
            return dto;
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

            return dto;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Join failed: incorrect passphrase for table {TableId}.", table.Id);
            throw new UnauthorizedAccessException("Incorrect passphrase. Please ask someone at the table.");
        }
    }

    private async Task ApplyEmptyStatusAsync(Table table)
    {
        //TODO: notifications should come after these actions, but then lack permissions because session was terminated.
        await notificationService.AddNotificationAsync(table,
            NotificationFactory.ForTableStatus(table, $"Guests have left table {table.Label}.", NotificationType.GuestLeftTable));
        await notificationService.ClearNotificationsAsync(table.Id);

        await guestSessionService.EndGroupSessionAsync(table.Id);
        await orderRepository.SetTableOrdersAsClosedAsync(table.Id);

        table.Status = TableStatus.empty;
        await repository.UpdateAsync(table);

        logger.LogInformation("Table {TableId} set to empty and all sessions/orders cleared.", table.Id);
    }
}
