﻿using AutoMapper;
using Bartender.Domain.Interfaces;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bartender.Data.Enums;
using System.ComponentModel.DataAnnotations;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;

namespace Bartender.Domain.Services.Data;
public class MenuItemService(
    IMenuItemRepository repository,
    IRepository<Place> placeRepository,
    IRepository<Product> productRepository,
    ILogger<MenuItemService> logger,
    ICurrentUserContext currentUser,
    IValidationService validationService,
    IMapper mapper) : IMenuItemService
{

    public async Task<List<MenuItemBaseDto>> GetByPlaceIdAsync(int id, bool onlyAvailable = false)
    {
        if (!await placeRepository.ExistsAsync(p => p.Id == id))
            throw new PlaceNotFoundException(id);

        var menu = await repository.GetMenuItemsByPlaceIdAync(id, onlyAvailable);

        var dto = mapper.Map<List<MenuItemBaseDto>>(menu);
        return dto;
    }

    /// <summary>
    /// retrieves menu items for a specific place grouped by product categories,
    /// with optional filtering for available items only
    /// </summary>
    /// <param name="id"></param>
    /// <param name="onlyAvailable"></param>
    /// <returns>Menu items grouped by product category for a single place</returns>
    public async Task<List<GroupedCategoryMenuDto>> GetByPlaceIdGroupedAsync(int id, bool onlyAvailable = false)
    {
        if (!await placeRepository.ExistsAsync(p => p.Id == id))
            throw new PlaceNotFoundException(id);

        var groupedMenu = await repository.GetMenuItemsByPlaceIdGroupedAsync(id, onlyAvailable);

        var groupedMenuDto = groupedMenu
            .Select( g => new GroupedCategoryMenuDto
            {
                Category = g.Key,
                Items = mapper.Map<List<MenuItemBaseDto>>(g.Value)
            }).ToList();
             
        return groupedMenuDto;
    }

    public async Task<MenuItemDto?> GetByIdAsync(int placeId, int productId)
    {
        var menuItem = await repository.GetByKeyAsync(
            mi => mi.PlaceId == placeId && mi.ProductId == productId,
            true,
            mi => mi.Product!.Category, mi => mi.Place!.Business!);

        if (menuItem == null)
            throw new MenuItemNotFoundException(placeId, productId);

        var dto = mapper.Map<MenuItemDto>(menuItem);

        return dto;
    }

    /// <summary>
    /// retrieves all menus grouped by their places with products sorted alphabetically
    /// </summary>
    /// <returns>List of places with their menus when successful</returns>
    public async Task<List<GroupedPlaceMenuDto>> GetAllAsync()
    {
        var groupedMenus = await repository.GetAllGroupedByPlaceAsync();
        var menuDto = groupedMenus
            .Select(g => new GroupedPlaceMenuDto
            {
                Place = mapper.Map<PlaceDto>(g.Key),
                Items = mapper.Map<List<MenuItemBaseDto>>(g.Value)
            }).ToList();

        return menuDto;
    }

    public async Task AddAsync(UpsertMenuItemDto menuItem)
    {
        if (!await validationService.VerifyUserPlaceAccess(menuItem.PlaceId))
            throw new UnauthorizedPlaceAccessException();

        await ValidateMenuItemAsync(menuItem);

        bool existingMenuItem = await repository.ExistsAsync(mi =>
            mi.ProductId == menuItem.ProductId &&
            mi.PlaceId == menuItem.PlaceId);

        if (existingMenuItem)
            throw new ConflictException($"The menu item with product ID {menuItem.ProductId} already exists at the place with ID {menuItem.PlaceId}");

        var newMenuItem = mapper.Map<MenuItem>(menuItem);
        await repository.AddAsync(newMenuItem);
        logger.LogInformation("User {UserId} added product {ProductId} in menu for place {PlaceId}",
            currentUser.UserId, menuItem.ProductId, menuItem.PlaceId);
    }

    public async Task<List<FailedMenuItemDto>> AddMultipleAsync(List<UpsertMenuItemDto> menuItems)
    {
        var validMenuItems = new List<MenuItem>();
        var failedMenuItems = new List<FailedMenuItemDto>();

        foreach (var menuItem in menuItems)
        {
            try
            {
                if (!await validationService.VerifyUserPlaceAccess(menuItem.PlaceId))
                    throw new UnauthorizedAccessException("Cross-business access denied.");   

                if (await repository.ExistsAsync(mi =>
                    mi.ProductId == menuItem.ProductId &&
                    mi.PlaceId == menuItem.PlaceId))
                {
                    throw new ValidationException($"Duplicate item at place {menuItem.PlaceId}, product {menuItem.ProductId}");
                }

                await ValidateMenuItemAsync(menuItem);

                validMenuItems.Add(mapper.Map<MenuItem>(menuItem));
            }
            catch (Exception ex) {
                var errorMessage = ex switch
                {
                    UnauthorizedAccessException => ex.Message,
                    ValidationException => ex.Message,
                    AppValidationException => ex.Message,
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

        if (validMenuItems.Count > 0)
        {
            await repository.AddMultipleAsync(validMenuItems);
            logger.LogInformation("User {UserId} added {Count} products to the menu.",
                currentUser.UserId, validMenuItems.Count);
        }

        if (failedMenuItems.Count > 0)
            throw new ConflictException($"Successfully added {validMenuItems.Count}, failed: {failedMenuItems.Count}", data: failedMenuItems);

        return failedMenuItems;
    }

    public async Task<List<FailedMenuItemDto>> CopyMenuAsync(int fromPlaceId, int toPlaceId)
    {
        var validMenuItems = new List<MenuItem>();
        var failedMenuItems = new List<FailedMenuItemDto>();

        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == fromPlaceId);
        if (!existingPlace)
            throw new PlaceNotFoundException(fromPlaceId);

        bool existingPlace2 = await placeRepository.ExistsAsync(p => p.Id == toPlaceId);
        if (!existingPlace2)
            throw new PlaceNotFoundException(toPlaceId);

        if (!await validationService.VerifyUserPlaceAccess(toPlaceId) || !await VerifySameBusinessAccess(fromPlaceId, toPlaceId))
            throw new UnauthorizedPlaceAccessException();

            var menuItemsToCopy = await repository.GetMenuItemsByPlaceIdAync(fromPlaceId, false);

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

                var newMenuItem = new MenuItem
                {
                    PlaceId = toPlaceId,
                    ProductId = m.ProductId,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = false
                };

                validMenuItems.Add(newMenuItem);
            }

            if (validMenuItems.Count > 0)
            {
                await repository.AddMultipleAsync(validMenuItems);
                logger.LogInformation(
                    "User {UserId} copied {ItemCount} menu items from place {FromPlaceId} to place {ToPlaceId}.",
                    currentUser.UserId, validMenuItems.Count, fromPlaceId, toPlaceId);

            }

        if (failedMenuItems.Count > 0)
            throw new ConflictException($"Successfully copied {validMenuItems.Count}, failed: {failedMenuItems.Count}", data: failedMenuItems);
        
        return failedMenuItems;

    }

    public async Task UpdateAsync(UpsertMenuItemDto menuItem)
    {
        if (!await validationService.VerifyUserPlaceAccess(menuItem.PlaceId))
            throw new UnauthorizedPlaceAccessException();

        var existingItem = await repository.GetByKeyAsync(mi =>
            mi.PlaceId == menuItem.PlaceId &&
            mi.ProductId == menuItem.ProductId);
        if (existingItem == null)
            throw new MenuItemNotFoundException(menuItem.PlaceId, menuItem.ProductId);

        await ValidateMenuItemAsync(menuItem);
        mapper.Map(menuItem, existingItem);

        await repository.UpdateAsync(existingItem);
        logger.LogInformation("User {UserId} updated product {ProductId} in menu for place {PlaceId}.",
                currentUser.UserId, menuItem.ProductId, menuItem.PlaceId);
    }

    public async Task UpdateItemAvailabilityAsync(int placeId, int productId, bool isAvailable)
    {

        if (!await validationService.VerifyUserPlaceAccess(placeId))
        {
            throw new UnauthorizedPlaceAccessException();
        }
        var menuItem = await repository.GetByKeyAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);
        if (menuItem == null) {
            throw new MenuItemNotFoundException(placeId, productId);
        }

        menuItem.IsAvailable = isAvailable;
        await repository.UpdateAsync(menuItem);
        logger.LogInformation("User {UserId} updated availability for product {ProductId} in menu for place {PlaceId}. New availability: {IsAvailable}",
            currentUser.UserId, menuItem.ProductId, menuItem.PlaceId, isAvailable);
    }

    public async Task DeleteAsync(int placeId, int productId)
    {
        if (!await validationService.VerifyUserPlaceAccess(placeId))
            throw new UnauthorizedPlaceAccessException();

        var menuItem = await repository.GetByKeyAsync(mi => mi.PlaceId == placeId && mi.ProductId == productId);

        if (menuItem == null)
            throw new MenuItemNotFoundException(placeId, productId);

        menuItem.DeletedAt = DateTime.UtcNow;
        await repository.UpdateAsync(menuItem);

        logger.LogInformation("User {UserId} deleted product {ProductId} in menu for place {PlaceId}",
            currentUser.UserId, menuItem.ProductId, menuItem.PlaceId);
    }

    public async Task<List<MenuItemBaseDto>> GetFilteredAsync(int placeId, string searchProduct)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == placeId);

        if (!existingPlace)
            throw new PlaceNotFoundException(placeId);

        var menu = await repository
            .QueryIncluding(mi => mi.Product!, mi => mi.Place!, mi => mi.Product!.Category)
            .Where(mi => mi.PlaceId == placeId && mi.Product != null &&
            EF.Functions.ILike(mi.Product.Name, $"%{searchProduct}%"))
        .ToListAsync();

        var dto = mapper.Map<List<MenuItemBaseDto>>(menu);
        return dto;
    }

    public async Task ValidateMenuItemAsync(UpsertMenuItemDto menuItem)
    {
        bool existingPlace = await placeRepository.ExistsAsync(p => p.Id == menuItem.PlaceId);
        if (!existingPlace)
            throw new NotFoundException($"Place with id {menuItem.PlaceId} not found");

        var existingProduct = await productRepository.GetByIdAsync(menuItem.ProductId, true);
        if (existingProduct == null)
            throw new NotFoundException($"Product with id {menuItem.ProductId} not found");

        if (menuItem.Price < 0)
            throw new ValidationException("Price must be greater than zero.");

        var user = await currentUser.GetCurrentUserAsync();
        if (existingProduct.BusinessId != null && existingProduct.BusinessId != user!.Place!.BusinessId)
            throw new UnauthorizedAccessException($"Access to product with id {menuItem.ProductId} denied");

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