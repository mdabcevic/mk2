using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.Interfaces
{
    public interface IMenuItemService
    {
        Task<ServiceResult<IEnumerable<MenuItemBaseDto>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false);
        Task<ServiceResult<IEnumerable<GroupedCategoryMenuDto>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false);
        Task<ServiceResult<MenuItemDto?>> GetByIdAsync(int placeId, int productId);
        Task<ServiceResult<IEnumerable<GroupedPlaceMenuDto>>> GetAllAsync();
        Task<ServiceResult<IEnumerable<MenuItemBaseDto>>> GetFilteredAsync(int placeId, string searchProduct);
        Task<ServiceResult> AddAsync(UpsertMenuItemDto menuItem);
        Task<ServiceResult<IEnumerable<FailedMenuItemDto>>> AddMultipleAsync(IEnumerable<UpsertMenuItemDto> menuItems);
        Task<ServiceResult<IEnumerable<FailedMenuItemDto>>> CopyMenuAsync(int fromPlaceId, int toPlaceId);
        Task<ServiceResult> UpdateAsync(UpsertMenuItemDto menuItem);
        Task<ServiceResult> UpdateItemAvailabilityAsync(int placeId, int productId, bool isAvailable);
        Task<ServiceResult> DeleteAsync(int placeId, int productId);
    }
}
