using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IPlacesService
{
    Task<ServiceResult<PlaceDto>> GetByIdAsync(int id, bool includeNavigations = false);
    Task<ServiceResult<List<PlaceDto>>> GetAllAsync();
    Task<ServiceResult> AddAsync(UpsertPlaceDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpsertPlaceDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
