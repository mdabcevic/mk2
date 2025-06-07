using AutoMapper;
using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace BartenderTests.UnitTests;

[TestFixture]
public class OrderServiceMutationTests
{
    private IOrderRepository _orderRepo;
    private IRepository<Table> _tableRepo;
    private IRepository<MenuItem> _menuItemRepo;
    private IRepository<GuestSession> _guestSessionRepo;
    private ILogger<OrderService> _logger;
    private ICurrentUserContext _currentUser;
    private IValidationService _validationService;
    private INotificationService _notificationService;
    private IMapper _mapper;

    private OrderService _service;

    [SetUp]
    public void SetUp()
    {
        _orderRepo = Substitute.For<IOrderRepository>();
        _tableRepo = Substitute.For<IRepository<Table>>();
        _menuItemRepo = Substitute.For<IRepository<MenuItem>>();
        _guestSessionRepo = Substitute.For<IRepository<GuestSession>>();
        _logger = Substitute.For<ILogger<OrderService>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _validationService = Substitute.For<IValidationService>();
        _notificationService = Substitute.For<INotificationService>();
        _mapper = Substitute.For<IMapper>();

        _service = new OrderService(
            _orderRepo,
            _tableRepo,
            _menuItemRepo,
            _guestSessionRepo,
            _logger,
            _currentUser,
            _validationService,
            _notificationService,
            _mapper);
    }

    [Test]
    public void AddAsync_ShouldThrow_WhenGuestAccessValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto();
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<TableAccessDeniedException>(() => _service.AddAsync(dto));

