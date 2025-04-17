using AutoMapper;
using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class PlacesService(
    IRepository<Places> repository,
    ITableRepository tableRepository,
    ILogger<PlacesService> logger,
    ICurrentUserContext currentUser,
    INotificationService notificationService,
    IMapper mapper
    )
    : IPlacesService
{
    public async Task<ServiceResult> AddAsync(InsertPlaceDto dto)
    {
        if (!await IsSameBusinessAsync(dto.BusinessId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var entity = mapper.Map<Places>(dto);
        await repository.AddAsync(entity);
        logger.LogInformation("Place created: {Address}, BusinessId: {BusinessId}", dto.Address, dto.BusinessId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var place = await repository.GetByIdAsync(id);
        if (place == null)
            return ServiceResult.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(place.BusinessId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        await repository.DeleteAsync(place);
        logger.LogInformation("Place deleted with ID: {PlaceId}", id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<PlaceDto>>> GetAllAsync()
    {
        var placesWithMenus = repository.QueryIncluding(
            p => p.Business,
            p => p.City
        );

        var list = await placesWithMenus
            .Select(p => mapper.Map<PlaceDto>(p))
            .ToListAsync();
        return ServiceResult<List<PlaceDto>>.Ok(list);
    }

    public async Task<ServiceResult<PlaceWithMenuDto>> GetByIdAsync(int id, bool includeNavigations = true)
    {
        var place = await repository.Query()
        .Include(p => p.Business)
        .Include(p => p.City)
        .Include(p => p.MenuItems)!
            .ThenInclude(mi => mi.Product)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (place == null)
            return ServiceResult<PlaceWithMenuDto>.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        var dto = mapper.Map<PlaceWithMenuDto>(place);
        return ServiceResult<PlaceWithMenuDto>.Ok(dto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdatePlaceDto dto)
    {
        var place = await repository.GetByIdAsync(id);
        if (place == null)
            return ServiceResult.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(place.BusinessId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        mapper.Map(dto, place);
        await repository.UpdateAsync(place);
        logger.LogInformation("Place updated with ID: {PlaceId}", place.Id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> NotifyStaffAsync(string salt)
    {
        var table = await tableRepository.GetBySaltAsync(salt);

        if (table is null)
        {
            logger.LogWarning("NotifyStaff failed: Table does not exist.");
            return ServiceResult.Fail("Table not found", ErrorType.NotFound);
        }

        await notificationService.AddNotificationAsync(table.Id, 
            NotificationFactory.ForTableStatus(table, $"Waiter requested at table {table.Label}.", NotificationType.StaffNeeded));

        logger.LogInformation("Staff notified for table {Label}", table.Label);
        return ServiceResult.Ok();
    }

    private async Task<bool> IsSameBusinessAsync(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return targetPlaceId == user.PlaceId;
    }
}
