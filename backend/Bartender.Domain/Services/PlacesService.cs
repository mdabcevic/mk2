using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class PlacesService(
    IRepository<Places> repository,
    ILogger<StaffService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
    )
    : IPlacesService
{
    public async Task<ServiceResult> AddAsync(UpsertPlaceDto dto)
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
        var placesWithMenus = repository.Query() //needed because of nested product include!
            .Include(p => p.Business)
            .Include(p => p.City)
            .Include(p => p.MenuItems)!
        .ThenInclude(mi => mi.Product);

        var list = await placesWithMenus
            .Select(p => mapper.Map<PlaceDto>(p))
            .ToListAsync();
        return ServiceResult<List<PlaceDto>>.Ok(list);
    }

    public async Task<ServiceResult<PlaceDto>> GetByIdAsync(int id, bool includeNavigations = false)
    {
        var place = await repository.GetByIdAsync(id, includeNavigations);
        if (place == null)
            return ServiceResult<PlaceDto>.Fail($"Place with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(place.BusinessId))
            return ServiceResult<PlaceDto>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var dto = mapper.Map<PlaceDto>(place);
        return ServiceResult<PlaceDto>.Ok(dto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertPlaceDto dto)
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

    private async Task<bool> IsSameBusinessAsync(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return targetPlaceId == user.PlaceId;
    }
}
