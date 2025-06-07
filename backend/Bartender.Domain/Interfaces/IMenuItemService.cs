using Bartender.Domain.DTO.MenuItem;

namespace Bartender.Domain.Interfaces
{
    public interface IMenuItemService
    {
        Task<List<MenuItemBaseDto>> GetByPlaceIdAsync(int id, bool onlyAvailable = false);
        Task<List<GroupedCategoryMenuDto>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false);
        Task<MenuItemDto?> GetByIdAsync(int placeId, int productId);
        Task<List<GroupedPlaceMenuDto>> GetAllAsync();
        Task<List<MenuItemBaseDto>> GetFilteredAsync(int placeId, string searchProduct);
        Task AddAsync(UpsertMenuItemDto menuItem);
        Task<List<FailedMenuItemDto>> AddMultipleAsync(List<UpsertMenuItemDto> menuItems);
        Task<List<FailedMenuItemDto>> CopyMenuAsync(int fromPlaceId, int toPlaceId);
        Task UpdateAsync(UpsertMenuItemDto menuItem);
        Task UpdateItemAvailabilityAsync(int placeId, int productId, bool isAvailable);
        Task DeleteAsync(int placeId, int productId);
    }
}
