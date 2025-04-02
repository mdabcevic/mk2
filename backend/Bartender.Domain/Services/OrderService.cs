using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO.Orders;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Bartender.Domain.Services;

public class OrderService(
    IRepository<Orders> repository,
    IRepository<Tables> tableRepository,
    IRepository<MenuItems> menuItemRepository,
    IRepository<ProductsPerOrder> orderProductsRepo,
    ILogger<OrderService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
    ) : IOrderService
{
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again later.";
    public async Task<ServiceResult> AddAsync(UpsertOrderDto order)
    {
        // validate order requirements
        // - table exists and is occupied
        // - order contains at least one item
        // - all menu items exist and are available
        var validationResult = await ValidateOrderAsync(order);
        if (!validationResult.Success)
            return validationResult;    

        // combine duplicate items (same MenuItemId) by summing their quantities
        order.Items = CombineDuplicateItems(order.Items);

        var newOrderItems = mapper.Map<List<ProductsPerOrder>>(order.Items);

        var menuItems = await GetOrderItemsAsync(order);
        newOrderItems.Select(o => o.Price = menuItems.FirstOrDefault(mi => mi.Id == o.MenuItemId).Price);

        // calculate total order price
        order.TotalPrice = CalculateTotalPriceWithUpdates(newOrderItems);

        order.Status = OrderStatus.created;

        // create order transaction - either completes both order and items creation or rolls back completely on any failure
        using var transaction = await repository.BeginTransactionAsync();
        try
        {
            var newOrder = mapper.Map<Orders>(order);
            await repository.AddAsync(newOrder);

            foreach (var item in newOrderItems)
                item.OrderId = newOrder.Id;

            await orderProductsRepo.AddMultipleAsync(newOrderItems);
            await transaction.CommitAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error creating order");
            return ServiceResult.Fail("Error creating order", ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertOrderDto order)
    {
        var existingOrder = await repository.GetByIdAsync(id, true);
        if (existingOrder == null) {
            return ServiceResult.Fail($"Order with id {id} not found", ErrorType.NotFound);
        }

        if (currentUser.IsGuest && existingOrder.Status != OrderStatus.created)
            return ServiceResult.Fail("Order cannot be changed anymore", ErrorType.Validation);

        // validate order requirements
        var validationResult = await ValidateOrderAsync(order);
        if (!validationResult.Success)
            return validationResult;

        // combine duplicate items (same MenuItemId) by summing their quantities
        order.Items = CombineDuplicateItems(order.Items);

        var newOrderItems = mapper.Map<List<ProductsPerOrder>>(order.Items);

        var menuItems = await GetOrderItemsAsync(order);
        newOrderItems.Select(o => o.Price = menuItems.FirstOrDefault(mi => mi.Id == o.MenuItemId).Price);

        order.TotalPrice = CalculateTotalPriceWithUpdates(newOrderItems);

        order.Status = existingOrder.Status;

        using var transaction = await repository.BeginTransactionAsync();

        // create order transaction - either completes both order and items creation or rolls back completely on any failure
        try
        {
            mapper.Map(order, existingOrder);
            await repository.UpdateAsync(existingOrder);

            var oldItems = await orderProductsRepo.GetFilteredAsync(false, it => it.OrderId == id);
            await orderProductsRepo.DeleteRangeAsync(oldItems);

            foreach (var item in newOrderItems)
                item.OrderId = existingOrder.Id;

            await orderProductsRepo.AddMultipleAsync(newOrderItems);

            await transaction.CommitAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error updating order");
            return ServiceResult.Fail("Error updating order", ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        try
        {
            var order = await repository.GetByIdAsync(id);
            if (order == null)
            {
                return ServiceResult.Fail($"Order with ID {id} not found", ErrorType.NotFound);
            }
            await repository.DeleteAsync(order);
            return ServiceResult.Ok();
        }
        catch (Exception ex) {
            logger.LogError(ex, "An unexpected error occurred while deleting a order.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<OrderDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<OrderDto?>> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    

    private async Task<ServiceResult> ValidateOrderAsync(UpsertOrderDto order)
    {
        var table = await tableRepository.GetByIdAsync(order.TableId);
        var menuItems = await GetOrderItemsAsync(order);

        if (!order.Items.Any())
            return ServiceResult.Fail("Cannot create an order with no items", ErrorType.Validation);

        if (table == null)
            return ServiceResult.Fail($"Table with ID {order.TableId} not found", ErrorType.NotFound);

        if (table.Status != TableStatus.occupied)
            return ServiceResult.Fail("Cannot create an order on an unoccupied table", ErrorType.Unauthorized);

        if (menuItems.Count != order.Items.Select(i => i.MenuItemId).Distinct().Count())
            return ServiceResult.Fail("One or more menu items do not exist", ErrorType.NotFound);

        var unavailableItems = menuItems
            .Where(mi => !mi.IsAvailable || mi.PlaceId != table.PlaceId)
            .Select(mi => mi.Product != null
                ? $"{mi.Product.Name} {mi.Product.Volume}"
                : "Unknown Product") 
            .ToList();

        if (unavailableItems.Any())
            return ServiceResult.Fail($"These items are currently unavailable: {string.Join(", ", unavailableItems)}", ErrorType.Validation);

        return ServiceResult.Ok();
    }

    private async Task<List<MenuItems>> GetOrderItemsAsync(UpsertOrderDto order)
    {
        var menuItemIds = order.Items.Select(i => i.MenuItemId).Distinct();
        var menuItems = await menuItemRepository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: it => menuItemIds.Contains(it.Id));

        return menuItems;
    }

    private List<UpsertOrderMenuItemDto> CombineDuplicateItems(List<UpsertOrderMenuItemDto> items)
    {
        return items
            .GroupBy(i => i.MenuItemId)
            .Select(g => new UpsertOrderMenuItemDto
            {
                MenuItemId = g.Key,
                Count = g.Sum(i => i.Count)
            })
            .ToList();
    }

    private decimal CalculateTotalPriceWithUpdates(List<ProductsPerOrder> items)
    {
        decimal totalPrice = 0m;

        foreach (var item in items)
        {
            totalPrice += item.Price * item.Count * (1 - item.Discount / 100m);
        }
        return totalPrice;
    }
}