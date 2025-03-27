using AutoMapper;
using Bartender.Domain.Interfaces;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Bartender.Domain.DTO.Places;

namespace Bartender.Domain.Services;
public class MenuItemService(
    IRepository<MenuItems> repository,
    IRepository<Places> placeRepository,
    IRepository<Products> productRepository,
    ILogger<MenuItemService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper) : IMenuItemService
{
    public async Task<ServiceResult<IEnumerable<MenuItemBaseDTO>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetBaseQuery(id, onlyAvailable);

            var menu = await query
                .OrderBy(mi => mi.Product.Name)
                .ProjectTo<MenuItemBaseDTO>(mapper.ConfigurationProvider)
                .ToListAsync();

            return ServiceResult<IEnumerable<MenuItemBaseDTO?>>.Ok(menu);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<IEnumerable<MenuItemBaseDTO?>>.Fail(ex.Message, ErrorType.NotFound);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<MenuItemBaseDTO?>>.Fail(ex.Message, ErrorType.Unknown);
        }
    }

    // grouping menu items by product categories
    public async Task<ServiceResult<IEnumerable<GroupedCategoryMenu>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetBaseQuery(id, onlyAvailable);

            var groupedMenu = await query
                .GroupBy(mi => mi.Product.Category)
                .Select(g => new GroupedCategoryMenu
                {
                    Category = g.Key.Name,
                    Items = g.OrderBy(mi => mi.Product.Name)
                    .Select(mi => new MenuItemBaseDTO
                    {
                        Product = mapper.Map<ProductBaseDTO>(mi.Product),
                        Price = mi.Price,
                        Description = mi.Description,
                        IsAvailable = mi.IsAvailable,
                    })
                    .ToList()
                })
                .ToListAsync();
            return ServiceResult<IEnumerable<GroupedCategoryMenu?>>.Ok(groupedMenu);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<IEnumerable<GroupedCategoryMenu?>>.Fail(ex.Message, ErrorType.NotFound);
        }
        catch (Exception ex) {
            return ServiceResult<IEnumerable<GroupedCategoryMenu?>>.Fail($"An error occurred while retrieving the menu items: {ex.Message}", ErrorType.Unknown);
        } 
    }

    private async Task<IQueryable<MenuItems>> GetBaseQuery(int id, bool onlyAvailable)
    {
        if (!await placeRepository.ExistsAsync(p => p.Id == id))
            throw new NotFoundException($"Place with id {id} not found");

        var query = repository
            .QueryIncluding(mi => mi.Product, mi => mi.Product.Category) 
            .Where(mi => mi.PlaceId == id);                   

        if (onlyAvailable)
            query = query.Where(mi => mi.IsAvailable);

        if (!await query.AnyAsync())
            throw new NotFoundException("This place does not have any products on the menu at the moment");

        return query;
    }

    public async Task<ServiceResult<MenuItemDTO?>> GetByIdAsync(int placeId, int productId)
    {
        try
        {
            var menuItem = await repository.GetByKeyAsync(
                mi => mi.PlaceId == placeId && mi.ProductId == productId,
                mi => mi.Product.Category, mi => mi.Place.Business);

            if (menuItem == null)
                return ServiceResult<MenuItemDTO?>.Fail($"MenutItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

            var dto = mapper.Map<MenuItemDTO>(menuItem);

            return ServiceResult<MenuItemDTO?>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<MenuItemDTO?>.Fail($"An error occurred while fetching the menu item: {ex.Message}", ErrorType.Unknown);
        }
    }

    // get all menus grouped by their places and sort by product name
    public async Task<ServiceResult<IEnumerable<GroupedPlaceMenuDTO>>> GetAllAsync()
    {
        try
        {
            var groupedMenus = await placeRepository.Query()
            .Include(p => p.Business)
            .Include(p => p.MenuItems)
                .ThenInclude(mi => mi.Product)
                    .ThenInclude(p => p.Category)
            .Select(g => new GroupedPlaceMenuDTO
            {
                Place = mapper.Map<PlaceDTO>(g),
                Items = g.MenuItems
                .OrderBy(m => m.Product.Name)
                .Select(it => new MenuItemBaseDTO
                {
                    Product = mapper.Map<ProductBaseDTO>(it.Product),
                    Price = it.Price,
                    Description = it.Description,
                    IsAvailable = it.IsAvailable,
                })
            }).ToListAsync();

            return ServiceResult<IEnumerable<GroupedPlaceMenuDTO>>.Ok(groupedMenus);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<GroupedPlaceMenuDTO?>>.Fail($"An error occurred while retrieving the menu items: {ex.Message}", ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> AddAsync(UpsertMenuItemDTO menuItem)
    {
        try
        {
            if (!await VerifyUserPlaceAccess(menuItem.PlaceId))
                return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

            await ValidateMenuItemAsync(menuItem);

            bool existingMenuItem = await repository.ExistsAsync(mi =>
                mi.ProductId == menuItem.ProductId &&
                mi.PlaceId == menuItem.PlaceId);

            if (existingMenuItem)
                return ServiceResult.Fail($"The menu item with product ID {menuItem.ProductId} already exists at the place with ID {menuItem.PlaceId}", ErrorType.Conflict);  

            var newMenuItem = mapper.Map<MenuItems>(menuItem);
            await repository.AddAsync(newMenuItem);
            logger.LogInformation($"User {currentUser.UserId} added product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            var errorType = ex switch
            {
                NotFoundException => ErrorType.NotFound,
                ValidationException => ErrorType.Validation,
                _ => ErrorType.Unknown
            };

            return ServiceResult.Fail(ex.Message, errorType);
        }
    }

    public async Task<ServiceResult<IEnumerable<FailedMenuItemDTO>>> AddMultipleAsync(IEnumerable<UpsertMenuItemDTO> menuItems)
    {
        var validMenuItems = new List<MenuItems>();
        var failedMenuItems = new List<FailedMenuItemDTO>();

        foreach (var menuItem in menuItems)
        {
            try
            {
                if (!await VerifyUserPlaceAccess(menuItem.PlaceId))
                    throw new UnauthorizedAccessException("Cross-business access denied.");   

                if (await repository.ExistsAsync(mi =>
                    mi.ProductId == menuItem.ProductId &&
                    mi.PlaceId == menuItem.PlaceId))
                {
                    throw new DuplicateEntryException($"Duplicate item at place {menuItem.PlaceId}, product {menuItem.ProductId}");
                }

                await ValidateMenuItemAsync(menuItem);

                validMenuItems.Add(mapper.Map<MenuItems>(menuItem));
            }
            catch (Exception ex) {
                failedMenuItems.Add(new FailedMenuItemDTO
                {
                    MenuItem = menuItem,
                    ErrorMessage = ex.Message,
                });
            }
        }

        if (validMenuItems.Any())
            try
            {
                await repository.AddMultipleAsync(validMenuItems);
            }
            catch (Exception ex) {
                return ServiceResult<IEnumerable<FailedMenuItemDTO>>.Fail($"An error occured while adding the items: {ex.Message}", ErrorType.Unknown);
            }


        return failedMenuItems.Any()
            ? ServiceResult<IEnumerable<FailedMenuItemDTO>>.Fail($"Successfully added {validMenuItems.Count}, failed: {failedMenuItems.Count}", ErrorType.Conflict, failedMenuItems)
            : ServiceResult<IEnumerable<FailedMenuItemDTO>>.Ok(failedMenuItems);
        
    }

    public async Task ValidateMenuItemAsync(UpsertMenuItemDTO menuItem)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == menuItem.PlaceId);
        if (!existingPlace) 
            throw new NotFoundException($"Place with id {menuItem.PlaceId} not found");

        bool existingProduct = await productRepository.ExistsAsync(p => p.Id == menuItem.ProductId);
        if (!existingProduct) 
            throw new NotFoundException($"Product with id {menuItem.PlaceId} not found");

        if (menuItem.Price <= 0)
            throw new ValidationException("Price must be greater than zero.");

    }

    public async Task<ServiceResult> UpdateAsync(UpsertMenuItemDTO menuItem)
    {
        try {
            if (!await VerifyUserPlaceAccess(menuItem.PlaceId))
                return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

            bool existingMenuItem = await repository.ExistsAsync(mi => mi.PlaceId == menuItem.PlaceId && mi.ProductId == menuItem.ProductId);
            if (!existingMenuItem)
                return ServiceResult.Fail($"MenutItem with place id {menuItem.PlaceId} and product id {menuItem.ProductId} not found", ErrorType.NotFound);

            await ValidateMenuItemAsync(menuItem);
      
            var newMenuItem = mapper.Map<MenuItems>(menuItem);
            await repository.UpdateAsync(newMenuItem);
            logger.LogInformation($"User {currentUser.UserId} updated product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
            return ServiceResult.Ok();
        }
        catch (Exception ex) {
            var errorType = ex switch
            {
                NotFoundException => ErrorType.NotFound,
                ValidationException => ErrorType.Validation,
                _ => ErrorType.Unknown
            };
            return ServiceResult.Fail(ex.Message, errorType);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int placeId, int productId)
    {
        try
        {
            if (!await VerifyUserPlaceAccess(placeId))
                return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

            var menuItem = await repository.GetByKeyAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);

            if (menuItem == null)
                return ServiceResult.Fail($"MenutItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

            await repository.DeleteAsync(menuItem);

            logger.LogInformation($"User {currentUser.UserId} deleted product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            var errorType = ex switch
            {
                NotFoundException => ErrorType.NotFound,
                ValidationException => ErrorType.Validation,
                _ => ErrorType.Unknown
            };
            return ServiceResult.Fail(ex.Message,errorType);
        }
    }


    public async Task<ServiceResult<IEnumerable<MenuItemBaseDTO>>> GetFilteredAsync(int placeId, string searchProduct)
    {
        try
        {
            bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);

            if (!existingPlace)
                return ServiceResult<IEnumerable<MenuItemBaseDTO>>.Fail($"Place with id {placeId} not found", ErrorType.NotFound);

            var menu = await repository
                .QueryIncluding(mi => mi.Product, mi => mi.Place, mi => mi.Product.Category)
                .Where(mi => mi.PlaceId == placeId && mi.Product.Name.ToLower().Contains(searchProduct.ToLower()))
            .ToListAsync();

            if (!menu.Any())
                return ServiceResult<IEnumerable<MenuItemBaseDTO>>.Fail("This place does not have any products that match the criteria", ErrorType.NotFound);

            var dto = mapper.Map<IEnumerable<MenuItemBaseDTO>>(menu);
            return ServiceResult<IEnumerable<MenuItemBaseDTO>>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<MenuItemBaseDTO?>>.Fail($"An error occurred while retrieving the menu items: {ex.Message}", ErrorType.Unknown);
        }
    }

    private async Task<bool> VerifyUserPlaceAccess(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user.Role == EmployeeRole.admin)
            return true;

        return targetPlaceId == user.PlaceId;
    }
}

