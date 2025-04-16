using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class TableManagementService(
    ITableRepository repository,
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
        var tables = await repository.GetAllByPlaceAsync(user!.PlaceId);
        var result = mapper.Map<List<TableDto>>(tables);

        return ServiceResult<List<TableDto>>.Ok(result);
    }

    public async Task<ServiceResult<List<BaseTableDto>>> GetByPlaceId(int placeId)
    {
        var tables = await repository.GetActiveByPlaceAsync(placeId);
        var result = mapper.Map<List<BaseTableDto>>(tables);
        return ServiceResult<List<BaseTableDto>>.Ok(result); //TODO: redact properties for employees (salt, disabled flag...)
    }

    public async Task<ServiceResult<TableDto>> GetByLabelAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label);

        if (table is null)
        {
            logger.LogWarning("Table with label '{Label}' not found for Place {PlaceId}", label, user!.PlaceId);
            return ServiceResult<TableDto>.Fail("Table not found", ErrorType.NotFound);
        }
        return ServiceResult<TableDto>.Ok(mapper.Map<TableDto>(table));
    }

    public async Task<ServiceResult> BulkUpsertAsync(List<UpsertTableDto> dtoList)
    {
        var duplicatesInInput = dtoList
            .GroupBy(dto => dto.Label, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatesInInput.Count != 0)
        {
            logger.LogWarning("Duplicate labels in bulk upsert input: {Labels}", string.Join(", ", duplicatesInInput));
            return ServiceResult.Fail("Duplicate labels found in input: " + string.Join(", ", duplicatesInInput), ErrorType.Conflict);
        }

        var user = await currentUser.GetCurrentUserAsync();
        var existing = await repository.GetByPlaceAsLabelDictionaryAsync(user!.PlaceId);

        var toInsert = new List<Tables>();
        var toUpdate = new List<Tables>();
        foreach (var dto in dtoList)
        {
            if (existing.TryGetValue(dto.Label, out var existingTable))
            {
                mapper.Map(dto, existingTable);
                toUpdate.Add(existingTable);
                logger.LogInformation("Table '{Label}' updated by User {UserId}", existingTable.Label, user!.Id);
            }
            else
            {
                var newTable = mapper.Map<Tables>(dto);
                newTable.PlaceId = user!.PlaceId;
                toInsert.Add(newTable);
                logger.LogInformation("New table added by user {UserId}. Currently active token: {Token}", user.Id, newTable.QrSalt);
            }
        }

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
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label);

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
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label);

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
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label);

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
