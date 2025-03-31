using AutoMapper;
using Bartender.Domain.Interfaces;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItems;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Services;
public class MenuItemService(
    IRepository<MenuItems> repository,
    IRepository<Places> placeRepository,
    IRepository<Products> productRepository,
    ILogger<MenuItemService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper) : IMenuItemService
{
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again later.";

    public async Task<ServiceResult<List<MenuItemBaseDto>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetPlaceMenuItemsQuery(id, onlyAvailable);

            var menu = await query
                .OrderBy(mi => mi.Product != null ? mi.Product.Name : "")
                .ToListAsync();

            var dto = mapper.Map<List<MenuItemBaseDto>>(menu);
            return ServiceResult<List<MenuItemBaseDto>>.Ok(dto);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<List<MenuItemBaseDto>>.Fail(ex.Message, ErrorType.NotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return ServiceResult<List<MenuItemBaseDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    /// <summary>
    /// retrieves menu items for a specific place grouped by product categories,
    /// with optional filtering for available items only
    /// </summary>
    /// <param name="id"></param>
    /// <param name="onlyAvailable"></param>
    /// <returns>Menu items grouped by product category for a single place</returns>
    public async Task<ServiceResult<List<GroupedCategoryMenuDto>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetPlaceMenuItemsQuery(id, onlyAvailable);

            var menuItems = await query.ToListAsync();

            var groupedMenu = menuItems
                .GroupBy(mi => mi.Product != null && mi.Product.Category != null
                    ? mi.Product.Category.Name
                    : "Unknown")
                .Select(g => new GroupedCategoryMenuDto
                {
                    Category = g.Key,
                    Items = g.OrderBy(mi => mi.Product != null ? mi.Product.Name : "")
                    .Select(mi => new MenuItemBaseDto
                    {
                        Product = mapper.Map<ProductBaseDto>(mi.Product),
                        Price = mi.Price,
                        Description = mi.Description,
                        IsAvailable = mi.IsAvailable,
                    })
                    .ToList()
                })
                .Where(g => g.Items.Any())
                .ToList();
        
            return ServiceResult<List<GroupedCategoryMenuDto>>.Ok(groupedMenu);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<List<GroupedCategoryMenuDto>>.Fail(ex.Message, ErrorType.NotFound);
        }
        catch (Exception ex) {
            logger.LogError(ex, "An unexpected error occurred while retrieving the menu items.");
            return ServiceResult<List<GroupedCategoryMenuDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
    
        } 
    }

    private async Task<IQueryable<MenuItems>> GetPlaceMenuItemsQuery(int id, bool onlyAvailable)
    {
        if (!await placeRepository.ExistsAsync(p => p.Id == id))
            throw new NotFoundException($"Place with id {id} not found");

        var query = repository
            .QueryIncluding(mi => mi.Product!, mi => mi.Product!.Category) 
            .Where(mi => mi.PlaceId == id);                   

        if (onlyAvailable)
            query = query.Where(mi => mi.IsAvailable);

        return query;
    }

    public async Task<ServiceResult<MenuItemDto?>> GetByIdAsync(int placeId, int productId)
    {
        try
        {
            var menuItem = await repository.GetByKeyAsync(
                mi => mi.PlaceId == placeId && mi.ProductId == productId,
                true,
                mi => mi.Product!.Category, mi => mi.Place!.Business!);

            if (menuItem == null)
                return ServiceResult<MenuItemDto?>.Fail($"MenuItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

            var dto = mapper.Map<MenuItemDto>(menuItem);

            return ServiceResult<MenuItemDto?>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the menu item.");
            return ServiceResult<MenuItemDto?>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    /// <summary>
    /// retrieves all menus grouped by their places with products sorted alphabetically
    /// </summary>
    /// <returns>List of places with their menus when successful</returns>
    public async Task<ServiceResult<List<GroupedPlaceMenuDto>>> GetAllAsync()
    {
        try
        {
            var groupedMenus = await placeRepository.Query()
            .Select(p => new GroupedPlaceMenuDto
            {
                Place = new PlaceDto
                {
                    BusinessName = p.Business != null ? p.Business.Name : "Unknown",
                    Address = p.Address,
                    CityName = p.City != null ? p.City.Name : "Unknown",
                    WorkHours = $"{p.OpensAt:hh\\:mm} - {p.ClosesAt:hh\\:mm}"
                },
                Items = p.MenuItems
                    .Where(mi => mi.Product != null)
                    .OrderBy(mi => mi.Product!.Name)
                    .Select(mi => new MenuItemBaseDto
                    {
                        Product = new ProductBaseDto
                        {
                            Name = mi.Product!.Name,
                            Volume = mi.Product!.Volume != null ? mi.Product!.Volume : "",
                            Category = mi.Product.Category != null
                                ? mi.Product.Category.Name
                                : "Uncategorized"
                        },
                        Price = mi.Price,
                        Description = mi.Description,
                        IsAvailable = mi.IsAvailable
                    })
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync();

            return ServiceResult<List<GroupedPlaceMenuDto>>.Ok(groupedMenus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving the menu items.");
            return ServiceResult<List<GroupedPlaceMenuDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
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
        catch (Exception ex) when (ex is NotFoundException || ex is ValidationException || ex is UnauthorizedAccessException)
        {
            var errorType = ex is NotFoundException ? ErrorType.NotFound :
               ex is ValidationException ? ErrorType.Validation :
               ErrorType.Unauthorized;

            return ServiceResult.Fail(ex.Message, errorType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while adding the menu item.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<FailedMenuItemDto>>> AddMultipleAsync(List<UpsertMenuItemDto> menuItems)
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
                    throw new ValidationException($"Duplicate item at place {menuItem.PlaceId}, product {menuItem.ProductId}");
                }

                await ValidateMenuItemAsync(menuItem);

                validMenuItems.Add(mapper.Map<MenuItems>(menuItem));
            }
            catch (Exception ex) {
                var errorMessage = ex switch
                {
                    UnauthorizedAccessException => ex.Message,
                    ValidationException => ex.Message,
                    NotFoundException => ex.Message,
                    _ => "An unexpected error occurred."
                };

                failedMenuItems.Add(new FailedMenuItemDto
                {
                    MenuItem = menuItem,
                    ErrorMessage = errorMessage,
                });
            }
        }

        if (validMenuItems.Any())
        {
            try
            {
                await repository.AddMultipleAsync(validMenuItems);
                logger.LogInformation($"User {currentUser.UserId} added {validMenuItems.Count} products to the menu.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding the menu item.");
                return ServiceResult<List<FailedMenuItemDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
            }
        }


        return failedMenuItems.Any()
            ? ServiceResult<List<FailedMenuItemDto>>.Fail($"Successfully added {validMenuItems.Count}, failed: {failedMenuItems.Count}", ErrorType.Conflict, failedMenuItems)
            : ServiceResult<List<FailedMenuItemDto>>.Ok(failedMenuItems);
        
    }

    public async Task<ServiceResult<List<FailedMenuItemDto>>> CopyMenuAsync(int fromPlaceId, int toPlaceId)
    {
        var validMenuItems = new List<MenuItems>();
        var failedMenuItems = new List<FailedMenuItemDto>();

        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == fromPlaceId);
        if (!existingPlace)
            return ServiceResult<List<FailedMenuItemDto>>.Fail($"Place with id {fromPlaceId} not found", ErrorType.NotFound);

        bool existingPlace2 = await placeRepository.ExistsAsync(p => p.Id == toPlaceId);
        if (!existingPlace2)
            return ServiceResult<List<FailedMenuItemDto>>.Fail($"Place with id {toPlaceId} not found", ErrorType.NotFound);

        if (!await VerifyUserPlaceAccess(toPlaceId) || !await VerifySameBusinessAccess(fromPlaceId, toPlaceId))
            return ServiceResult<List<FailedMenuItemDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        try
        {
            var menuItemsToCopy = await (await GetPlaceMenuItemsQuery(fromPlaceId, false)).ToListAsync();

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
            {
                try
                {
                    await repository.AddMultipleAsync(validMenuItems);
                    logger.LogInformation($"User {currentUser.UserId} copied {validMenuItems.Count} menu items from place {fromPlaceId} to place {toPlaceId}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occurred while copying the menu items.");
                    return ServiceResult<List<FailedMenuItemDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
                }
            }

            return failedMenuItems.Any()
                ? ServiceResult<List<FailedMenuItemDto>>.Fail($"Successfully copied {validMenuItems.Count}, failed: {failedMenuItems.Count}", ErrorType.Conflict, failedMenuItems)
                : ServiceResult<List<FailedMenuItemDto>>.Ok(failedMenuItems);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while copying the menu items.");
            return ServiceResult<List<FailedMenuItemDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }

    }

    public async Task<ServiceResult> UpdateAsync(UpsertMenuItemDto menuItem)
    {
        try {
            if (!await VerifyUserPlaceAccess(menuItem.PlaceId))
                return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

            bool existingMenuItem = await repository.ExistsAsync(mi => mi.PlaceId == menuItem.PlaceId && mi.ProductId == menuItem.ProductId);
            if (!existingMenuItem)
                return ServiceResult.Fail($"MenuItem with place id {menuItem.PlaceId} and product id {menuItem.ProductId} not found", ErrorType.NotFound);

            await ValidateMenuItemAsync(menuItem);
      
            var newMenuItem = mapper.Map<MenuItems>(menuItem);
            await repository.UpdateAsync(newMenuItem);
            logger.LogInformation($"User {currentUser.UserId} updated product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is NotFoundException || ex is ValidationException || ex is UnauthorizedAccessException)
        {
            var errorType = ex is NotFoundException ? ErrorType.NotFound :
               ex is ValidationException ? ErrorType.Validation :
               ErrorType.Unauthorized;

            return ServiceResult.Fail(ex.Message, errorType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating the menu item.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
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
                return ServiceResult.Fail($"MenuItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);
            }

            menuItem.IsAvailable = isAvailable;
            await repository.UpdateAsync(menuItem);
            logger.LogInformation($"User {currentUser.UserId} updated availability for product {menuItem.ProductId} in menu for place {menuItem.PlaceId}. New availability: {isAvailable}");
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating the menu item.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
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
                return ServiceResult.Fail($"MenuItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

            await repository.DeleteAsync(menuItem);

            logger.LogInformation($"User {currentUser.UserId} deleted product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting the menu item.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }


    public async Task<ServiceResult<List<MenuItemBaseDto>>> GetFilteredAsync(int placeId, string searchProduct)
    {
        try
        {
            bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);

            if (!existingPlace)
                return ServiceResult<List<MenuItemBaseDto>>.Fail($"Place with id {placeId} not found", ErrorType.NotFound);

            var menu = await repository
                .QueryIncluding(mi => mi.Product!, mi => mi.Place!, mi => mi.Product!.Category)
                .Where(mi => mi.PlaceId == placeId && mi.Product != null &&
                EF.Functions.ILike(mi.Product.Name, $"%{searchProduct}%"))
            .ToListAsync();

            var dto = mapper.Map<List<MenuItemBaseDto>>(menu);
            return ServiceResult<List<MenuItemBaseDto>>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while retrieving the menu items.");
            return ServiceResult<List<MenuItemBaseDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task ValidateMenuItemAsync(UpsertMenuItemDto menuItem)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == menuItem.PlaceId);
        if (!existingPlace)
            throw new NotFoundException($"Place with id {menuItem.PlaceId} not found");

        var existingProduct = await productRepository.GetByIdAsync(menuItem.ProductId, true);
        if (existingProduct == null)
            throw new NotFoundException($"Product with id {menuItem.PlaceId} not found");

        if (menuItem.Price <= 0)
            throw new ValidationException("Price must be greater than zero.");

        var user = await currentUser.GetCurrentUserAsync();
        if (existingProduct.BusinessId != null && existingProduct.BusinessId != user!.Place!.BusinessId)
            throw new UnauthorizedAccessException($"Access to product with id {menuItem.ProductId} denied");

    }

    private async Task<bool> VerifyUserPlaceAccess(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        
        if (user!.Role == EmployeeRole.admin) // TODO: Add Owner role check when implemented
            return true;

        return targetPlaceId == user.PlaceId;
    }

    private async Task<bool> VerifySameBusinessAccess(int placeId1, int placeId2)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user!.Role == EmployeeRole.admin)
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

