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
    public interface IMenuItemsService
    {
        Task<IEnumerable<MenuItemsBaseDTO>> GetByPlaceIdAsync(int id);
        Task<MenuItemsDTO?> GetByIdAsync(int placeId, int productId);
        Task<IEnumerable<GroupedMenusDTO>> GetAllAsync();
        Task<IEnumerable<MenuItemsBaseDTO>> GetFilteredAsync(int placeId, string searchProduct);
        Task AddAsync(UpsertMenuItemDTO menuItem);
        Task<IEnumerable<FailedMenuItemsDTO>> AddMultipleAsync(IEnumerable<UpsertMenuItemDTO> menuItems);
        Task UpdateAsync(UpsertMenuItemDTO menuItem);
        Task DeleteAsync(int placeId, int productId);
    }
}
