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

namespace Bartender.Domain.Services;
public class MenuItemService(
    IRepository<MenuItems> repository,
    IRepository<Places> placeRepository,
    IRepository<Products> productRepository,
    ILogger<MenuItemService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper) : IMenuItemService
{
    public async Task<ServiceResult<IEnumerable<MenuItemsBaseDTO>>> GetByPlaceIdAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetBaseQuery(id, onlyAvailable);

            var menu = await query
                .OrderBy(mi => mi.Product.Name)
                .ProjectTo<MenuItemsBaseDTO>(mapper.ConfigurationProvider)
                .ToListAsync();

            if (!menu.Any())
                return ServiceResult<IEnumerable<MenuItemsBaseDTO>>.Fail("This place does not have any products on the menu at the moment", ErrorType.NotFound);

            return ServiceResult<IEnumerable<MenuItemsBaseDTO?>>.Ok(menu);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<MenuItemsBaseDTO?>>.Fail(ex.Message, ErrorType.NotFound);
        }
    }

    // grouping menu items by product categories
    public async Task<ServiceResult<IEnumerable<GroupedCategoryMenu>>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false)
    {
        try
        {
            var query = await GetBaseQuery(id, onlyAvailable);

            if (!await query.AnyAsync())
                return ServiceResult<IEnumerable<GroupedCategoryMenu>>.Fail("This place does not have any products on the menu at the moment", ErrorType.NotFound);

            var groupedMenu = await query
                .GroupBy(mi => mi.Product.Category)
                .Select(g => new GroupedCategoryMenu
                {
                    Category = g.Key.Name,
                    Items = g.OrderBy(mi => mi.Product.Name)
                    .Select(mi => new MenuItemsBaseDTO
                    {
                        Product = mapper.Map<ProductsBaseDTO>(mi.Product),
                        Price = mi.Price,
                        Description = mi.Description,
                        IsAvailable = mi.IsAvailable,
                    })
                    .ToList()
                })
                .ToListAsync();
            return ServiceResult<IEnumerable<GroupedCategoryMenu?>>.Ok(groupedMenu);
        }
        catch (Exception ex) {
            return ServiceResult<IEnumerable<GroupedCategoryMenu?>>.Fail(ex.Message, ErrorType.NotFound);
        } 
    }

    private async Task<IQueryable<MenuItems>> GetBaseQuery(int id, bool onlyAvailable)
    {
        if (!await placeRepository.ExistsAsync(p => p.Id == id))
            throw new NotFoundException($"Place with id {id} not found");

        var query = repository
            .QueryIncluding(mi => mi.Product, mi => mi.Product.Category) 
            .Where(mi => mi.PlaceId == id);                   

        // apply availability filter if requested
        if (onlyAvailable)
            query = query.Where(mi => mi.IsAvailable);

        return query;
    }

    public async Task<ServiceResult<MenuItemsDTO?>> GetByIdAsync(int placeId, int productId)
    {
        var menuItem = await repository.GetByKeyAsync(
            mi => mi.PlaceId == placeId && mi.ProductId == productId,
            mi => mi.Product.Category, mi => mi.Place.Business);

        if (menuItem == null)
            return ServiceResult<MenuItemsDTO?>.Fail($"MenutItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

        var dto = mapper.Map<MenuItemsDTO>(menuItem);

        return ServiceResult<MenuItemsDTO?>.Ok(dto);
    }

    // get all menus grouped by their places
    public async Task<ServiceResult<IEnumerable<GroupedPlaceMenuDTO>>> GetAllAsync()
    {
        var groupedMenus =  await placeRepository.Query()
        .Include(p => p.Business)
        .Include(p => p.MenuItems)
            .ThenInclude(mi => mi.Product)
                .ThenInclude(p => p.Category)
        .ToListAsync();

        var dto = mapper.Map<IEnumerable<GroupedPlaceMenuDTO>>(groupedMenus);

        return ServiceResult<IEnumerable<GroupedPlaceMenuDTO>>.Ok(dto);
    }

    public async Task<ServiceResult> AddAsync(UpsertMenuItemDTO menuItem)
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

    public async Task<ServiceResult<IEnumerable<FailedMenuItemsDTO>>> AddMultipleAsync(IEnumerable<UpsertMenuItemDTO> menuItems)
    {
        var validMenuItems = new List<MenuItems>();
        var failedMenuItems = new List<FailedMenuItemsDTO>();

        foreach (var menuItem in menuItems)
        {
            try
            {
                if (!await VerifyUserPlaceAccess(menuItem.PlaceId))
                    throw new UnauthorizedAccessException("Cross-business access denied.");

                await ValidateMenuItemAsync(menuItem);

                if (await repository.ExistsAsync(mi =>
                    mi.ProductId == menuItem.ProductId &&
                    mi.PlaceId == menuItem.PlaceId))
                {
                    throw new DuplicateEntryException($"Duplicate item at place {menuItem.PlaceId}, product {menuItem.ProductId}");
                }

                validMenuItems.Add(mapper.Map<MenuItems>(menuItem));
            }
            catch (Exception ex) {
                failedMenuItems.Add(new FailedMenuItemsDTO
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
                return ServiceResult<IEnumerable<FailedMenuItemsDTO>>.Fail(ex.Message, ErrorType.Unknown);
            }


        return failedMenuItems.Any()
            ? ServiceResult<IEnumerable<FailedMenuItemsDTO>>.Fail($"Successfully added {validMenuItems.Count}, failed: {failedMenuItems.Count}", ErrorType.Conflict, failedMenuItems)
            : ServiceResult<IEnumerable<FailedMenuItemsDTO>>.Ok(failedMenuItems);
        
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
            throw new ArgumentException("Price must be greater than zero.");

    }

    public async Task<ServiceResult> UpdateAsync(UpsertMenuItemDTO menuItem)
    {
        if (!await VerifyUserPlaceAccess(menuItem.PlaceId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        bool existingMenuItem = await repository.ExistsAsync(mi => mi.PlaceId == menuItem.PlaceId && mi.ProductId == menuItem.ProductId);
        if (!existingMenuItem)
            return ServiceResult.Fail($"MenutItem with place id {menuItem.PlaceId} and product id {menuItem.ProductId} not found", ErrorType.NotFound);

        try
        {
            await ValidateMenuItemAsync(menuItem);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(ex.Message, ErrorType.Validation);
        }

        var newMenuItem = mapper.Map<MenuItems>(menuItem);
        await repository.UpdateAsync(newMenuItem);
        logger.LogInformation($"User {currentUser.UserId} updated product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int placeId, int productId)
    {
        if (!await VerifyUserPlaceAccess(placeId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        bool existingMenuItem = await repository.ExistsAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);
        if (!existingMenuItem)
            return ServiceResult.Fail($"MenutItem with place id {placeId} and product id {productId} not found", ErrorType.NotFound);

        var menuItem = await repository.GetByKeyAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);
        await repository.DeleteAsync(menuItem);
        logger.LogInformation($"User {currentUser.UserId} deleted product {menuItem.ProductId} in menu for place {menuItem.PlaceId}");
        return ServiceResult.Ok();
    }


    public async Task<ServiceResult<IEnumerable<MenuItemsBaseDTO>>> GetFilteredAsync(int placeId, string searchProduct)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);

        if (!existingPlace)
            throw new NotFoundException($"Place with id {placeId} not found");

        var menu = await repository
            .QueryIncluding(mi => mi.Product, mi => mi.Place, mi => mi.Product.Category)
            .Where(mi => mi.PlaceId == placeId && mi.Product.Name.ToLower().Contains(searchProduct.ToLower()))
        .ToListAsync();

        if (!menu.Any())
            return ServiceResult<IEnumerable<MenuItemsBaseDTO>>.Fail("This place does not have any products that match the criteria", ErrorType.NotFound);

        var dto = mapper.Map<IEnumerable<MenuItemsBaseDTO>>(menu);
        return ServiceResult<IEnumerable<MenuItemsBaseDTO>>.Ok(dto);
    }

    private async Task<bool> VerifyUserPlaceAccess(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user.Role == EmployeeRole.admin)
            return true;

        return targetPlaceId == user.PlaceId;
    }
}

