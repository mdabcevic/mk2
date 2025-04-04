using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO.Orders;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class OrderService(
    IOrderRepository repository,
    IRepository<Tables> tableRepository,
    IRepository<MenuItems> menuItemRepository,
    IRepository<GuestSession> guestSessionRepo,
    ILogger<OrderService> logger,
    ICurrentUserContext currentUser,
    IValidationService validationService,
    IMapper mapper
    ) : IOrderService
{
    public async Task<ServiceResult> AddAsync(UpsertOrderDto order)
    {
        var validUser = await validationService.VerifyUserGuestAccess(order.TableId);
        if (!validUser.Success)
            return validUser;

        // validate order requirements
        var validationResult = await ValidateOrderAsync(order);
        if (!validationResult.Success)
            return validationResult;

        // combine duplicate items (same MenuItemId) by summing their quantities and add price to each item
        List<ProductsPerOrder> newOrderItems = await CombineDuplicateItemsAndAddPrices(order);

        var calculatedTotal = CalculateTotalPrice(newOrderItems);
        if (calculatedTotal != order.TotalPrice)
            logger.LogWarning($"Mismatch between frontend and backend total price. Frontend: {order.TotalPrice}, Backend: {calculatedTotal}");

        order.TotalPrice = calculatedTotal;
        order.Status = OrderStatus.created;

        var guest = await guestSessionRepo.GetByKeyAsync(g => g.Token == currentUser.GetRawToken());
        if (guest == null)
            return ServiceResult.Fail("There is currently no active session found", ErrorType.NotFound);

        order.GuestSessionId = guest.Id;

        // create order transaction - either completes both order and items creation or rolls back completely on any failure 
        await repository.CreateOrderWithItemsAsync(mapper.Map<Orders>(order), newOrderItems);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateStatusAsync(int id, UpdateOrderStatusDto newStatus)
    {
        var existingOrder = await repository.GetByIdAsync(id, true);

        if (existingOrder == null)
            return ServiceResult.Fail($"Order with id {id} not found", ErrorType.NotFound);  

        var validUser = await validationService.VerifyUserGuestAccess(existingOrder.TableId);
        if (!validUser.Success)
            return validUser;

        if (currentUser.IsGuest)
        {
            if ((newStatus.Status == OrderStatus.payment_requested && existingOrder.Status == OrderStatus.delivered)
                || (newStatus.Status == OrderStatus.cancelled && existingOrder.Status == OrderStatus.created))
            {
                existingOrder.Status = newStatus.Status;
                existingOrder.PaymentType = newStatus.PaymentType ?? PaymentType.cash;
                logger.LogInformation("Guest updated status of OrderId {OrderId} to {NewStatus}", id, newStatus.Status);
            }
            else
            {
                logger.LogWarning("Guest cannot update OrderId {OrderId} from {CurrentStatus} to {NewStatus}", id, existingOrder.Status, newStatus.Status);
                return ServiceResult.Fail($"Order status cannot be changed to {newStatus.Status}", ErrorType.NotFound);
            }
        }
        else
        {
            existingOrder.Status = newStatus.Status;
            existingOrder.PaymentType = newStatus.PaymentType ?? PaymentType.cash;
            logger.LogInformation("Staff updated status of OrderId {OrderId} to {NewStatus}", id, newStatus.Status);
        }
        await repository.UpdateAsync(existingOrder);
        return ServiceResult.Ok();      
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertOrderDto order)
    {
        var existingOrder = await repository.GetByIdAsync(id, true);
        if (existingOrder == null)
        {
            return ServiceResult.Fail($"Order with id {id} not found", ErrorType.NotFound);
        }

        var validUser = await validationService.VerifyUserGuestAccess(order.TableId);
        if (!validUser.Success)
            return validUser;

        // if the order is closed or the guest is trying to modify an already approved order, return an error.
        if (existingOrder.Status == OrderStatus.closed || (currentUser.IsGuest && existingOrder.Status != OrderStatus.created))
        {
            logger.LogWarning($"Update failed: Attempt to modify a closed or finalized order with id {id}");
            return ServiceResult.Fail("Order cannot be changed anymore", ErrorType.Validation);
        }

        // validate order requirements
        var validationResult = await ValidateOrderAsync(order);
        if (!validationResult.Success)
            return validationResult;

        List<ProductsPerOrder> newOrderItems = await CombineDuplicateItemsAndAddPrices(order);

        order.TotalPrice = CalculateTotalPrice(newOrderItems);
        order.Status = existingOrder.Status;

        // create order transaction - either completes both order and items creation or rolls back completely on any failure
        await repository.UpdateOrderWithItemsAsync(existingOrder, newOrderItems);
        logger.LogInformation("Successfully updated OrderId {OrderId} with new items and total price: {TotalPrice}", id, order.TotalPrice);
        return ServiceResult.Ok();
    }

    //TODO - soft delete
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var order = await repository.GetByIdAsync(id);
        if (order == null)
        {
            return ServiceResult.Fail($"Order with ID {id} not found", ErrorType.NotFound);
        }
        await repository.DeleteAsync(order);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<OrderDto>>> GetAllClosedOrdersByPlaceIdAsync(int placeId)
    {
        var placeValidationResult = await validationService.EnsurePlaceExistsAsync(placeId);
        if (!placeValidationResult.Success)
            return ServiceResult<List<OrderDto>>.Fail(placeValidationResult.Error!, placeValidationResult.errorType!.Value);

        if (!await validationService.VerifyUserPlaceAccess(placeId))
            return ServiceResult<List<OrderDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var orders = await repository.GetAllByPlaceIdAsync(placeId);

        var dto = mapper.Map<List<OrderDto>>(orders);
        return ServiceResult<List<OrderDto>>.Ok(dto);
    }

    public async Task<ServiceResult<List<OrderDto>>> GetAllActiveOrdersByPlaceIdAsync(int placeId, bool onlyWaitingForStaff = false)
    {
        var placeValidationResult = await validationService.EnsurePlaceExistsAsync(placeId);
        if (!placeValidationResult.Success)
            return ServiceResult<List<OrderDto>>.Fail(placeValidationResult.Error!, placeValidationResult.errorType!.Value);

        if (!await validationService.VerifyUserPlaceAccess(placeId))
            return ServiceResult<List<OrderDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var orders = onlyWaitingForStaff ? await repository.GetPendingByPlaceIdAsync(placeId) :
                    await repository.GetActiveByPlaceIdAsync(placeId);

        var dto = mapper.Map<List<OrderDto>>(orders);
        return ServiceResult<List<OrderDto>>.Ok(dto);
    }

    public async Task<ServiceResult<List<OrderBaseDto>>> GetAllByBusinessIdAsync(int businessId)
    {
        var businessValidationResult = await validationService.EnsureBusinessExistsAsync(businessId);
        if (!businessValidationResult.Success)
            return ServiceResult<List<OrderBaseDto>>.Fail(businessValidationResult.Error!, businessValidationResult.errorType!.Value);

        if (!await validationService.VerifyUserBusinessAccess(businessId))
            return ServiceResult<List<OrderBaseDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var orders = await repository.GetAllOrdersByBusinessIdAsync(businessId);

        var dto = mapper.Map<List<OrderBaseDto>>(orders);
        return ServiceResult<List<OrderBaseDto>>.Ok(dto);
    }

    public async Task<ServiceResult<OrderDto?>> GetByIdAsync(int id)
    {
        var order = await repository.GetByIdAsync(id, true);

        if (order == null)
            return ServiceResult<OrderDto?>.Fail($"Order with id {id} not found", ErrorType.NotFound);

        var verifyUser = await validationService.VerifyUserGuestAccess(order.TableId);
        if (!verifyUser.Success)
            return ServiceResult<OrderDto?>.Fail(verifyUser.Error!, verifyUser.errorType!.Value);

        var dto = mapper.Map<OrderDto>(order);

        return ServiceResult<OrderDto?>.Ok(dto);
    }

    public async Task<ServiceResult<List<OrderDto>>> GetActiveTableOrdersForUserAsync(bool userSpecific = true)
    {
        var guest = await guestSessionRepo.GetByKeyAsync(g => g.Token == currentUser.GetRawToken());
        if (guest == null)
            return ServiceResult<List<OrderDto>>.Fail("There is currently no active session found", ErrorType.NotFound);

        var order = userSpecific ? await repository.GetActiveOrdersByGuestIdAsync(guest.Id) :      
            await repository.GetActiveOrdersByTableIdAsync(guest.TableId);

        var dto = mapper.Map<List<OrderDto>>(order);

        return ServiceResult< List<OrderDto>>.Ok(dto);
    }

    private async Task<ServiceResult> ValidateOrderAsync(UpsertOrderDto order)
    {
        var table = await tableRepository.GetByIdAsync(order.TableId);
        var menuItems = await GetOrderItemsAsync(order);

        if (!order.Items.Any())
            return ServiceResult.Fail("Cannot create an order with no items", ErrorType.Validation);

        if (table == null)
            return ServiceResult.Fail($"Table not found", ErrorType.NotFound);

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

        foreach (var item in items)
            item.Price = menuItems.FirstOrDefault(mi => mi.Id == item.MenuItemId)?.Price ?? 0m;

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
}