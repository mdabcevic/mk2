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
    public async Task<ServiceResult<IEnumerable<MenuItemBaseDto>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetBaseQuery(id, onlyAvailable);

            var menu = await query
                .OrderBy(mi => mi.Product.Name)
                .ProjectTo<MenuItemBaseDto>(mapper.ConfigurationProvider)
                .ToListAsync();

            return ServiceResult<IEnumerable<MenuItemBaseDto?>>.Ok(menu);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<IEnumerable<MenuItemBaseDto?>>.Fail(ex.Message, ErrorType.NotFound);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<MenuItemBaseDto?>>.Fail(ex.Message, ErrorType.Unknown);
        }
    }

    // grouping menu items by product categories
    public async Task<ServiceResult<IEnumerable<GroupedCategoryMenuDto>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetBaseQuery(id, onlyAvailable);

            var groupedMenu = await query
                .GroupBy(mi => mi.Product.Category)
                .Select(g => new GroupedCategoryMenuDto
                {
                    Category = g.Key.Name,
                    Items = g.OrderBy(mi => mi.Product.Name)
                    .Select(mi => new MenuItemBaseDto
                    {
                        Product = mapper.Map<ProductBaseDto>(mi.Product),
                        Price = mi.Price,
                        Description = mi.Description,
                        IsAvailable = mi.IsAvailable,
                    })
                    .ToList()
                })
                .ToListAsync();
            return ServiceResult<IEnumerable<GroupedCategoryMenuDto?>>.Ok(groupedMenu);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<IEnumerable<GroupedCategoryMenuDto?>>.Fail(ex.Message, ErrorType.NotFound);
        }
        catch (Exception ex) {
            return ServiceResult<IEnumerable<GroupedCategoryMenuDto?>>.Fail($"An error occurred while retrieving the menu items: {ex.Message}", ErrorType.Unknown);
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

    public async Task<ServiceResult<MenuItemDto?>> GetByIdAsync(int placeId, int productId)
    {
        try
        {
            var menuItem = await repository.GetByKeyAsync(
                mi => mi.PlaceId == placeId && mi.ProductId == productId,
                true,
                mi => mi.Product.Category, mi => mi.Place.Business);

            if (menuItem == null)
                return ServiceResult<MenuItemDto?>.Fail($"MenutItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

            var dto = mapper.Map<MenuItemDto>(menuItem);

            return ServiceResult<MenuItemDto?>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<MenuItemDto?>.Fail($"An error occurred while fetching the menu item: {ex.Message}", ErrorType.Unknown);
        }
    }

    // get all menus grouped by their places and sort by product name
    public async Task<ServiceResult<IEnumerable<GroupedPlaceMenuDto>>> GetAllAsync()
    {
        try
        {
            var groupedMenus = await placeRepository.Query()
            .Include(p => p.Business)
            .Include(p => p.MenuItems)
                .ThenInclude(mi => mi.Product)
                    .ThenInclude(p => p.Category)
            .Select(g => new GroupedPlaceMenuDto
            {
                Place = mapper.Map<PlaceDto>(g),
                Items = g.MenuItems
                .OrderBy(m => m.Product.Name)
                .Select(it => new MenuItemBaseDto
                {
                    Product = mapper.Map<ProductBaseDto>(it.Product),
                    Price = it.Price,
                    Description = it.Description,
                    IsAvailable = it.IsAvailable,
                })
            }).ToListAsync();

            return ServiceResult<IEnumerable<GroupedPlaceMenuDto>>.Ok(groupedMenus);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<GroupedPlaceMenuDto?>>.Fail($"An error occurred while retrieving the menu items: {ex.Message}", ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> AddAsync(UpsertMenuItemDto menuItem)
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

    public async Task<ServiceResult<IEnumerable<FailedMenuItemDto>>> AddMultipleAsync(IEnumerable<UpsertMenuItemDto> menuItems)
    {
        var validMenuItems = new List<MenuItems>();
        var failedMenuItems = new List<FailedMenuItemDto>();

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
                failedMenuItems.Add(new FailedMenuItemDto
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
                return ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"An error occured while adding the items: {ex.Message}", ErrorType.Unknown);
            }


        return failedMenuItems.Any()
            ? ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"Successfully added {validMenuItems.Count}, failed: {failedMenuItems.Count}", ErrorType.Conflict, failedMenuItems)
            : ServiceResult<IEnumerable<FailedMenuItemDto>>.Ok(failedMenuItems);
        
    }

    public async Task<ServiceResult<IEnumerable<FailedMenuItemDto>>> CopyMenuAsync(int fromPlaceId, int toPlaceId)
    {
        var validMenuItems = new List<MenuItems>();
        var failedMenuItems = new List<FailedMenuItemDto>();

        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == fromPlaceId);
        if (!existingPlace)
            return ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"Place with id {fromPlaceId} not found", ErrorType.NotFound);

        bool existingPlace2 = await placeRepository.ExistsAsync(p => p.Id == toPlaceId);
        if (!existingPlace2)
            return ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"Place with id {toPlaceId} not found", ErrorType.NotFound);

        if (!await VerifyUserPlaceAccess(toPlaceId) || !await VerifySameBusinessAccess(fromPlaceId, toPlaceId))
            return ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        try
        {
            var menuItemsToCopy = await (await GetBaseQuery(fromPlaceId, false)).ToListAsync();

            var existingProductIds = await repository.QueryIncluding()
                .Where(mi => mi.PlaceId == toPlaceId)
                .Select(mi => mi.ProductId)
            .ToListAsync();

            foreach (var m in menuItemsToCopy)
            {
                if (existingProductIds.Contains(m.ProductId))
                {
                    failedMenuItems.Add(new FailedMenuItemDto
                    {
                        MenuItem = mapper.Map<UpsertMenuItemDto>(m),
                        ErrorMessage = $"Product {m.ProductId} already exists at location {toPlaceId}"
                    });
                    continue;
                }

                var newMenuItem = new MenuItems
                {
                    PlaceId = toPlaceId,
                    ProductId = m.ProductId,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = false
                };

                validMenuItems.Add(newMenuItem);
            }

            if (validMenuItems.Any())
                try
                {
                    await repository.AddMultipleAsync(validMenuItems);
                }
                catch (Exception ex)
                {
                    return ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"An error occured while adding the items: {ex.Message}", ErrorType.Unknown);
                }


            return failedMenuItems.Any()
                ? ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"Successfully copied {validMenuItems.Count}, failed: {failedMenuItems.Count}", ErrorType.Conflict, failedMenuItems)
                : ServiceResult<IEnumerable<FailedMenuItemDto>>.Ok(failedMenuItems);

        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<FailedMenuItemDto>>.Fail($"An unexpected error occurred: {ex.Message}", ErrorType.Unknown);
        }

    }

    public async Task ValidateMenuItemAsync(UpsertMenuItemDto menuItem)
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

    public async Task<ServiceResult> UpdateAsync(UpsertMenuItemDto menuItem)
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

    public async Task<ServiceResult> UpdateItemAvailabilityAsync(int placeId, int productId, bool isAvailable)
    {
        try
        {
            if (!await VerifyUserPlaceAccess(placeId))
            {
                return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);
            }
            var menuItem = await repository.GetByKeyAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);
            if (menuItem == null) {
                return ServiceResult.Fail($"MenutItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);
            }

            menuItem.IsAvailable = isAvailable;
            await repository.UpdateAsync(menuItem);
            return ServiceResult.Ok();
        }
        catch (Exception ex) {
            return ServiceResult.Fail(ex.Message, ErrorType.Unknown);
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


    public async Task<ServiceResult<IEnumerable<MenuItemBaseDto>>> GetFilteredAsync(int placeId, string searchProduct)
    {
        try
        {
            bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);

            if (!existingPlace)
                return ServiceResult<IEnumerable<MenuItemBaseDto>>.Fail($"Place with id {placeId} not found", ErrorType.NotFound);

            var menu = await repository
                .QueryIncluding(mi => mi.Product, mi => mi.Place, mi => mi.Product.Category)
                .Where(mi => mi.PlaceId == placeId && mi.Product.Name.ToLower().Contains(searchProduct.ToLower()))
            .ToListAsync();

            if (!menu.Any())
                return ServiceResult<IEnumerable<MenuItemBaseDto>>.Fail("This place does not have any products that match the criteria", ErrorType.NotFound);

            var dto = mapper.Map<IEnumerable<MenuItemBaseDto>>(menu);
            return ServiceResult<IEnumerable<MenuItemBaseDto>>.Ok(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<MenuItemBaseDto?>>.Fail($"An error occurred while retrieving the menu items: {ex.Message}", ErrorType.Unknown);
        }
    }

    private async Task<bool> VerifyUserPlaceAccess(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user.Role == EmployeeRole.admin)
            return true;

        return targetPlaceId == user.PlaceId;
    }

    private async Task<bool> VerifySameBusinessAccess(int placeId1, int placeId2)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user.Role == EmployeeRole.admin)
            return true;

        var place1BusinessId = await placeRepository.Query()
            .Where(p => p.Id == placeId1)
            .Select(p => (int?)p.BusinessId)
            .FirstOrDefaultAsync();

        var place2BusinessId = await placeRepository.Query()
            .Where(p => p.Id == placeId2)
            .Select(p => (int?)p.BusinessId)
            .FirstOrDefaultAsync();

        if (place1BusinessId == null || place2BusinessId == null)
            return false;

        return place1BusinessId == place2BusinessId;
    }
}

