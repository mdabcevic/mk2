using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services.Data;

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
    public async Task<List<TableDto>> GetAllAsync()
    {
        var user = await currentUser.GetCurrentUserAsync();
        var tables = await repository.GetAllByPlaceAsync(user!.PlaceId);
        var result = mapper.Map<List<TableDto>>(tables);

        return result;
    }

    public async Task<List<BaseTableDto>> GetByPlaceId(int placeId)
    {
        var tables = await repository.GetActiveByPlaceAsync(placeId);
        var result = mapper.Map<List<BaseTableDto>>(tables);
        return result;
    }

    public async Task<TableDto> GetByLabelAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label) ?? throw new TableNotFoundException(label: label);
        return mapper.Map<TableDto>(table);
    }

    public async Task BulkUpsertAsync(List<UpsertTableDto> dtoList)
    {
        var duplicatesInInput = dtoList
            .GroupBy(dto => dto.Label, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatesInInput.Count != 0)
            throw new ConflictException("Duplicate labels found in input: " + string.Join(", ", duplicatesInInput));

        var user = await currentUser.GetCurrentUserAsync();
        var existing = await repository.GetByPlaceAsLabelDictionaryAsync(user!.PlaceId);

        var toInsert = new List<Table>();
        var toUpdate = new List<Table>();
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
                var newTable = mapper.Map<Table>(dto);
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
        return;
    }

    public async Task DeleteAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label) ?? throw new TableNotFoundException(label: label);

        await repository.DeleteAsync(table);
        logger.LogInformation("Table '{Label}' deleted by User {UserId}", label, user!.Id);
        return;
    }

    public async Task<string> RegenerateSaltAsync(string label)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label) ?? throw new TableNotFoundException(label: label)
                .WithLogMessage($"Resalt failed: Table '{label}' not found for Place {user!.PlaceId}");

        table.QrSalt = Guid.NewGuid().ToString("N");
        await repository.UpdateAsync(table);
        logger.LogInformation("Salt rotated for Table '{Label}' by User {UserId}", label, user!.Id);
        return table.QrSalt;
    }

    public async Task SwitchDisabledAsync(string label, bool flag)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var table = await repository.GetByPlaceLabelAsync(user!.PlaceId, label) ?? throw new TableNotFoundException(label: label)
                .WithLogMessage($"Disable toggle failed: Table '{label}' not found for Place {user!.PlaceId}");

        table.IsDisabled = flag;
        await repository.UpdateAsync(table);
        logger.LogInformation("Table '{Label}' disabled state set to {Flag} by Staff {UserId}", label, flag, user!.Id);
        return;
    }
}
