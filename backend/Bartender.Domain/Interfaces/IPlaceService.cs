using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.Interfaces;

public interface IPlaceService
{
    Task<PlaceWithMenuDto> GetByIdAsync(int id, bool includeNavigations = true);
    Task<List<PlaceDto>> GetAllAsync();
    Task AddAsync(InsertPlaceDto dto);
    Task UpdateAsync(int id, UpdatePlaceDto dto);
    Task DeleteAsync(int id);
    Task NotifyStaffAsync(string salt);
}