        Assert.That(ex!.Message, Does.Contain($"Access"));
        Assert.That(ex!.Message, Does.Contain($"denied"));
    }

    [Test]
    public void AddAsync_ShouldThrow_WhenOrderValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto();

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true);

        // Simulate a table in invalid state (e.g., not occupied)
        var table = TestDataFactory.CreateValidTable(dto.TableId, status: TableStatus.empty);
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);

        _menuItemRepo.GetFilteredAsync(true, Arg.Any<Expression<Func<MenuItem, bool>>>())
        .Returns([]);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.AddAsync(dto));
        Assert.That(ex!.Message, Does.Contain("unoccupied table"));
    }

    [Test]
    public void AddAsync_ShouldThrow_WhenGuestSessionNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 1, menuItemId: 1, count: 1, totalPrice: 5m);

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true);
        _tableRepo.GetByIdAsync(dto.TableId)
            .Returns(TestDataFactory.CreateValidTable(dto.TableId));

        _menuItemRepo.GetFilteredAsync(
            true,
            Arg.Any<Expression<Func<MenuItem, bool>>>())
        .Returns(TestDataFactory.CreateSampleMenuItems());

        _currentUser.IsGuest.Returns(true);
        _currentUser.GetRawToken().Returns("invalid-token");
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>())
            .Returns((GuestSession?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NoActiveSessionFoundException>(() => _service.AddAsync(dto));

        Assert.That(ex!.Message, Is.EqualTo("There is currently no active session found"));
    }

    [Test]
    public async Task AddAsync_ShouldSucceed_WhenGuestOrderIsValid()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var guest = TestDataFactory.CreateValidGuestSession(table);
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: table.Id, menuItemId: 1, count: 1, totalPrice: 5m);

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true);
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            true,
            Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns(TestDataFactory.CreateSampleMenuItems());

        _currentUser.IsGuest.Returns(true);
        _currentUser.GetRawToken().Returns(guest.Token);
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(guest);

        var newOrder = new Order { Id = 1, Table = table };

        _mapper.Map<Order>(dto).Returns(new Order { Table = table });
        _orderRepo.CreateOrderWithItemsAsync(Arg.Any<Order>(), Arg.Any<List<ProductPerOrder>>())
            .Returns(newOrder);

        _orderRepo.getOrderById(newOrder.Id).Returns(newOrder);

        _mapper.Map<OrderDto>(Arg.Any<Order>())
            .Returns(TestDataFactory.CreateValidOrderDto(id: 1, tableLabel: table.Label));

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _service.AddAsync(dto));
        await _orderRepo.Received(1).CreateOrderWithItemsAsync(Arg.Any<Order>(), Arg.Any<List<ProductPerOrder>>());
        await _notificationService.Received(1).AddNotificationAsync(Arg.Any<Table>(), Arg.Any<TableNotification>());
    }

    [Test]
    public async Task AddAsync_ShouldSucceed_WhenStaffOrderIsValid()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            Items = [new() { MenuItemId = 1, Count = 1 }],
            TotalPrice = 5m
        };

        var newOrder = new Order { Id = 1, Table = table };
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true);
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            true,
            Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns(TestDataFactory.CreateSampleMenuItems());

        _currentUser.IsGuest.Returns(false);

        _mapper.Map<Order>(dto).Returns(new Order { Table = table });
        _orderRepo.CreateOrderWithItemsAsync(Arg.Any<Order>(), Arg.Any<List<ProductPerOrder>>())
            .Returns(newOrder);

        _orderRepo.getOrderById(newOrder.Id).Returns(newOrder);

        _mapper.Map<OrderDto>(Arg.Any<Order>())
            .Returns(TestDataFactory.CreateValidOrderDto(id: 1, tableLabel: table.Label));

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _service.AddAsync(dto));
        await _orderRepo.Received(1).CreateOrderWithItemsAsync(Arg.Any<Order>(), Arg.Any<List<ProductPerOrder>>());
        await _notificationService.Received(1).AddNotificationAsync(Arg.Any<Table>(), Arg.Any<TableNotification>());
    }

    [Test]
    public void UpdateStatusAsync_ShouldThrow_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = 99;
        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(orderId, true).Returns((Order?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<OrderNotFoundException>(() =>
            _service.UpdateStatusAsync(orderId, updateDto));

        Assert.That(ex!.Message, Does.Contain($"not found"));
    }

    [Test]
    public void UpdateStatusAsync_ShouldThrow_WhenGuestAccessValidationFails()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: 10);
        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(() =>
            _service.UpdateStatusAsync(order.Id, updateDto));

        Assert.That(ex!.Message, Does.Contain($"Cannot access"));
    }

    [Test]
    public void UpdateStatusAsync_ShouldThrow_WhenGuestTriesInvalidStatusChange()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: table.Id, status: OrderStatus.delivered);
        order.Table = table;

        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled); // Invalid transition

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true);
        _currentUser.IsGuest.Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.UpdateStatusAsync(order.Id, updateDto));

        Assert.That(ex!.Message, Does.Contain($"Order status cannot be changed to {updateDto.Status}"));
    }
    [Test]
    public async Task UpdateStatusAsync_ShouldSucceed_WhenGuestCancelsCreatedOrder()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var order = TestDataFactory.CreateValidOrder(id: 3, tableId: table.Id, status: OrderStatus.created);
        order.Table = table;

        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true); // Now returns bool
        _currentUser.IsGuest.Returns(true);

        // Act & Assert (no exception means success)
        Assert.DoesNotThrowAsync(() => _service.UpdateStatusAsync(order.Id, updateDto));
        await _orderRepo.Received().UpdateAsync(Arg.Is<Order>(o => o.Status == OrderStatus.cancelled));
        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderStatusUpdated)
        );
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldSucceed_WhenGuestRequestsPaymentOnDeliveredOrder()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var order = TestDataFactory.CreateValidOrder(id: 4, tableId: table.Id, status: OrderStatus.delivered);
        order.Table = table;

        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.payment_requested, PaymentType.creditcard);

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true); // Updated to match new bool-returning method
        _currentUser.IsGuest.Returns(true);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _service.UpdateStatusAsync(order.Id, updateDto));

        await _orderRepo.Received().UpdateAsync(Arg.Is<Order>(o =>
            o.Status == OrderStatus.payment_requested &&
            o.PaymentType == PaymentType.creditcard));

        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderStatusUpdated)
        );
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldSucceed_WhenStaffUpdatesOrderStatus()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var order = TestDataFactory.CreateValidOrder(id: 5, tableId: table.Id, status: OrderStatus.created);
        order.Table = table;

        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.delivered, PaymentType.creditcard);

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true); // Adjusted for new bool return
        _currentUser.IsGuest.Returns(false);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _service.UpdateStatusAsync(order.Id, updateDto));

        await _orderRepo.Received().UpdateAsync(Arg.Is<Order>(o =>
            o.Status == OrderStatus.delivered &&
            o.PaymentType == PaymentType.creditcard));

        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderStatusUpdated)
        );
    }

    [Test]
    public void UpdateAsync_ShouldThrow_WhenOrderDoesNotExist()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 1);
        int orderId = 999;

        _orderRepo.GetByIdAsync(orderId, true).Returns((Order?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<OrderNotFoundException>(() => _service.UpdateAsync(orderId, dto));

        Assert.That(ex!.Message, Does.Contain($"not found"));
    }

    [Test]
    public void UpdateAsync_ShouldThrow_WhenGuestAccessValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 10);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 2, tableId: dto.TableId);

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId)
            .Returns(false); // Now returns a bool, not ServiceResult

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(() =>
            _service.UpdateAsync(existingOrder.Id, dto));

        Assert.That(ex!.Message, Does.Contain($"Cannot access"));
    }

    [Test]
    public void UpdateAsync_ShouldThrow_WhenOrderIsClosed()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 10);
        var closedOrder = TestDataFactory.CreateValidOrder(id: 3, tableId: dto.TableId, status: OrderStatus.closed);

        _orderRepo.GetByIdAsync(closedOrder.Id, true).Returns(closedOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true); // validation now returns bool
        _currentUser.IsGuest.Returns(false); // doesn't matter if role is not elevated

        _currentUser.GetCurrentUserAsync()
            .Returns(TestDataFactory.CreateValidStaff(role: EmployeeRole.regular)); // regular can't override closed orders

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.UpdateAsync(closedOrder.Id, dto));

        Assert.That(ex!.Message, Does.Contain("Access to change order denied"));
    }

    [Test]
    public void UpdateAsync_ShouldThrow_WhenGuestTriesToUpdateNonCancelledOrder()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 20);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 4, tableId: dto.TableId, status: OrderStatus.delivered);

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true);
        _currentUser.IsGuest.Returns(true); // guest is trying this

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() =>
            _service.UpdateAsync(existingOrder.Id, dto));

        Assert.That(ex!.Message, Does.Contain("Access to change order denied"));
    }

    [Test]
    public void UpdateAsync_ShouldThrow_WhenOrderValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 5);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 5, tableId: dto.TableId, status: OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(true);
        _currentUser.IsGuest.Returns(true); // guest updating cancelled order is allowed

        // Simulate failure in validation: table is not occupied
        var table = TestDataFactory.CreateValidTable(id: dto.TableId, status: TableStatus.empty);
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.UpdateAsync(existingOrder.Id, dto));

        Assert.That(ex!.Message, Does.Contain("unoccupied table"));
    }

    [Test]
    public async Task UpdateAsync_ShouldSucceed_WhenStaffUpdatesOrder()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: table.Id);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 6, tableId: table.Id, status: OrderStatus.created);
        existingOrder.Table = table;

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(table.Id).Returns(true); // now returns bool
        _currentUser.IsGuest.Returns(false); // staff

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(role: EmployeeRole.admin));
        _tableRepo.GetByIdAsync(table.Id).Returns(table);

        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        _orderRepo.UpdateOrderWithItemsAsync(existingOrder, Arg.Any<List<ProductPerOrder>>())
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(existingOrder.Id, dto);

        // Assert
        Assert.DoesNotThrowAsync(() => _service.UpdateAsync(existingOrder.Id, dto));
        await _orderRepo.Received().UpdateOrderWithItemsAsync(existingOrder, Arg.Any<List<ProductPerOrder>>());
        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderContentUpdated)
        );
    }

    [Test]
    public async Task UpdateAsync_ShouldSucceed_WhenGuestUpdatesCancelledOrder()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: table.Id);
        var cancelledOrder = TestDataFactory.CreateValidOrder(id: 7, tableId: table.Id, status: OrderStatus.cancelled);
        cancelledOrder.Table = table;

        _orderRepo.GetByIdAsync(cancelledOrder.Id, true).Returns(cancelledOrder);
        _validationService.VerifyUserGuestAccess(table.Id).Returns(true);
        _currentUser.IsGuest.Returns(true);

        _tableRepo.GetByIdAsync(table.Id).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        _orderRepo.UpdateOrderWithItemsAsync(cancelledOrder, Arg.Any<List<ProductPerOrder>>())
            .Returns(Task.CompletedTask);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _service.UpdateAsync(cancelledOrder.Id, dto));

        await _orderRepo.Received().UpdateOrderWithItemsAsync(cancelledOrder, Arg.Any<List<ProductPerOrder>>());
        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderContentUpdated)
        );
    }

    [Test]
    public void DeleteAsync_ShouldThrow_WhenOrderDoesNotExist()
    {
        // Arrange
        int orderId = 999;
        _orderRepo.GetByIdAsync(orderId).Returns((Order?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<OrderNotFoundException>(() => _service.DeleteAsync(orderId));
        Assert.That(ex!.Message, Does.Contain($"not found"));
    }

    [Test]
    public void DeleteAsync_ShouldThrow_WhenGuestAccessValidationFails()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: 10);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId)
            .Returns(false); // Not a ServiceResult anymore

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(() => _service.DeleteAsync(order.Id));
        Assert.That(ex!.Message, Does.Contain($"Cannot access").IgnoreCase);
    }

    [Test]
    public void DeleteAsync_ShouldThrow_WhenOrderIsNotCancelled()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 3, status: OrderStatus.delivered);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AppValidationException>(() => _service.DeleteAsync(order.Id));
        Assert.That(ex!.Message, Is.EqualTo("Only cancelled orders can be removed"));
    }

    [Test]
    public async Task DeleteAsync_ShouldSucceed_WhenOrderIsCancelledAndAccessIsValid()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 4, status: OrderStatus.cancelled);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _service.DeleteAsync(order.Id));
        await _orderRepo.Received().DeleteAsync(order);
    }
}
