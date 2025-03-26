using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
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
    public Task<ServiceResult> AddAsync(UpsertPlaceDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<PlaceDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<PlaceDto>> GetByIdAsync(int id, bool includeNavigations = false)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> UpdateAsync(int id, UpsertPlaceDto dto)
    {
        throw new NotImplementedException();
    }
}
