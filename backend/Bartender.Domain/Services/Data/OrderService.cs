using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.DTO;
using Bartender.Data;
using Bartender.Domain.DTO.Place;

namespace Bartender.Domain.Services.Data;

public class OrderService(
    IOrderRepository repository,
    IRepository<Table> tableRepository,
    IRepository<MenuItem> menuItemRepository,
    IRepository<GuestSession> guestSessionRepo,
    ILogger<OrderService> logger,
    ICurrentUserContext currentUser,
    IValidationService validationService,
    INotificationService notificationService,
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
        List<ProductPerOrder> newOrderItems = await ProcessOrderItemsAsync(order);

        var calculatedTotal = CalculateTotalPrice(newOrderItems);
        if (calculatedTotal != order.TotalPrice)
            logger.LogWarning($"Mismatch between frontend and backend total price. Frontend: {order.TotalPrice}, Backend: {calculatedTotal}");

        order.TotalPrice = calculatedTotal;
        order.Status = OrderStatus.created;

        if (currentUser.IsGuest)
        {
            var guest = await guestSessionRepo.GetByKeyAsync(g => g.Token == currentUser.GetRawToken());
            if (guest == null)
                return ServiceResult.Fail("There is currently no active session found", ErrorType.NotFound);

            order.GuestSessionId = guest.Id;
        }

        var orderDetails = await GetByIdAsync(order.TableId,true);
        var messageMenuItems = "";
        orderDetails.Data?.Items.ForEach(i =>
        {
            messageMenuItems += $"{i.Count} x {i.MenuItem},";
        });
        // create order transaction - either completes both order and items creation or rolls back completely on any failure 
        var newOrder = await repository.CreateOrderWithItemsAsync(mapper.Map<Order>(order), newOrderItems);

        await notificationService.AddNotificationAsync(newOrder.Table,
            NotificationFactory.ForOrder(newOrder.Table, newOrder.Id, $"Table {newOrder.Table.Label}: {messageMenuItems}", NotificationType.OrderCreated));
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

        TableNotification notification;

        if (currentUser.IsGuest)
        {
            if (newStatus.Status == OrderStatus.payment_requested && existingOrder.Status == OrderStatus.delivered
                || newStatus.Status == OrderStatus.cancelled && existingOrder.Status == OrderStatus.created)
            {
                existingOrder.Status = newStatus.Status;
                existingOrder.PaymentType = newStatus.PaymentType ?? existingOrder.PaymentType;
                logger.LogInformation("Guest updated status of OrderId {OrderId} to {NewStatus}", id, newStatus.Status);
                notification = NotificationFactory.ForOrder
                    (existingOrder.Table, existingOrder.Id, $"Guest updated Order {existingOrder.Id} status to {existingOrder.Status}.", NotificationType.OrderStatusUpdated);
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
            existingOrder.PaymentType = newStatus.PaymentType ?? existingOrder.PaymentType;
            logger.LogInformation("Staff updated status of OrderId {OrderId} to {NewStatus}", id, newStatus.Status);
            notification = NotificationFactory.ForOrder
                    (existingOrder.Table, existingOrder.Id, $"Staff updated Order {existingOrder.Id} status to {existingOrder.Status}.", NotificationType.OrderStatusUpdated, false);
        }
        existingOrder.CreatedAt = DateTime.SpecifyKind(existingOrder.CreatedAt, DateTimeKind.Utc);
        await repository.UpdateAsync(existingOrder);

        await notificationService.AddNotificationAsync(existingOrder.Table, notification);
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
        if (existingOrder.Status == OrderStatus.closed || currentUser.IsGuest && existingOrder.Status != OrderStatus.cancelled)
        {
            logger.LogWarning($"Update failed: Attempt to modify a closed or finalized order with id {id}");
            return ServiceResult.Fail("Order cannot be changed anymore", ErrorType.Validation);
        }

        // validate order requirements
        var validationResult = await ValidateOrderAsync(order);
        if (!validationResult.Success)
            return validationResult;

        List<ProductPerOrder> newOrderItems = await ProcessOrderItemsAsync(order);

        order.TotalPrice = CalculateTotalPrice(newOrderItems);
        order.Status = existingOrder.Status;

        // create order transaction - either completes both order and items creation or rolls back completely on any failure
        await repository.UpdateOrderWithItemsAsync(existingOrder, newOrderItems);
        logger.LogInformation("Successfully updated OrderId {OrderId} with new items and total price: {TotalPrice}", id, order.TotalPrice);

        await notificationService.AddNotificationAsync(existingOrder.Table,
            NotificationFactory.ForOrder(existingOrder.Table, existingOrder.Id, 
                        $"Contents of order {existingOrder.Id} have changed. Order status: {existingOrder.Status}.", NotificationType.OrderContentUpdated));

        return ServiceResult.Ok();
    }

    /// <summary>
    /// Only orders with status 'cancelled' can be fully deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var order = await repository.GetByIdAsync(id);
        if (order == null)
            return ServiceResult.Fail($"Order with ID {id} not found", ErrorType.NotFound);

        var validUser = await validationService.VerifyUserGuestAccess(order.TableId);
        if (!validUser.Success)
            return validUser;

        if (order.Status != OrderStatus.cancelled)
            return ServiceResult.Fail($"Only cancelled orders can be removed", ErrorType.NotFound);

        await repository.DeleteAsync(order);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<ListResponse<OrderDto>>> GetAllClosedOrdersByPlaceIdAsync(int placeId,int page)
    {
        var validationResult = await ValidatePlaceAccessAsync(placeId);
        if (!validationResult.Success)
            return ServiceResult<ListResponse<OrderDto>>.Fail(validationResult.Error!, validationResult.errorType!.Value);

        var (orders,total) = await repository.GetAllByPlaceIdAsync(placeId,page);

        var dto = mapper.Map<List<OrderDto>>(orders);
        var response = new ListResponse<OrderDto> { Items = dto, Total = total };
        return ServiceResult<ListResponse<OrderDto>>.Ok(response);
    }

    public async Task<ServiceResult<List<OrderDto>>> GetAllActiveOrdersByPlaceIdAsync(int placeId, bool onlyWaitingForStaff = false)
    {
        var validationResult = await ValidatePlaceAccessAsync(placeId);
        if (!validationResult.Success)
            return ServiceResult<List<OrderDto>>.Fail(validationResult.Error!, validationResult.errorType!.Value);

        var orders = onlyWaitingForStaff ? await repository.GetPendingByPlaceIdAsync(placeId) :
                    await repository.GetActiveByPlaceIdAsync(placeId);

        var dto = mapper.Map<List<OrderDto>>(orders);
        return ServiceResult<List<OrderDto>>.Ok(dto);
    }

    public async Task<ServiceResult<ListResponse<GroupedOrderStatusDto>>> GetAllActiveOrdersByPlaceIdGroupedAsync(int placeId,int page, bool onlyWaitingForStaff = false)
    {
        var validationResult = await ValidatePlaceAccessAsync(placeId);
        if (!validationResult.Success)
            return ServiceResult<ListResponse<GroupedOrderStatusDto>>.Fail(validationResult.Error!, validationResult.errorType!.Value);

        var (groupedOrders,total) = onlyWaitingForStaff ? await repository.GetPendingByPlaceIdGroupedAsync(placeId,page) :
                    await repository.GetActiveByPlaceIdGroupedAsync(placeId, page);

        var result = groupedOrders.Select(g => new GroupedOrderStatusDto
        {
            Status = g.Key,
            Orders = mapper.Map<List<OrderDto>>(g.Value)
        }).ToList();

        var response = new ListResponse<GroupedOrderStatusDto> { Items = result, Total = total };
        return ServiceResult<ListResponse<GroupedOrderStatusDto>>.Ok(response);
    }

    public async Task<ServiceResult<List<BusinessOrdersDto>>> GetAllByBusinessIdAsync(int businessId)
    {
        var businessValidationResult = await validationService.EnsureBusinessExistsAsync(businessId);
        if (!businessValidationResult.Success)
            return ServiceResult<List<BusinessOrdersDto>>.Fail(businessValidationResult.Error!, businessValidationResult.errorType!.Value);

        if (!await validationService.VerifyUserBusinessAccess(businessId))
            return ServiceResult<List<BusinessOrdersDto>>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var orders = await repository.GetAllOrdersByBusinessIdAsync(businessId);

        var result = orders.Select(g => new BusinessOrdersDto
        {
            Place = mapper.Map<PlaceDto>(g.Key),
            Orders = mapper.Map<List<OrderDto>>(g.Value)
        }).ToList();

        return ServiceResult<List<BusinessOrdersDto>>.Ok(result);
    }

    //TODO: troubleshoot validation...
    public async Task<ServiceResult<OrderDto?>> GetByIdAsync(int id, bool skipValidation)
    {
        var order = await repository.getOrderById(id); //TODO: should fetching be done after validation?

        if (order == null)
            return ServiceResult<OrderDto?>.Fail($"Order with id {id} not found", ErrorType.NotFound);

        if (!skipValidation)
        {
            var verifyUser = await validationService.VerifyUserGuestAccess(order.TableId);
            if (!verifyUser.Success)
                return ServiceResult<OrderDto?>.Fail(verifyUser.Error!, verifyUser.errorType!.Value);
        }
        var dto = mapper.Map<OrderDto>(order);

        return ServiceResult<OrderDto?>.Ok(dto);
    }

    public async Task<ServiceResult<List<OrderDto>>> GetCurrentOrdersByTableLabelAsync(string tableLabel) //staff only?
    {
        var orders = await repository.GetCurrentOrdersByTableLabelAsync(tableLabel); //TODO: should fetching be done after validation?
        if(!orders.Any())
            return ServiceResult<List<OrderDto>>.Ok(new List<OrderDto>());
        var verifyUser = await validationService.VerifyUserGuestAccess(orders[0].Table.Id);
        if (!verifyUser.Success)
            return ServiceResult<List<OrderDto>>.Fail(verifyUser.Error!, verifyUser.errorType!.Value);

        var dtos = mapper.Map<List<OrderDto>>(orders);
        return ServiceResult<List<OrderDto>>.Ok(dtos);
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

    private async Task<List<MenuItem>> GetOrderItemsAsync(UpsertOrderDto order)
    {
        var menuItemIds = order.Items.Select(i => i.MenuItemId).Distinct();
        var menuItems = await menuItemRepository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: it => menuItemIds.Contains(it.Id));

        return menuItems;
    }

    private async Task<List<ProductPerOrder>> ProcessOrderItemsAsync(UpsertOrderDto order)
    {
        var menuItems = await GetOrderItemsAsync(order);
        var combinedItems = order.Items
            .GroupBy(i => i.MenuItemId)
            .Select(g => new ProductPerOrder
            {
                MenuItemId = g.Key,
                Count = g.Sum(i => i.Count),
                Price = menuItems.First(mi => mi.Id == g.Key).Price,
                Discount = g.Max(i => i.Discount ?? 0) // TODO: Update discount logic to use menuItem discount when available
            })
            .ToList();

        return combinedItems;
    }


    private decimal CalculateTotalPrice(List<ProductPerOrder> items)
    {
        return items.Sum(item => item.Price * item.Count * (1 - item.Discount / 100m));
    }

    private async Task<ServiceResult> ValidatePlaceAccessAsync(int placeId)
    {
        var placeValidationResult = await validationService.EnsurePlaceExistsAsync(placeId);
        if (!placeValidationResult.Success)
            return placeValidationResult;

        if (!await validationService.VerifyUserPlaceAccess(placeId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        return ServiceResult.Ok();
    }
}