using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Business;
using Bartender.Domain.Interfaces;
using Bartender.Domain.utility.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.Services.Data;

public class BusinessService(
    IRepository<Business> repository,
    ILogger<BusinessService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
) : IBusinessService
{
    public async Task<List<BusinessDto>> GetAllAsync()
    {
        var businesses = await repository.QueryIncluding(b => b.Places).ToListAsync();
        var dtos = mapper.Map<List<BusinessDto>>(businesses);
        return dtos;
    }

    public async Task<BusinessDto> GetByIdAsync(int id)
    {
        if (!await IsAccessAllowedAsync(id))
        {
            throw new UnauthorizedBusinessAccessException();
        }

        var business = await repository.GetByIdAsync(id, true);

        if (business is null) { 
            throw new BusinessNotFoundException(id);
        }

        var dto = mapper.Map<BusinessDto>(business);
        logger.LogInformation("Successfully retrieved business with {Id}", id);
        return dto;
    }

    public async Task AddAsync(UpsertBusinessDto dto)
    {
        if (dto.OIB.Length != 11) //TODO: include actual OIB validation
            throw new AppValidationException("OIB must be 11 characters");

        var entity = mapper.Map<Business>(dto);
        await repository.AddAsync(entity);
        logger.LogInformation("Business created: {Name}, OIB: {OIB}", dto.Name, dto.OIB);
    }

    public async Task UpdateAsync(int id, UpsertBusinessDto dto)
    {
        var business = await repository.GetByIdAsync(id);
        if (business is null)
            throw new BusinessNotFoundException(id);

        mapper.Map(dto, business);
        await repository.UpdateAsync(business);
        logger.LogInformation("Business updated with ID: {BusinessId}", id);
    }

    public async Task UpdateSubscriptionAsync(SubscriptionTier tier)
    {
        var user = await currentUser.GetCurrentUserAsync();
        if (user?.Place == null)
        {
            throw new UserPlaceAssignmentException(user?.Id);
        }

        var business = await repository.GetByIdAsync(user.Place.BusinessId);
        if (business is null)
        {
            throw new BusinessNotFoundException(user.Place.BusinessId);
        }

        business.SubscriptionTier = tier;
        await repository.UpdateAsync(business);

        logger.LogInformation("Subscription updated for business ID: {BusinessId}", user.Place.BusinessId);
    }

    public async Task DeleteAsync(int id)
    {
        var business = await repository.GetByIdAsync(id);
        if (business is null)
            throw new BusinessNotFoundException(id);

        await repository.DeleteAsync(business);
        logger.LogInformation("Business deleted with ID: {BusinessId}", id);
    }

    private async Task<bool> IsAccessAllowedAsync(int targetBusinessId)
    {
        var user = await currentUser.GetCurrentUserAsync() ?? throw new ValidationException();
        if (user.Role == EmployeeRole.owner)
            return true;

        return user.Place?.BusinessId == targetBusinessId;
    }

}
