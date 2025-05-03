using AutoMapper;
using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Linq.Expressions;

namespace BartenderTests;

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
    public async Task AddAsync_ShouldFail_WhenGuestAccessValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto();
        _validationService.VerifyUserGuestAccess(dto.TableId)
            .Returns(ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized));

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Unauthorized"));
        });
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenOrderValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto();

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _validationService.EnsurePlaceExistsAsync(Arg.Any<int>()).Returns(ServiceResult.Ok());
        _service.GetType().GetMethod("ValidateOrderAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_service, [dto]);

        _tableRepo.GetByIdAsync(dto.TableId)
            .Returns(TestDataFactory.CreateValidTable(dto.TableId, status: TableStatus.empty));

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenGuestSessionNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 1, menuItemId: 1, count: 1, totalPrice: 5m);

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _tableRepo.GetByIdAsync(dto.TableId)
            .Returns(TestDataFactory.CreateValidTable(dto.TableId));

        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());


        _currentUser.IsGuest.Returns(true);
        _currentUser.GetRawToken().Returns("invalid-token");
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>())
            .Returns((GuestSession?)null);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo("There is currently no active session found"));
        });
    }

    [Test]
    public async Task AddAsync_ShouldSucceed_WhenGuestOrderIsValid()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var guest = TestDataFactory.CreateValidGuestSession(table);
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: table.Id, menuItemId: 1, count: 1, totalPrice: 5m);

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        _currentUser.IsGuest.Returns(true);
        _currentUser.GetRawToken().Returns(guest.Token);
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(guest);

        _mapper.Map<Order>(dto).Returns(new Order { Table = table });
        _orderRepo.CreateOrderWithItemsAsync(Arg.Any<Order>(), Arg.Any<List<ProductPerOrder>>()).Returns(new Order { Id = 1, Table = table });

        _mapper.Map<OrderDto>(Arg.Any<Order>())
        .Returns(TestDataFactory.CreateValidOrderDto(id: 1, tableLabel: table.Label));

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
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

        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);
        _menuItemRepo.GetFilteredAsync(filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(), includeNavigations: true)
            .Returns(TestDataFactory.CreateSampleMenuItems());

        _currentUser.IsGuest.Returns(false);

        _mapper.Map<Order>(dto).Returns(new Order { Table = table });
        _orderRepo.CreateOrderWithItemsAsync(Arg.Any<Order>(), Arg.Any<List<ProductPerOrder>>()).Returns(new Order { Id = 1, Table = table });

        _mapper.Map<OrderDto>(Arg.Any<Order>())
            .Returns(TestDataFactory.CreateValidOrderDto(id: 1, tableLabel: table.Label));

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldFail_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = 99;
        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(orderId, true).Returns((Order?)null);

        // Act
        var result = await _service.UpdateStatusAsync(orderId, updateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo($"Order with id {orderId} not found"));
        });
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldFail_WhenGuestAccessValidationFails()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: 10);
        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId)
            .Returns(ServiceResult.Fail("Access denied", ErrorType.Unauthorized));

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, updateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Access denied"));
        });
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldFail_WhenGuestTriesInvalidStatusChange()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable();
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: table.Id, status: OrderStatus.delivered);
        order.Table = table;

        var updateDto = TestDataFactory.CreateUpdateStatusDto(OrderStatus.cancelled); // not allowed

        _orderRepo.GetByIdAsync(order.Id, true).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(true);

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, updateDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo($"Order status cannot be changed to {updateDto.Status}"));
        });
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
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(true);

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, updateDto);

        // Assert
        Assert.That(result.Success, Is.True);
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
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(true);

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, updateDto);

        // Assert
        Assert.That(result.Success, Is.True);
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
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(false);

        // Act
        var result = await _service.UpdateStatusAsync(order.Id, updateDto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _orderRepo.Received().UpdateAsync(Arg.Is<Order>(o =>
            o.Status == OrderStatus.delivered &&
            o.PaymentType == PaymentType.creditcard));
        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderStatusUpdated)
        );
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenOrderDoesNotExist()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 1);
        int orderId = 999;

        _orderRepo.GetByIdAsync(orderId, true).Returns((Order?)null);

        // Act
        var result = await _service.UpdateAsync(orderId, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo($"Order with id {orderId} not found"));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenGuestAccessValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 10);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 2, tableId: dto.TableId);

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId)
            .Returns(ServiceResult.Fail("Access denied", ErrorType.Unauthorized));

        // Act
        var result = await _service.UpdateAsync(existingOrder.Id, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Access denied"));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenOrderIsClosed()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 10);
        var closedOrder = TestDataFactory.CreateValidOrder(id: 3, tableId: dto.TableId, status: OrderStatus.closed);

        _orderRepo.GetByIdAsync(closedOrder.Id, true).Returns(closedOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(false); // doesn't matter here

        // Act
        var result = await _service.UpdateAsync(closedOrder.Id, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error, Is.EqualTo("Order cannot be changed anymore"));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenGuestTriesToUpdateNonCancelledOrder()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 20);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 4, tableId: dto.TableId, status: OrderStatus.delivered);

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(true); // guest is trying this

        // Act
        var result = await _service.UpdateAsync(existingOrder.Id, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error, Is.EqualTo("Order cannot be changed anymore"));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenOrderValidationFails()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertOrderDto(tableId: 5);
        var existingOrder = TestDataFactory.CreateValidOrder(id: 5, tableId: dto.TableId, status: OrderStatus.cancelled);

        _orderRepo.GetByIdAsync(existingOrder.Id, true).Returns(existingOrder);
        _validationService.VerifyUserGuestAccess(dto.TableId).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(true); // guest updating cancelled order is valid

        // Simulate failure from ValidateOrderAsync (e.g. table unoccupied)
        var table = TestDataFactory.CreateValidTable(id: dto.TableId, status: TableStatus.empty);
        _tableRepo.GetByIdAsync(dto.TableId).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        // Act
        var result = await _service.UpdateAsync(existingOrder.Id, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Cannot create an order on an unoccupied table"));
        });
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
        _validationService.VerifyUserGuestAccess(table.Id).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(false); // staff

        _tableRepo.GetByIdAsync(table.Id).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        _orderRepo.UpdateOrderWithItemsAsync(existingOrder, Arg.Any<List<ProductPerOrder>>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(existingOrder.Id, dto);

        // Assert
        Assert.That(result.Success, Is.True);
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
        _validationService.VerifyUserGuestAccess(table.Id).Returns(ServiceResult.Ok());
        _currentUser.IsGuest.Returns(true); // guest

        _tableRepo.GetByIdAsync(table.Id).Returns(table);
        _menuItemRepo.GetFilteredAsync(
            filterBy: Arg.Any<Expression<Func<MenuItem, bool>>>(),
            includeNavigations: true
        ).Returns(TestDataFactory.CreateSampleMenuItems());

        _orderRepo.UpdateOrderWithItemsAsync(cancelledOrder, Arg.Any<List<ProductPerOrder>>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(cancelledOrder.Id, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _orderRepo.Received().UpdateOrderWithItemsAsync(cancelledOrder, Arg.Any<List<ProductPerOrder>>());
        await _notificationService.Received().AddNotificationAsync(
            Arg.Any<Table>(),
            Arg.Is<TableNotification>(n => n.Type == NotificationType.OrderContentUpdated)
        );
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenOrderDoesNotExist()
    {
        // Arrange
        int orderId = 999;
        _orderRepo.GetByIdAsync(orderId).Returns((Order?)null);

        // Act
        var result = await _service.DeleteAsync(orderId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo($"Order with ID {orderId} not found"));
        });
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenGuestAccessValidationFails()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: 10);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId)
            .Returns(ServiceResult.Fail("Access denied", ErrorType.Unauthorized));

        // Act
        var result = await _service.DeleteAsync(order.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Access denied"));
        });
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenOrderIsNotCancelled()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 3, status: OrderStatus.delivered);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());

        // Act
        var result = await _service.DeleteAsync(order.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo("Only cancelled orders can be removed"));
        });
    }

    [Test]
    public async Task DeleteAsync_ShouldSucceed_WhenOrderIsCancelledAndAccessIsValid()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 4, status: OrderStatus.cancelled);
        _orderRepo.GetByIdAsync(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());

        // Act
        var result = await _service.DeleteAsync(order.Id);

        // Assert
        Assert.That(result.Success, Is.True);
        await _orderRepo.Received().DeleteAsync(order);
    }

}
