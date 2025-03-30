using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class TableService(
    IRepository<Tables> repository,
    ILogger<TableService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
) : ITableService
{
    public async Task<ServiceResult<List<TableDto>>> GetAllAsync()
    {
        var user = await currentUser.GetCurrentUserAsync();
        var tables = await repository.GetAllAsync();

        var filtered = tables
            .Where(t => t.PlaceId == user.PlaceId)
            .Select(t => mapper.Map<TableDto>(t))
            .ToList();

        return ServiceResult<List<TableDto>>.Ok(filtered);
    }

    public async Task<ServiceResult<TableDto>> GetByIdAsync(int id)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult<TableDto>.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult<TableDto>.Fail("Unauthorized", ErrorType.Unauthorized);

        return ServiceResult<TableDto>.Ok(mapper.Map<TableDto>(table));
    }

    public async Task<ServiceResult<TableDto>> GetBySaltAsync(string salt)
    {
        var table = await repository.GetByKeyAsync(t => t.QrSalt == salt && !t.IsDisabled);

        if (table is null)
        { 
            logger.LogWarning("Detected attempt to access table via old token or currently unavailable table via token: {Token}", salt);
            return ServiceResult<TableDto>.Fail("Invalid or expired QR code", ErrorType.NotFound);
        }

        return ServiceResult<TableDto>.Ok(mapper.Map<TableDto>(table));
    }

    public async Task<ServiceResult> AddAsync(UpsertTableDto dto)
    {
        //TODO: label should be unique per place.
        var user = await currentUser.GetCurrentUserAsync();
        if (await repository.ExistsAsync(t =>
            t.PlaceId == user.PlaceId &&
            t.Label.Equals(dto.Label, StringComparison.CurrentCultureIgnoreCase)))
        {
            return ServiceResult.Fail("This table label is already in use for your place.", ErrorType.Conflict);
        }

        var entity = mapper.Map<Tables>(dto);

        entity.PlaceId = user.PlaceId;
        entity.Status = TableStatus.empty;
        entity.QrSalt = Guid.NewGuid().ToString("N");

        await repository.AddAsync(entity);
        logger.LogInformation("New table added by user {UserId}. Currently active token: {Token}", user.Id, entity.QrSalt);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertTableDto dto)
    {
        //TODO: label can't get updated.
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);

        table.Label = dto.Label;
        table.Seats = dto.Seats;

        await repository.UpdateAsync(table);
        logger.LogInformation("Table {Id} updated by user {UserId}", id, currentUser.UserId);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);

        await repository.DeleteAsync(table);
        logger.LogInformation("Table {Id} deleted", id);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangeStatusAsync(int id, TableStatus newStatus)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);

        table.Status = newStatus;
        await repository.UpdateAsync(table);

        logger.LogInformation("Table {Id} state changed to {Status}", id, newStatus);
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

    public async Task<ServiceResult> EnableAsync(int id)
    {
        return await ChangeDisabledAsync(id, false);
    }

    public async Task<ServiceResult> DisableAsync(int id)
    {
        return await ChangeDisabledAsync(id, true);
    }

    private async Task<ServiceResult> ChangeDisabledAsync(int id, bool flag)
    {
        var table = await repository.GetByIdAsync(id);
        if (table is null)
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(table.PlaceId))
            return ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized);

        table.IsDisabled = flag;
        await repository.UpdateAsync(table);

        logger.LogInformation("Table {Id} set to disabled = {Flag}", id, flag);
        return ServiceResult.Ok();
    }

    private async Task<bool> IsSameBusinessAsync(int placeId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return placeId == user.PlaceId;
    }
}
