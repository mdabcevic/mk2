using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class BusinessService(
    IRepository<Businesses> repository,
    ILogger<BusinessService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
) : IBusinessService
{
    public async Task<ServiceResult<List<BusinessDto>>> GetAllAsync()
    {
        var businesses = await repository.QueryIncluding(b => b.Places).ToListAsync();
        var dtos = mapper.Map<List<BusinessDto>>(businesses);
        return ServiceResult<List<BusinessDto>>.Ok(dtos);
    }

    public async Task<ServiceResult<BusinessDto>> GetByIdAsync(int id)
    {
        if (!await IsSameBusinessAsync(id))
        {
            logger.LogWarning("Cross-entity request detected.");
            return ServiceResult<BusinessDto>.Fail($"Failure fetching business with requested id.", ErrorType.NotFound);
        }
        //var business = await repository.QueryIncluding(
        //b => b.Places
        //).FirstOrDefaultAsync(b => b.Id == id);
        var business = await repository.GetByIdAsync(id, true);

        if (business is null) { 
            logger.LogWarning("Business entity does not exist.");
            return ServiceResult<BusinessDto>.Fail($"Business with ID {id} not found.", ErrorType.NotFound);
        }

        var dto = mapper.Map<BusinessDto>(business);
        logger.LogInformation("Successfully retrieved business with {Id}", id);
        return ServiceResult<BusinessDto>.Ok(dto);
    }

    public async Task<ServiceResult> AddAsync(UpsertBusinessDto dto)
    {
        if (dto.OIB.Length != 11) //TODO: include actual OIB validation
            return ServiceResult.Fail("OIB must be 11 characters", ErrorType.Validation);

        var entity = mapper.Map<Businesses>(dto);
        await repository.AddAsync(entity);
        logger.LogInformation("Business created: {Name}, OIB: {OIB}", dto.Name, dto.OIB);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertBusinessDto dto)
    {
        var business = await repository.GetByIdAsync(id);
        if (business is null)
            return ServiceResult.Fail($"Business with ID {id} not found.", ErrorType.NotFound);

        mapper.Map(dto, business);
        await repository.UpdateAsync(business);
        logger.LogInformation("Business updated with ID: {BusinessId}", id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateSubscriptionAsync(SubscriptionTier tier)
    {
        var user = await currentUser.GetCurrentUserAsync();
        if (user.Place == null)
        {
            logger.LogError("User isn't assigned to any place.");
            return ServiceResult.Fail("Error fetching user's place.", ErrorType.Unknown);
        }

        var business = await repository.GetByIdAsync(user.Place.BusinessId);
        if (business is null)
        {
            logger.LogError("Business not found for current user.");
            return ServiceResult.Fail("Business not found.", ErrorType.NotFound);
        }

        business.SubscriptionTier = tier;
        await repository.UpdateAsync(business);

        logger.LogInformation("Subscription updated for business ID: {BusinessId}", user.Place.BusinessId);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var business = await repository.GetByIdAsync(id);
        if (business is null)
            return ServiceResult.Fail($"Business with ID {id} not found.", ErrorType.NotFound);

        await repository.DeleteAsync(business);
        logger.LogInformation("Business deleted with ID: {BusinessId}", id);
        return ServiceResult.Ok();
    }

    private async Task<bool> IsSameBusinessAsync(int targetBusinessId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return user.Place.BusinessId == targetBusinessId;
    }

}
