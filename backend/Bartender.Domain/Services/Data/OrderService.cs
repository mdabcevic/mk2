using AutoMapper;
using Bartender.Data.Models;
using Bartender.Data.Enums;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.DTO;
using Bartender.Data;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;

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
    public async Task AddAsync(UpsertOrderDto order)
    {
        var verifyAccess = await validationService.VerifyUserGuestAccess(order.TableId);
        if (!verifyAccess)
            throw new TableAccessDeniedException(tableId: order.TableId);

        // validate order requirements
        await ValidateOrderAsync(order);

        // combine duplicate items (same MenuItemId) by summing their quantities and add price to each item
        List<ProductPerOrder> newOrderItems = await ProcessOrderItemsAsync(order);

        var calculatedTotal = CalculateTotalPrice(newOrderItems);
        if (calculatedTotal != order.TotalPrice)
            logger.LogWarning("Mismatch between frontend and backend total price. Frontend: {FrontendTotal}, Backend: {BackendTotal}", order.TotalPrice, calculatedTotal);


        order.TotalPrice = calculatedTotal;
        order.Status = OrderStatus.created;

        if (currentUser.IsGuest)
        {
            var guest = await guestSessionRepo.GetByKeyAsync(g => g.Token == currentUser.GetRawToken());

            if (guest == null)
                throw new NoActiveSessionFoundException();

            order.GuestSessionId = guest.Id;
        }

        
        // create order transaction - either completes both order and items creation or rolls back completely on any failure 
        var newOrder = await repository.CreateOrderWithItemsAsync(mapper.Map<Order>(order), newOrderItems);
        var orderDetails = await GetByIdAsync(newOrder!.Id, true);
        var messageMenuItems = "";
        orderDetails?.Items.ForEach(i =>
        {
            messageMenuItems += $"{i.Count} x {i.MenuItem},";
        });
        await notificationService.AddNotificationAsync(newOrder.Table,
            NotificationFactory.ForOrder(newOrder!.Table, newOrder.Id, $"Table {newOrder!.Table!.Label}: {messageMenuItems}", NotificationType.OrderCreated));
    }

    public async Task UpdateStatusAsync(int id, UpdateOrderStatusDto newStatus)
    {
        var existingOrder = await repository.GetByIdAsync(id, true);

        if (existingOrder == null)
            throw new OrderNotFoundException(id); 

        var verifyAccess = await validationService.VerifyUserGuestAccess(existingOrder.TableId);
        if (!verifyAccess)
            throw new UnauthorizedOrderAccessException(id);

        TableNotification notification;

        if (currentUser.IsGuest)
        {
            if ((newStatus.Status == OrderStatus.payment_requested && 
                (existingOrder.Status == OrderStatus.delivered || existingOrder.Status == OrderStatus.payment_requested))
                || (newStatus.Status == OrderStatus.cancelled && existingOrder.Status == OrderStatus.created))
            {
                existingOrder.Status = newStatus.Status;
                existingOrder.PaymentType = newStatus.PaymentType ?? existingOrder.PaymentType;
                logger.LogInformation("Guest updated status of OrderId {OrderId} to {NewStatus}", id, newStatus.Status);
                notification = NotificationFactory.ForOrder
                    (existingOrder.Table, existingOrder.Id, $"Guest updated Order {existingOrder.Id} status to {existingOrder.Status}.", NotificationType.OrderStatusUpdated,true);
            }
            else
            {
                throw new AuthorizationException($"Order status cannot be changed to {newStatus.Status}")
                    .WithLogMessage("$\"Guest cannot update OrderId {id} from {existingOrder.Status} to {newStatus.Status}\"");
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
        existingOrder.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        await repository.UpdateAsync(existingOrder);

        await notificationService.AddNotificationAsync(existingOrder.Table, notification);   
    }

    public async Task UpdateAsync(int id, UpsertOrderDto order)
    {
        var existingOrder = await repository.GetByIdAsync(id, true);
        if (existingOrder == null)
        {
            throw new OrderNotFoundException(id);
        }

        var verifyAccess = await validationService.VerifyUserGuestAccess(existingOrder.TableId);
        if (!verifyAccess)
            throw new UnauthorizedOrderAccessException(id);

        var user = await currentUser.GetCurrentUserAsync();
        
        var hasHigherAccess = user?.Role == EmployeeRole.owner || user?.Role == EmployeeRole.admin || user?.Role == EmployeeRole.manager;

        // if the order is closed or the guest is trying to modify an already approved order, return an error.
        if ((existingOrder.Status == OrderStatus.closed && !hasHigherAccess) || 
            (currentUser.IsGuest && existingOrder.Status != OrderStatus.cancelled))
        {
            throw new AuthorizationException("Access to change order denied");
        }

        // validate order requirements
        await ValidateOrderAsync(order);

        List<ProductPerOrder> newOrderItems = await ProcessOrderItemsAsync(order);

        order.TotalPrice = CalculateTotalPrice(newOrderItems);
        order.Status = existingOrder.Status;

        // create order transaction - either completes both order and items creation or rolls back completely on any failure
        await repository.UpdateOrderWithItemsAsync(existingOrder, newOrderItems);
        logger.LogInformation("Successfully updated OrderId {OrderId} with new items and total price: {TotalPrice}", id, order.TotalPrice);

        await notificationService.AddNotificationAsync(existingOrder.Table,
            NotificationFactory.ForOrder(existingOrder.Table, existingOrder.Id, 
                        $"Contents of order {existingOrder.Id} have changed. Order status: {existingOrder.Status}.", NotificationType.OrderContentUpdated));
    }

    /// <summary>
    /// Only orders with status 'cancelled' can be fully deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task DeleteAsync(int id)
    {
        var order = await repository.GetByIdAsync(id) ?? throw new OrderNotFoundException(id);
        var verifyAccess = await validationService.VerifyUserGuestAccess(order.TableId);
        if (!verifyAccess)
            throw new UnauthorizedOrderAccessException(id);

        if (order.Status != OrderStatus.cancelled)
            throw new AppValidationException("Only cancelled orders can be removed");

        await repository.DeleteAsync(order);
    }

    public async Task<ListResponse<OrderDto>> GetAllClosedOrdersByPlaceIdAsync(int placeId,int page,int size)
    {
        await ValidatePlaceAccessAsync(placeId);

        var (orders,total) = await repository.GetAllByPlaceIdAsync(placeId,page,size);

        var dto = mapper.Map<List<OrderDto>>(orders);
        var response = new ListResponse<OrderDto> { Items = dto, Total = total };
        return response;
    }

    public async Task<List<OrderDto>> GetAllActiveOrdersByPlaceIdAsync(int placeId, bool onlyWaitingForStaff = false)
    {
        await ValidatePlaceAccessAsync(placeId);

        var orders = onlyWaitingForStaff ? await repository.GetPendingByPlaceIdAsync(placeId) :
                    await repository.GetActiveByPlaceIdAsync(placeId);

        var dto = mapper.Map<List<OrderDto>>(orders);
        return dto;
    }

    public async Task<ListResponse<GroupedOrderStatusDto>> GetAllActiveOrdersByPlaceIdGroupedAsync(int placeId,int page, int size, bool onlyWaitingForStaff = false)
    {
        await ValidatePlaceAccessAsync(placeId);

        var (groupedOrders,total) = onlyWaitingForStaff ? await repository.GetPendingByPlaceIdGroupedAsync(placeId,page,size) :
                    await repository.GetActiveByPlaceIdGroupedAsync(placeId, page, size);

        var result = groupedOrders.Select(g => new GroupedOrderStatusDto
        {
            Status = g.Key,
            Orders = mapper.Map<List<OrderDto>>(g.Value)
        }).ToList();

        var response = new ListResponse<GroupedOrderStatusDto> { Items = result, Total = total };
        return response;
    }

    public async Task<List<BusinessOrdersDto>> GetAllByBusinessIdAsync(int businessId)
    {
        await validationService.EnsureBusinessExistsAsync(businessId);

        if (!await validationService.VerifyUserBusinessAccess(businessId))
            throw new UnauthorizedBusinessAccessException();

        var orders = await repository.GetAllOrdersByBusinessIdAsync(businessId);

        var result = orders.Select(g => new BusinessOrdersDto
        {
            Place = mapper.Map<PlaceDto>(g.Key),
            Orders = mapper.Map<List<OrderDto>>(g.Value)
        }).ToList();

        return result;
    }

    public async Task<OrderDto?> GetByIdAsync(int id, bool skipValidation)
    {
        var order = await repository.getOrderById(id) ?? throw new OrderNotFoundException(id); // should fetching be done after validation?

        if (!skipValidation)
        {
            var verifyAccess = await validationService.VerifyUserGuestAccess(order.TableId);
            if (!verifyAccess)
                throw new UnauthorizedOrderAccessException(order.Id);
        }
        var dto = mapper.Map<OrderDto>(order);

        return dto;
    }

    public async Task<List<OrderDto>> GetCurrentOrdersByTableLabelAsync(string tableLabel)
    {
        var orders = await repository.GetCurrentOrdersByTableLabelAsync(tableLabel); // should fetching be done after validation?
        if(orders == null || orders.Count == 0)
            return [];

        var verifyAccess = await validationService.VerifyUserGuestAccess(orders[0]!.Table!.Id);
        if (!verifyAccess)
        {
            throw new TableAccessDeniedException(tableLabel: tableLabel);
        }

        var dtos = mapper.Map<List<OrderDto>>(orders);
        return dtos;
    }

    public async Task<List<OrderDto>> GetActiveTableOrdersForUserAsync(bool userSpecific = true)
    {
        var guest = await guestSessionRepo.GetByKeyAsync(g => g.Token == currentUser.GetRawToken()) ?? throw new NoActiveSessionFoundException();
        var order = userSpecific ? await repository.GetActiveOrdersByGuestIdAsync(guest.Id) :      
            await repository.GetActiveOrdersByTableIdAsync(guest.TableId);

        var dto = mapper.Map<List<OrderDto>>(order);

        return dto;
    }

    private async Task ValidateOrderAsync(UpsertOrderDto order)
    {
        var table = await tableRepository.GetByIdAsync(order.TableId);
        var menuItems = await GetOrderItemsAsync(order);

        if (order.Items.Count == 0)
            throw new AppValidationException("Cannot create an order with no items");

        if (table == null)
            throw new TableNotFoundException(order.TableId);

        if (table.Status != TableStatus.occupied)
            throw new AuthorizationException("Cannot create an order on an unoccupied table");

        var missingItems = order.Items
            .Where(oi => !menuItems.Any(mi => mi.Id == oi.MenuItemId))
            .Select(oi => oi.MenuItemId)
            .ToList();

        if (missingItems.Count != 0)
            throw new NotFoundException($"One or more menu items do not exist or are not available: {string.Join(", ", missingItems)}");

        var unavailableItems = menuItems
            .Where(mi => !mi.IsAvailable || mi.PlaceId != table.PlaceId)
            .Select(mi => mi.Product != null
                ? $"{mi.Product.Name} {mi.Product.Volume}"
                : "Unknown Product") 
            .ToList();

        if (unavailableItems.Count != 0)
            throw new AppValidationException($"These items are currently unavailable: {string.Join(", ", unavailableItems)}");
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

    private static decimal CalculateTotalPrice(List<ProductPerOrder> items)
    {
        return items.Sum(item => item.Price * item.Count * (1 - item.Discount / 100m));
    }

    private async Task ValidatePlaceAccessAsync(int placeId)
    {
        await validationService.EnsurePlaceExistsAsync(placeId);

        if (!await validationService.VerifyUserPlaceAccess(placeId))
            throw new UnauthorizedPlaceAccessException();
    }
}