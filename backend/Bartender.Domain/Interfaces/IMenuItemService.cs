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
        Task<ServiceResult<IEnumerable<MenuItemBaseDTO>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false);
        Task<ServiceResult<IEnumerable<GroupedCategoryMenu>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false);
        Task<ServiceResult<MenuItemDTO?>> GetByIdAsync(int placeId, int productId);
        Task<ServiceResult<IEnumerable<GroupedPlaceMenuDTO>>> GetAllAsync();
        Task<ServiceResult<IEnumerable<MenuItemBaseDTO>>> GetFilteredAsync(int placeId, string searchProduct);
        Task<ServiceResult> AddAsync(UpsertMenuItemDTO menuItem);
        Task<ServiceResult<IEnumerable<FailedMenuItemDTO>>> AddMultipleAsync(IEnumerable<UpsertMenuItemDTO> menuItems);
        Task<ServiceResult> UpdateAsync(UpsertMenuItemDTO menuItem);
        Task<ServiceResult> DeleteAsync(int placeId, int productId);
    }
}
