using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Services;

public class TableInteractionService(
    IRepository<Tables> repository,
    IGuestSessionService guestSessionService,
    ITableSessionService tableSessionService,
    IOrderRepository orderRepository,
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
    public async Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt)
    {
        var table = await repository.GetByKeyAsync(t => t.QrSalt == salt);
        if (table is null)
        {
            logger.LogWarning("QR lookup failed: Stale or invalid Token was used: {Token}", salt);
            return ServiceResult<TableScanDto>.Fail("Invalid QR code", ErrorType.NotFound);
        }

        return currentUser.IsGuest
            ? await HandleGuestScanAsync(table)
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
            ? await HandleGuestStatusChangeAsync(table, newStatus, token)
            : await HandleStaffStatusChangeAsync(table, newStatus);
    }

    private async Task<ServiceResult<TableScanDto>> HandleGuestScanAsync(Tables table)
    {
        if (table.IsDisabled)
        {
            logger.LogWarning("QR scan blocked for disabled Table {TableId}", table.Id);
            return ServiceResult<TableScanDto>.Fail("QR for this table is currently unavailable. Waiter is coming.", ErrorType.Unauthorized);
        }

        if (table.Status == TableStatus.occupied)
            return await HandleOccupiedOnGuestScan(table);

        return await BeginNewGuestSession(table);
    }

    private async Task<ServiceResult<TableScanDto>> HandleOccupiedOnGuestScan(Tables table)
    {
        var token = currentUser.GetRawToken();
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
        return await BeginNewGuestSession(table);
    }

    private async Task<ServiceResult<TableScanDto>> BeginNewGuestSession(Tables table)
    {
        table.Status = TableStatus.occupied;
        await repository.UpdateAsync(table);

        var passphrase = GeneratePassphrase();
        var newToken = await guestSessionService.CreateSessionAsync(table.Id, passphrase);

        var resultDto = mapper.Map<TableScanDto>(table);
        resultDto.GuestToken = newToken;
        resultDto.Passphrase = passphrase; // add this property to DTO

        return ServiceResult<TableScanDto>.Ok(resultDto);
    }

    private async Task<ServiceResult> HandleGuestStatusChangeAsync(Tables table, TableStatus newStatus, string token)
    {
        var accessToken = currentUser.GetRawToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogWarning("Guest attempted to change table {Id} without token", token);
            return ServiceResult.Fail("Missing authentication token.", ErrorType.Unauthorized);
        }

        var session = await guestSessionService.GetByTokenAsync(table.Id, accessToken);

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
            return ServiceResult.Ok();
        }

        logger.LogInformation("Guest freed Table {Id} via valid session", table.Id);
        await guestSessionService.DeleteSessionAsync(session.Id);
        logger.LogInformation("Session {Id} was deleted.", session.Id);

        table.Status = newStatus;
        await repository.UpdateAsync(table);

        // TODO: Re-evaluate this logic when implementing multi-guest table sharing
        await orderRepository.SetTableOrdersAsClosedAsync(table.Id);

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
            await orderRepository.SetTableOrdersAsClosedAsync(table.Id);
            logger.LogInformation("Orders set as closed for TableId: {TableId}", table.Id);
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

    private async Task<bool> IsSameBusinessAsync(int placeId) //TODO: business guard
    {
        var user = await currentUser.GetCurrentUserAsync();
        return placeId == user!.PlaceId;
    }

    private string GeneratePassphrase(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

}
