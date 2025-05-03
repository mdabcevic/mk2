using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace BartenderTests;

[TestFixture]
public class OrderServiceReadTests
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
    public async Task GetAllClosedOrdersByPlaceIdAsync_ShouldReturnOrders_WhenUserHasAccess()
    {
        // Arrange
        var placeId = 1;
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: placeId);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1);

        _validationService.EnsurePlaceExistsAsync(placeId)
            .Returns(ServiceResult.Ok());

        _validationService.VerifyUserPlaceAccess(placeId)
            .Returns(true);

        _orderRepo.GetAllByPlaceIdAsync(placeId, Arg.Any<int>())
            .Returns(([order], 1));

        _mapper.Map<List<OrderDto>>(Arg.Is<List<Order>>(o => o.Any()))
            .Returns([orderDto]);

        // Act
        var result = await _service.GetAllClosedOrdersByPlaceIdAsync(placeId, page: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Items, Is.Not.Null);
        });
        Assert.That(result.Data!.Items, Has.Count.EqualTo(1));
        Assert.That(result.Data!.Items![0].Id, Is.EqualTo(orderDto.Id));
    }


    [Test]
    public async Task GetAllClosedOrdersByPlaceIdAsync_ShouldReturnFail_WhenPlaceAccessInvalid()
    {
        // Arrange
        var placeId = 1;
        _validationService.EnsurePlaceExistsAsync(placeId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId).Returns(false); // simulate cross-access

        // Act
        var result = await _service.GetAllClosedOrdersByPlaceIdAsync(placeId, page: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Cross-business access denied."));
        });
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdAsync_ShouldReturnActiveOrders_WhenAccessValid()
    {
        // Arrange
        var placeId = 1;
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: placeId);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1);

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _orderRepo.GetActiveByPlaceIdAsync(placeId).Returns([order]);
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns([orderDto]);

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdAsync(placeId, onlyWaitingForStaff: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.That(result.Data!, Has.Count.EqualTo(1));
        Assert.That(result.Data![0].Id, Is.EqualTo(orderDto.Id));
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdAsync_ShouldReturnPendingOrders_WhenOnlyWaitingForStaffIsTrue()
    {
        // Arrange
        var placeId = 1;
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: placeId);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 2);

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _orderRepo.GetPendingByPlaceIdAsync(placeId).Returns([order]);
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns([orderDto]);

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdAsync(placeId, onlyWaitingForStaff: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.That(result.Data!, Has.Count.EqualTo(1));
        Assert.That(result.Data![0].Id, Is.EqualTo(orderDto.Id));
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var placeId = 1;
        _validationService.EnsurePlaceExistsAsync(placeId)
            .Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId)
            .Returns(false); // Unauthorized access

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdAsync(placeId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Cross-business access denied."));
        });
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldReturnGroupedOrders_WhenAccessValid()
    {
        // Arrange
        var placeId = 1;
        var page = 1;

        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: placeId, status: OrderStatus.created);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1, status: OrderStatus.created);

        var grouped = new Dictionary<OrderStatus, List<Order>>
        {
            [OrderStatus.created] = [order]
        };

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _orderRepo.GetActiveByPlaceIdGroupedAsync(placeId, page).Returns((grouped, 1));
        _mapper.Map<List<OrderDto>>(grouped[OrderStatus.created]).Returns([orderDto]);

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.That(result.Data!.Items, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Items![0].Status!, Is.EqualTo(OrderStatus.created));
            Assert.That(result.Data!.Items[0].Orders, Has.Count.EqualTo(1));
        });
        Assert.That(result.Data!.Items![0].Orders![0].Id, Is.EqualTo(orderDto.Id));
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldReturnPendingGroupedOrders_WhenOnlyWaitingForStaffIsTrue()
    {
        // Arrange
        var placeId = 1;
        var page = 1;

        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: placeId, status: OrderStatus.payment_requested);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 2, status: OrderStatus.payment_requested);

        var grouped = new Dictionary<OrderStatus, List<Order>>
        {
            [OrderStatus.payment_requested] = [order]
        };

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _orderRepo.GetPendingByPlaceIdGroupedAsync(placeId, page).Returns((grouped, 1));
        _mapper.Map<List<OrderDto>>(grouped[OrderStatus.payment_requested]).Returns([orderDto]);

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page, onlyWaitingForStaff: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
        });
        Assert.That(result.Data!.Items, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Items![0].Status!, Is.EqualTo(OrderStatus.payment_requested));
            Assert.That(result.Data!.Items![0].Orders![0].Id, Is.EqualTo(orderDto.Id));
        });
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var placeId = 1;
        _validationService.EnsurePlaceExistsAsync(placeId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserPlaceAccess(placeId).Returns(false);

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
    }

    [Test]
    public async Task GetAllByBusinessIdAsync_ShouldReturnGroupedOrders_WhenUserHasAccess()
    {
        // Arrange
        var businessId = 1;
        var place = TestDataFactory.CreateValidPlace(id: 10, businessid: businessId);
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: 10);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1);
        var placeDto = TestDataFactory.CreatePlaceDtoFromPlace(place);

        var input = new Dictionary<Place, List<Order>>
    {
        { place, [order] }
    };

        _validationService.EnsureBusinessExistsAsync(businessId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserBusinessAccess(businessId).Returns(true);
        _orderRepo.GetAllOrdersByBusinessIdAsync(businessId).Returns(input);
        _mapper.Map<PlaceDto>(place).Returns(placeDto);
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns([orderDto]);

        // Act
        var result = await _service.GetAllByBusinessIdAsync(businessId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Place.Address, Is.EqualTo(placeDto.Address));
            Assert.That(result.Data![0].Orders, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Orders![0].Id, Is.EqualTo(orderDto.Id));
        });
    }

    [Test]
    public async Task GetAllByBusinessIdAsync_ShouldFail_WhenBusinessDoesNotExist()
    {
        // Arrange
        var businessId = 1;
        _validationService.EnsureBusinessExistsAsync(businessId)
            .Returns(ServiceResult.Fail("Business not found", ErrorType.NotFound));

        // Act
        var result = await _service.GetAllByBusinessIdAsync(businessId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo("Business not found"));
        });
    }


    [Test]
    public async Task GetAllByBusinessIdAsync_ShouldFail_WhenAccessIsDenied()
    {
        // Arrange
        var businessId = 1;
        _validationService.EnsureBusinessExistsAsync(businessId).Returns(ServiceResult.Ok());
        _validationService.VerifyUserBusinessAccess(businessId).Returns(false);

        // Act
        var result = await _service.GetAllByBusinessIdAsync(businessId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Cross-business access denied."));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExistsAndUserHasAccess()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: 5);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1);

        _orderRepo.getOrderById(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(ServiceResult.Ok());
        _mapper.Map<OrderDto>(order).Returns(orderDto);

        // Act
        var result = await _service.GetByIdAsync(order.Id, skipValidation: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(orderDto.Id));
        });
    }


    [Test]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenOrderIsMissing()
    {
        // Arrange
        _orderRepo.getOrderById(Arg.Any<int>()).Returns((Order?)null);

        // Act
        var result = await _service.GetByIdAsync(123, skipValidation: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: 10);

        _orderRepo.getOrderById(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId)
            .Returns(ServiceResult.Fail("Access denied", ErrorType.Unauthorized));

        // Act
        var result = await _service.GetByIdAsync(order.Id, skipValidation: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Access denied"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenSkipValidationIsTrue()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 3, tableId: 20);
        var dto = TestDataFactory.CreateValidOrderDto(id: 3);

        _orderRepo.getOrderById(order.Id).Returns(order);
        _mapper.Map<OrderDto>(order).Returns(dto);

        // Act
        var result = await _service.GetByIdAsync(order.Id, skipValidation: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(dto.Id));
        });
    }

    [Test]
    public async Task GetCurrentOrdersByTableLabelAsync_ShouldReturnEmptyList_WhenNoOrdersFound()
    {
        // Arrange
        _orderRepo.GetCurrentOrdersByTableLabelAsync("A1").Returns(new List<Order>());

        // Act
        var result = await _service.GetCurrentOrdersByTableLabelAsync("A1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
        });
    }

    [Test]
    public async Task GetCurrentOrdersByTableLabelAsync_ShouldReturnOrders_WhenValidationPasses()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 10, label: "T1");
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: table.Id);
        order.Table = table;

        var dto = TestDataFactory.CreateValidOrderDto(id: 1);

        _orderRepo.GetCurrentOrdersByTableLabelAsync("T1").Returns([order]);
        _validationService.VerifyUserGuestAccess(table.Id).Returns(ServiceResult.Ok());
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns([dto]);

        // Act
        var result = await _service.GetCurrentOrdersByTableLabelAsync("T1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(dto.Id));
        });
    }

    [Test]
    public async Task GetCurrentOrdersByTableLabelAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 10, label: "T1");
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: table.Id);
        order.Table = table;

        _orderRepo.GetCurrentOrdersByTableLabelAsync("T1").Returns([order]);
        _validationService.VerifyUserGuestAccess(table.Id)
            .Returns(ServiceResult.Fail("Unauthorized", ErrorType.Unauthorized));

        // Act
        var result = await _service.GetCurrentOrdersByTableLabelAsync("T1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Is.EqualTo("Unauthorized"));
        });
    }

    [Test]
    public async Task GetActiveTableOrdersForUserAsync_ShouldReturnOrders_WhenGuestSessionExists_AndUserSpecificTrue()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 5);
        var guest = TestDataFactory.CreateValidGuestSession(table);
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: table.Id);
        var dto = TestDataFactory.CreateValidOrderDto(id: 1);

        _currentUser.GetRawToken().Returns(guest.Token);
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(guest);
        _orderRepo.GetActiveOrdersByGuestIdAsync(guest.Id).Returns([order]);
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns([dto]);

        // Act
        var result = await _service.GetActiveTableOrdersForUserAsync(userSpecific: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(dto.Id));
        });
    }

    [Test]
    public async Task GetActiveTableOrdersForUserAsync_ShouldReturnOrders_WhenGuestSessionExists_AndUserSpecificFalse()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 10);
        var guest = TestDataFactory.CreateValidGuestSession(table);
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: table.Id);
        var dto = TestDataFactory.CreateValidOrderDto(id: 2);

        _currentUser.GetRawToken().Returns(guest.Token);
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(guest);
        _orderRepo.GetActiveOrdersByTableIdAsync(table.Id).Returns([order]);
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns([dto]);

        // Act
        var result = await _service.GetActiveTableOrdersForUserAsync(userSpecific: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(dto.Id));
        });
    }

    [Test]
    public async Task GetActiveTableOrdersForUserAsync_ShouldFail_WhenNoGuestSessionFound()
    {
        // Arrange
        _currentUser.GetRawToken().Returns("invalid-token");
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null);

        // Act
        var result = await _service.GetActiveTableOrdersForUserAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo("There is currently no active session found"));
        });
    }

}
