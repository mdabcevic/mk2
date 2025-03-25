using AutoMapper;
using Bartender.Domain.Interfaces;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

using Bartender.Domain.Repositories;
using System.Globalization;
using AutoMapper.QueryableExtensions;

namespace Bartender.Domain.Services;
public class MenuItemsService(
    IRepository<MenuItems> repository, 
    IRepository<Places> placeRepository,
    IRepository<Products> productRepository,
    IMapper mapper) : IMenuItemsService
{
    public async Task<IEnumerable<MenuItemsBaseDTO?>> GetByPlaceIdAsync(int id)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == id);

        if (!existingPlace)
        {
            throw new NotFoundException($"Place with id {id} not found");
        }

        var menu = await repository.QueryIncluding(mi => mi.Product, mi => mi.Product.Category, mi => mi.Place.Business)
            /*.Include(mi => mi.Place)
                .ThenInclude(p => p.Business)
            .Include(mi => mi.Product)
                .ThenInclude(p => p.Category)*/
            .Where(mi => mi.PlaceId == id)
         .ToListAsync();

        if (!menu.Any())
        {
            throw new NotFoundException("This place does not have any products on the menu at the moment");
        }
        return mapper.Map<IEnumerable<MenuItemsBaseDTO>>(menu);
    }

    public async Task<MenuItemsDTO?> GetByIdAsync(int placeId, int productId)
    {
        var menuItem = await repository.GetByKeyAsync(
            mi => mi.PlaceId == placeId && mi.ProductId == productId,
            mi => mi.Product.Category, mi => mi.Place.Business);
        if (menuItem == null)
        {
            throw new NotFoundException($"MenutItem with place id {placeId} and product id {productId} not found");
        }
        return mapper.Map<MenuItemsDTO>(menuItem);
    }

    public async Task<IEnumerable<GroupedMenusDTO>> GetAllAsync()
    {
        var groupedMenus =  await placeRepository.Query()
        .Include(p => p.Business)
        .Include(p => p.MenuItems)
            .ThenInclude(mi => mi.Product)
                .ThenInclude(p => p.Category)
        .ToListAsync();

        return mapper.Map<IEnumerable<GroupedMenusDTO>>(groupedMenus);
    }

    public async Task AddAsync(UpsertMenuItemDTO menuItem)
    {
        await ValidateMenuItemAsync(menuItem);

        bool existingMenuItem = await repository.ExistsAsync(mi =>
            mi.ProductId == menuItem.ProductId &&
            mi.PlaceId == menuItem.PlaceId);

        if (existingMenuItem)
        {
            throw new DuplicateEntryException($"The menu item with product ID {menuItem.ProductId} already exists at the place with ID {menuItem.PlaceId}");
        }

        var newMenuItem = mapper.Map<MenuItems>(menuItem);
        await repository.AddAsync(newMenuItem);
    }

    public async Task<IEnumerable<FailedMenuItemsDTO>> AddMultipleAsync(IEnumerable<UpsertMenuItemDTO> menuItems)
    {

        var validMenuItems = new List<MenuItems>();
        var failedMenuItems = new List<FailedMenuItemsDTO>();

        foreach (var menuItem in menuItems)
        {
            try
            {
                await ValidateMenuItemAsync(menuItem);
                bool existingMenuItem = await repository.ExistsAsync(mi =>
                    mi.ProductId == menuItem.ProductId &&
                    mi.PlaceId == menuItem.PlaceId);

                if (existingMenuItem)
                {
                    throw new DuplicateEntryException($"The menu item with product ID {menuItem.ProductId} already exists at the place with ID {menuItem.PlaceId}");
                }

                validMenuItems.Add(mapper.Map<MenuItems>(menuItem));
            }
            catch (Exception ex) {
                failedMenuItems.Add(new FailedMenuItemsDTO
                {
                    MenuItem = menuItem,
                    ErrorMessage = ex.Message
                });
            }
        }

        if (validMenuItems.Any())
        {
            await repository.AddMultipleAsync(validMenuItems);
        }

        return failedMenuItems;
    }

    public async Task ValidateMenuItemAsync(UpsertMenuItemDTO menuItem)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == menuItem.PlaceId);
        if (!existingPlace) 
            throw new NotFoundException($"Place with id {menuItem.PlaceId} not found");
        

        bool existingProduct = await productRepository.ExistsAsync(p => p.Id == menuItem.ProductId);
        if (!existingPlace) 
            throw new NotFoundException($"Product with id {menuItem.PlaceId} not found");

        if (menuItem.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.");

    }

    public async Task UpdateAsync(UpsertMenuItemDTO menuItem)
    {
        bool existingMenuItem = await repository.ExistsAsync(mi => mi.PlaceId == menuItem.PlaceId && mi.ProductId == menuItem.ProductId);
        if (!existingMenuItem)
            throw new NotFoundException($"MenutItem with place id {menuItem.PlaceId} and product id {menuItem.ProductId} not found");

        await ValidateMenuItemAsync(menuItem);

        var newMenuItem = mapper.Map<MenuItems>(menuItem);
        await repository.UpdateAsync(newMenuItem);
    }

    public async Task DeleteAsync(int placeId, int productId)
    {
        bool existingMenuItem = await repository.ExistsAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);
        if (!existingMenuItem)
            throw new NotFoundException($"MenutItem with place id {placeId} and product id {productId} not found");

        var menuItem = await repository.GetByKeyAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);
        await repository.DeleteAsync(menuItem);
    }


    public async Task<IEnumerable<MenuItemsBaseDTO>> GetFilteredAsync(int placeId, string searchProduct)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);

        if (!existingPlace)
            throw new NotFoundException($"Place with id {placeId} not found");

        var menu = await repository
            .QueryIncluding(mi => mi.Product, mi => mi.Place, mi => mi.Product.Category)
            .Where(mi => mi.PlaceId == placeId && mi.Product.Name.ToLower().Contains(searchProduct.ToLower()))
        .ToListAsync();

        if (!menu.Any())
        {
            throw new NotFoundException("This place does not have any products that match the criteria");
        }
        return mapper.Map<IEnumerable<MenuItemsBaseDTO>>(menu);
    }
}

