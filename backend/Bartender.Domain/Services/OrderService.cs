using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO.Orders;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Bartender.Domain.Services;

public class OrderService(
    IRepository<Orders> repository,
    IRepository<Tables> tableRepository,
    IRepository<MenuItems> menuItemRepository,
    IRepository<ProductsPerOrder> orderProductsRepo,
    IRepository<Places> placeRepository,
    IRepository<Businesses> businessRepository,
    ILogger<OrderService> logger,
    ICurrentUserContext currentUser,
    ITableSessionService tableSessionService,
    IMapper mapper
    ) : IOrderService
{
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again later.";
    public async Task<ServiceResult> AddAsync(UpsertOrderDto order)
    {
        try
        {
            var validUser = await VerifyUserAccess(order.TableId);
            if (!validUser.Success)
                return validUser;

            // validate order requirements
            var validationResult = await ValidateOrderAsync(order);
            if (!validationResult.Success)
                return validationResult;

            // combine duplicate items (same MenuItemId) by summing their quantities and add price to each item
            List<ProductsPerOrder> newOrderItems = await CombineDuplicateItemsAndAddPrices(order);

            // calculate total order price
            order.TotalPrice = CalculateTotalPrice(newOrderItems);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating an order.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> UpdateStatusAsync(int id, UpdateOrderStatusDto newStatus)
    {
        try
        {
            var existingOrder = await repository.GetByIdAsync(id, true);

            if (existingOrder == null)
            {
                return ServiceResult.Fail($"Order with id {id} not found", ErrorType.NotFound);
            }

            var validUser = await VerifyUserAccess(existingOrder.TableId);
            if (!validUser.Success)
                return validUser;

            if (currentUser.IsGuest)
            {
                if (newStatus.Status == OrderStatus.payment_requested && (existingOrder.Status == OrderStatus.delivered || existingOrder.Status == OrderStatus.approved))
                {
                    existingOrder.Status = newStatus.Status;
                    existingOrder.PaymentType = (PaymentType)(newStatus.PaymentType != null ? newStatus.PaymentType : PaymentType.cash);
                }
                else
                    return ServiceResult.Fail($"Order status cannot be changed to {newStatus.Status}", ErrorType.NotFound);
            }
            else
            {
                existingOrder.Status = newStatus.Status;

                if (existingOrder.Status == OrderStatus.closed)
                {
                    // TODO
                }
            }
            await repository.UpdateAsync(existingOrder);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating order status.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }

    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertOrderDto order)
    {
        try
        {
            var existingOrder = await repository.GetByIdAsync(id, true);
            if (existingOrder == null)
            {
                return ServiceResult.Fail($"Order with id {id} not found", ErrorType.NotFound);
            }

            var validUser = await VerifyUserAccess(order.TableId);
            if (!validUser.Success)
                return validUser;

            // if the order isn't yet approved it can still be changed
            if (currentUser.IsGuest && existingOrder.Status != OrderStatus.created)
                return ServiceResult.Fail("Order cannot be changed anymore", ErrorType.Validation);

            // validate order requirements
            var validationResult = await ValidateOrderAsync(order);
            if (!validationResult.Success)
                return validationResult;

            List<ProductsPerOrder> newOrderItems = await CombineDuplicateItemsAndAddPrices(order);

            order.TotalPrice = CalculateTotalPrice(newOrderItems);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while updating an order.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
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
            logger.LogError(ex, "An unexpected error occurred while deleting an order.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<OrderDto>>> GetAllByPlaceIdAsync(int placeId, bool onlyActive = false, bool pending = false)
    {
        try
        {
            var place = await placeRepository.ExistsAsync(p => p.Id == placeId);
            if (!place)
                return ServiceResult<List<OrderDto>>.Fail($"Place with id {placeId} not found", ErrorType.NotFound);

            if (!await VerifyUserPlaceAccess(null, placeId))
                return ServiceResult<List<OrderDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

            Expression<Func<Orders, bool>>? filter = null;
           
            if (onlyActive)
                filter = o => o.Table.PlaceId == placeId && (o.Status != OrderStatus.closed || o.Status == OrderStatus.cancelled);
            
            else if (pending)
                filter = o => o.Table.PlaceId == placeId &&
                                (o.Status == OrderStatus.created || o.Status == OrderStatus.payment_requested);
            else
                filter = o => o.Table.PlaceId == placeId;


            var orders = await repository.GetFilteredAsync(
                    includeNavigations: true,
                    filterBy: filter,
                    orderByDescending: true,
                    orderBy: o => o.CreatedAt);

            var dto = mapper.Map<List<OrderDto>>(orders);
            return ServiceResult<List<OrderDto>>.Ok(dto);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching orders.");
            return ServiceResult<List<OrderDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<OrderBaseDto>>> GetAllByBusinessIdAsync(int businessId)
    {
        try
        {
            var business = await businessRepository.ExistsAsync(b => b.Id == businessId);
            if (!business)
                return ServiceResult<List<OrderBaseDto>>.Fail($"Business with id {businessId} not found", ErrorType.NotFound);

            if (!await VerifyUserBusinessAccess(businessId))
            {
                return ServiceResult<List<OrderBaseDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);
            }

            var orders = await repository.GetFilteredAsync(
                    includeNavigations: true,
                    filterBy: o => o.Table.Place.BusinessId == businessId,
                    orderByDescending: true,
                    orderBy: o => o.CreatedAt);

            var dto = mapper.Map<List<OrderBaseDto>>(orders);
            return ServiceResult<List<OrderBaseDto>>.Ok(dto);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching orders.");
            return ServiceResult<List<OrderBaseDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<OrderDto?>> GetByIdAsync(int id)
    {
        try
        {
            var order = await repository.GetByIdAsync(id, true);

            if (order == null)
                return ServiceResult<OrderDto?>.Fail($"Order with id {id} not found", ErrorType.NotFound);

            var verifyUser = await VerifyUserAccess(order.TableId);
            if (!verifyUser.Success)
                return ServiceResult<OrderDto?>.Fail(verifyUser.Error!, verifyUser.errorType!.Value);

            var dto = mapper.Map<OrderDto>(order);

            return ServiceResult<OrderDto?>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the order.");
            return ServiceResult<OrderDto?>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
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

        var missingItems = order.Items
            .Where(oi => !menuItems.Any(mi => mi.Id == oi.MenuItemId))
            .Select(oi => oi.MenuItemId)
            .ToList();

        if (missingItems.Any())
            return ServiceResult.Fail($"One or more menu items do not exist or are not available: {string.Join(", ", missingItems)}",ErrorType.NotFound);

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

    private async Task<List<ProductsPerOrder>> CombineDuplicateItemsAndAddPrices(UpsertOrderDto order)
    {
        var combinedItemsDto = order.Items
            .GroupBy(i => i.MenuItemId)
            .Select(g => new UpsertOrderMenuItemDto
            {
                MenuItemId = g.Key,
                Count = g.Sum(i => i.Count)
            })
            .ToList();

        var items = mapper.Map<List<ProductsPerOrder>>(combinedItemsDto);

        var menuItems = await GetOrderItemsAsync(order);
        items.Select(it =>it.Price = menuItems.FirstOrDefault(mi => mi.Id == it.MenuItemId).Price);

        return items;
    }

    private decimal CalculateTotalPrice(List<ProductsPerOrder> items)
    {
        decimal totalPrice = 0m;

        foreach (var item in items)
        {
            totalPrice += item.Price * item.Count * (1 - item.Discount / 100m);
        }
        return totalPrice;
    }

    private async Task<ServiceResult> VerifyUserAccess(int orderTableId)
    {
        if (currentUser.IsGuest && !await tableSessionService.IsSameTokenAsActiveAsync(orderTableId, currentUser.GetRawToken()))
        {
            logger.LogWarning("Unauthorized order attempt by guest for Table {TableId}", orderTableId);
            return ServiceResult.Fail("You don't have access to manage orders for this table", ErrorType.Unauthorized);
        }
        else if (!currentUser.IsGuest)
        {
            var user = await currentUser.GetCurrentUserAsync();
            var table = await tableRepository.GetByIdAsync(orderTableId);
       
            if (!await VerifyUserPlaceAccess(user, table.PlaceId))
            {
                logger.LogWarning($"Unauthorized order attempt by user {user.Username}({user.Id}) for Table {orderTableId}");
                return ServiceResult.Fail("You don't have access to manage orders for this table", ErrorType.Unauthorized);
            }
        }
        return ServiceResult.Ok();
    }

    private async Task<bool> VerifyUserPlaceAccess(Staff? user, int targetPlaceId)
    {
        if (user == null) {
            user = await currentUser.GetCurrentUserAsync();
        }
        if (user!.Role == EmployeeRole.admin)
            return true;

        var targetPlace = await placeRepository.GetByIdAsync(targetPlaceId);

        if (targetPlace != null && targetPlace.BusinessId == user!.Place!.BusinessId && user!.Role == EmployeeRole.owner)
            return true;

        return targetPlaceId == user.PlaceId;
    }

    private async Task<bool> VerifyUserBusinessAccess(int businessId)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user!.Role == EmployeeRole.admin)
            return true;

        if (businessId == user!.Place!.BusinessId)
            return true;

        return false;
    }
}