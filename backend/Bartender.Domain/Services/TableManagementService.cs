using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class TableManagementService(
    IRepository<Tables> repository,
    ILogger<TableInteractionService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
    ) : ITableManagementService
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

    public async Task<ServiceResult<List<TableDto>>> GetByPlaceId(int placeId)
    {
        var tables = await repository.Query()
            .Where(t => t.PlaceId == placeId && !t.IsDisabled)
            .Select(t => mapper.Map<TableDto>(t))
            .ToListAsync();

        //var result = tables.Select(t => mapper.Map<TableDto>(t)).ToList();
        return ServiceResult<List<TableDto>>.Ok(tables);
    }

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

    public async Task<ServiceResult> AddAsync(UpsertTableDto dto)
    {
        var user = await currentUser.GetCurrentUserAsync();
        if (await repository.ExistsAsync(t =>
            t.PlaceId == user!.PlaceId &&
            t.Label.Equals(dto.Label, StringComparison.CurrentCultureIgnoreCase)))
        {
            logger.LogWarning("Failed to add table: Label '{Label}' already exists for Place {PlaceId} by User {UserId}",
            dto.Label, user!.PlaceId, user.Id);
            return ServiceResult.Fail("Cannot create table with that label - it already exists.", ErrorType.Conflict);
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

    public async Task<ServiceResult> BulkUpsertAsync(List<UpsertTableDto> dtoList)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var existing = await repository.Query()
            .Where(t => t.PlaceId == user!.PlaceId)
            .ToDictionaryAsync(t => t.Label.ToLower());

        var upserts = new List<Tables>();
        //TODO: filter for duplicates in DTO list?
        foreach (var dto in dtoList)
        {
            var key = dto.Label.ToLower();
            if (existing.TryGetValue(key, out var existingTable))
            {
                mapper.Map(dto, existingTable);
                upserts.Add(existingTable);
                logger.LogInformation("Table '{Label}' updated by User {UserId}", existingTable.Label, user!.Id);
            }
            else
            {
                var newTable = mapper.Map<Tables>(dto);
                newTable.Id = 0; // <-- ensure this
                newTable.PlaceId = user!.PlaceId;
                newTable.Status = TableStatus.empty;
                newTable.QrSalt = Guid.NewGuid().ToString("N");
                upserts.Add(newTable);
                logger.LogInformation("New table added by user {UserId}. Currently active token: {Token}", user.Id, newTable.QrSalt);
            }
        }
        var toUpdate = upserts.Where(t => t.Id > 0).ToList();
        var toInsert = upserts.Where(t => t.Id == 0).ToList();

        if (toUpdate.Count != 0)
            await repository.UpdateRangeAsync(toUpdate);

        if (toInsert.Count != 0)
            await repository.AddMultipleAsync(toInsert);
        logger.LogInformation("Bulk updated {Count} tables for place {PlaceId}", toUpdate.Count, user!.PlaceId);
        logger.LogInformation("Bulk inserted {Count} tables for place {PlaceId}", toInsert.Count, user!.PlaceId);
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
        logger.LogInformation("Table '{Label}' disabled state set to {Flag} by Staff {UserId}", label, flag, user!.Id);
        return ServiceResult.Ok();
    }
}
