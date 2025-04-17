using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IPlacesService
{
    Task<ServiceResult<PlaceWithMenuDto>> GetByIdAsync(int id, bool includeNavigations = true);
    Task<ServiceResult<List<PlaceDto>>> GetAllAsync();
    Task<ServiceResult> AddAsync(InsertPlaceDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpdatePlaceDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> NotifyStaffAsync(string salt);
}
