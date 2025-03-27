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
        Task<ServiceResult<IEnumerable<MenuItemsBaseDTO>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false);
        Task<ServiceResult<IEnumerable<GroupedCategoryMenu>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false);
        Task<ServiceResult<MenuItemsDTO?>> GetByIdAsync(int placeId, int productId);
        Task<ServiceResult<IEnumerable<GroupedPlaceMenuDTO>>> GetAllAsync();
        Task<ServiceResult<IEnumerable<MenuItemsBaseDTO>>> GetFilteredAsync(int placeId, string searchProduct);
        Task<ServiceResult> AddAsync(UpsertMenuItemDTO menuItem);
        Task<ServiceResult<IEnumerable<FailedMenuItemsDTO>>> AddMultipleAsync(IEnumerable<UpsertMenuItemDTO> menuItems);
        Task<ServiceResult> UpdateAsync(UpsertMenuItemDTO menuItem);
        Task<ServiceResult> DeleteAsync(int placeId, int productId);
    }
}
