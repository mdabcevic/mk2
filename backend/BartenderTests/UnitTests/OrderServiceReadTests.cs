using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.DTO.Place;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace BartenderTests.UnitTests;

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

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask);
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);

        _orderRepo.GetAllByPlaceIdAsync(placeId, Arg.Any<int>(), Arg.Any<int>())
            .Returns(([order], 1));

        _mapper.Map<List<OrderDto>>(Arg.Is<List<Order>>(o => o.Any()))
            .Returns([orderDto]);

        // Act
        var result = await _service.GetAllClosedOrdersByPlaceIdAsync(placeId, page: 1, size:15);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items[0].Id, Is.EqualTo(orderDto.Id));
    }

    [Test]
    public void GetAllClosedOrdersByPlaceIdAsync_ShouldThrow_WhenPlaceAccessInvalid()
    {
        // Arrange
        var placeId = 1;
        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask);
        _validationService.VerifyUserPlaceAccess(placeId).Returns(false); // simulate cross-access

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(
            () => _service.GetAllClosedOrdersByPlaceIdAsync(placeId, page: 1, size: 15)
        );
        Assert.That(ex!.Message, Does.Contain("Access"));
        Assert.That(ex!.Message, Does.Contain("denied"));
    }

    [Test]
    public void GetAllActiveOrdersByPlaceIdAsync_ShouldThrow_WhenPlaceAccessInvalid()
    {
        // Arrange
        var placeId = 1;
        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask); // Ensure the place exists
        _validationService.VerifyUserPlaceAccess(placeId).Returns(false); // Simulate cross-access

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(
            () => _service.GetAllActiveOrdersByPlaceIdAsync(placeId, onlyWaitingForStaff: false)
        );
        Assert.That(ex!.Message, Does.Contain("Access"));
        Assert.That(ex!.Message, Does.Contain("denied"));
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdAsync_ShouldReturnPendingOrders_WhenOnlyWaitingForStaffIsTrue()
    {
        // Arrange
        var placeId = 1;
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: placeId);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 2);

        // Simulate that place exists and the user has access
        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask); // no result means no error
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true); // User has access

        // Simulate getting pending orders from repository
        _orderRepo.GetPendingByPlaceIdAsync(placeId).Returns(new List<Order> { order });
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns(new List<OrderDto> { orderDto });

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdAsync(placeId, onlyWaitingForStaff: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);  // Check that the result is not null
            Assert.That(result, Has.Count.EqualTo(1));  // Check that exactly one order is returned
            Assert.That(result[0].Id, Is.EqualTo(orderDto.Id));  // Check that the order ID is correct
        });
    }

    [Test]
    public void GetAllActiveOrdersByPlaceIdAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var placeId = 1;
        _validationService.EnsurePlaceExistsAsync(placeId)
            .Returns(Task.CompletedTask); // Simulate valid place
        _validationService.VerifyUserPlaceAccess(placeId)
            .Returns(false); // Simulate unauthorized access

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () =>
            await _service.GetAllActiveOrdersByPlaceIdAsync(placeId));

        Assert.That(ex.Message, Does.Contain("Access"));
        Assert.That(ex.Message, Does.Contain("denied"));
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldReturnGroupedOrders_WhenAccessValid()
    {
        // Arrange
        var placeId = 1;
        var page = 1;
        var size = 15;

        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: placeId, status: OrderStatus.created);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1, status: OrderStatus.created);

        var grouped = new Dictionary<OrderStatus, List<Order>>
        {
            [OrderStatus.created] = new List<Order> { order }
        };

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask); // Simulate valid place
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true); // User has access
        _orderRepo.GetActiveByPlaceIdGroupedAsync(placeId, page,size).Returns((grouped, 1)); // Simulate grouped orders
        _mapper.Map<List<OrderDto>>(grouped[OrderStatus.created]).Returns(new List<OrderDto> { orderDto }); // Map orders to DTOs

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page,size);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.Not.Null); // Check if Items is not null
            Assert.That(result.Items, Has.Count.EqualTo(1)); // One group (OrderStatus.created)

            Assert.Multiple(() =>
            {
                Assert.That(result.Items[0].Status, Is.EqualTo(OrderStatus.created)); // Check the status of the group
                Assert.That(result.Items[0].Orders, Has.Count.EqualTo(1)); // One order in this group
            });

            Assert.That(result.Items[0].Orders[0].Id, Is.EqualTo(orderDto.Id)); // Check the ID of the order
        });
    }

    [Test]
    public async Task GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldReturnPendingGroupedOrders_WhenOnlyWaitingForStaffIsTrue()
    {
        // Arrange
        var placeId = 1;
        var page = 1;
        var size = 15;

        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: placeId, status: OrderStatus.payment_requested);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 2, status: OrderStatus.payment_requested);

        var grouped = new Dictionary<OrderStatus, List<Order>>
        {
            [OrderStatus.payment_requested] = new List<Order> { order }
        };

        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask); // No ServiceResult, just ensuring no exception is thrown
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true); // Simulating valid access
        _orderRepo.GetPendingByPlaceIdGroupedAsync(placeId, page, size).Returns((grouped, 1));
        _mapper.Map<List<OrderDto>>(grouped[OrderStatus.payment_requested]).Returns([orderDto]);

        // Act
        var result = await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page,size, onlyWaitingForStaff: true);

        // Assert
        Assert.That(result.Items, Is.Not.Null); // Accessing Items instead of Data
        Assert.That(result.Items, Has.Count.EqualTo(1)); // Checking the number of grouped orders
        Assert.Multiple(() =>
        {
            Assert.That(result.Items[0].Status, Is.EqualTo(OrderStatus.payment_requested)); // Verifying status
            Assert.That(result.Items[0].Orders[0].Id, Is.EqualTo(orderDto.Id)); // Verifying order ID in the group
        });
    }

    [Test]
    public void GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var placeId = 1;

        // Simulate that place exists but user does not have access
        _validationService.EnsurePlaceExistsAsync(placeId).Returns(Task.CompletedTask); // No result, just ensures no exception
        _validationService.VerifyUserPlaceAccess(placeId).Returns(false); // Unauthorized access

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(async () =>
            await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId, page: 1,size: 15));

        Assert.That(ex!.Message, Does.Contain("Access"));
        Assert.That(ex!.Message, Does.Contain("denied"));
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
        { place, new List<Order> { order } }
    };

        // Simulating that business exists and user has access
        _validationService.EnsureBusinessExistsAsync(businessId).Returns(Task.CompletedTask); // Ensures business exists
        _validationService.VerifyUserBusinessAccess(businessId).Returns(true); // User has access

        // Simulating order fetch and mapping
        _orderRepo.GetAllOrdersByBusinessIdAsync(businessId).Returns(input);
        _mapper.Map<PlaceDto>(place).Returns(placeDto);
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns(new List<OrderDto> { orderDto });

        // Act
        var result = await _service.GetAllByBusinessIdAsync(businessId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);  // Check that result is not null
            Assert.That(result, Has.Count.EqualTo(1));  // Ensure one business order is returned
            Assert.That(result[0].Place.Address, Is.EqualTo(placeDto.Address));
            Assert.That(result[0].Orders, Has.Count.EqualTo(1));
            Assert.That(result[0].Orders![0].Id, Is.EqualTo(orderDto.Id));
        });
    }

    [Test]
    public void GetAllByBusinessIdAsync_ShouldFail_WhenBusinessDoesNotExist()
    {
        // Arrange
        var businessId = 1;

        // Simulate the validation service throwing an exception when business does not exist
        _validationService.EnsureBusinessExistsAsync(businessId)
            .ThrowsAsync(new BusinessNotFoundException(businessId));

        // Act & Assert
        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(() => _service.GetAllByBusinessIdAsync(businessId));

        Assert.That(ex.Message, Does.Contain($"not found"));
    }

    [Test]
    public void GetAllByBusinessIdAsync_ShouldFail_WhenAccessIsDenied()
    {
        // Arrange
        var businessId = 1;

        // Simulate business existence check passing but user access being denied
        _validationService.EnsureBusinessExistsAsync(businessId).Returns(Task.CompletedTask);
        _validationService.VerifyUserBusinessAccess(businessId).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(() => _service.GetAllByBusinessIdAsync(businessId));

        // Assert the exception message
        Assert.That(ex!.Message, Does.Contain("Access"));
        Assert.That(ex!.Message, Does.Contain("denied"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExistsAndUserHasAccess()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: 5);
        var orderDto = TestDataFactory.CreateValidOrderDto(id: 1);

        // Simulate that order exists and user has access
        _orderRepo.getOrderById(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId).Returns(true); // Assuming true means access granted
        _mapper.Map<OrderDto>(order).Returns(orderDto);

        // Act
        var result = await _service.GetByIdAsync(order.Id, skipValidation: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);  // Check that result is not null
            Assert.That(result.Id, Is.EqualTo(orderDto.Id));  // Check if the order's ID matches
        });
    }

    [Test]
    public void GetByIdAsync_ShouldThrowOrderNotFoundException_WhenOrderIsMissing()
    {
        // Arrange
        _orderRepo.getOrderById(Arg.Any<int>()).Returns((Order?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<OrderNotFoundException>(async () =>
            await _service.GetByIdAsync(123, skipValidation: true));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public void GetByIdAsync_ShouldThrowUnauthorizedOrderAccessException_WhenValidationFails()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 2, tableId: 10);

        _orderRepo.getOrderById(order.Id).Returns(order);
        _validationService.VerifyUserGuestAccess(order.TableId)
            .Returns(false); // Simulate failure (i.e., access denied)

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(async () =>
            await _service.GetByIdAsync(order.Id, skipValidation: false));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Contain($"Cannot access"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenSkipValidationIsTrue()
    {
        // Arrange
        var order = TestDataFactory.CreateValidOrder(id: 3, tableId: 20);
        var dto = TestDataFactory.CreateValidOrderDto(id: 3);

        _orderRepo.getOrderById(order.Id).Returns(order);  // Simulating retrieval of the order
        _mapper.Map<OrderDto>(order).Returns(dto);  // Mapping the order to an OrderDto

        // Act
        var result = await _service.GetByIdAsync(order.Id, skipValidation: true);  // Calling the service method with skipValidation set to true

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Items, Is.EqualTo(dto.Items));
            Assert.That(result.Table, Is.EqualTo(dto.Table));
            Assert.That(result.PaymentType, Is.EqualTo(dto.PaymentType));
        });
    }

    [Test]
    public async Task GetCurrentOrdersByTableLabelAsync_ShouldReturnEmptyList_WhenNoOrdersFound()
    {
        // Arrange
        _orderRepo.GetCurrentOrdersByTableLabelAsync("A1").Returns(new List<Order>());  // Simulate no orders found for table "A1"

        // Act
        var result = await _service.GetCurrentOrdersByTableLabelAsync("A1");  // Call the service method

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
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

        _orderRepo.GetCurrentOrdersByTableLabelAsync("T1").Returns(new List<Order> { order });
        _validationService.VerifyUserGuestAccess(table.Id).Returns(true); // Simulating validation pass
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns(new List<OrderDto> { dto });

        // Act
        var result = await _service.GetCurrentOrdersByTableLabelAsync("T1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(dto.Id));
        });
    }

    [Test]
    public void GetCurrentOrdersByTableLabelAsync_ShouldFail_WhenValidationFails()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 10, label: "T1");
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: table.Id);
        order.Table = table;

        _orderRepo.GetCurrentOrdersByTableLabelAsync("T1").Returns(new List<Order> { order });
        _validationService.VerifyUserGuestAccess(table.Id).Returns(false); // Simulating validation failure

        // Act & Assert
        var ex = Assert.ThrowsAsync<TableAccessDeniedException>(async () =>
            await _service.GetCurrentOrdersByTableLabelAsync("T1")
        );

        Assert.That(ex!.Message, Does.Contain("Access"));
        Assert.That(ex!.Message, Does.Contain("denied"));
    }

    [Test]
    public async Task GetActiveTableOrdersForUserAsync_ShouldReturnOrders_WhenGuestSessionExists_AndUserSpecificTrue()
    {
        // Arrange
        var table = TestDataFactory.CreateValidTable(id: 5);
        var guest = TestDataFactory.CreateValidGuestSession(table);
        var order = TestDataFactory.CreateValidOrder(id: 1, tableId: table.Id);
        var dto = TestDataFactory.CreateValidOrderDto(id: 1);

        // Setting up mock returns for guest session and order retrieval
        _currentUser.GetRawToken().Returns(guest.Token); // Simulating the current user has the guest token
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(guest); // Return the guest session for the token
        _orderRepo.GetActiveOrdersByGuestIdAsync(guest.Id).Returns(new List<Order> { order }); // Simulating the active orders for the guest
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns(new List<OrderDto> { dto }); // Mapping order to DTO

        // Act
        var result = await _service.GetActiveTableOrdersForUserAsync(userSpecific: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(dto.Id));
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

        // Setting up mock returns for guest session and order retrieval
        _currentUser.GetRawToken().Returns(guest.Token); // Simulating the current user has the guest token
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns(guest); // Return the guest session for the token
        _orderRepo.GetActiveOrdersByTableIdAsync(table.Id).Returns(new List<Order> { order }); // Simulating the active orders for the table
        _mapper.Map<List<OrderDto>>(Arg.Any<List<Order>>()).Returns(new List<OrderDto> { dto }); // Mapping order to DTO

        // Act
        var result = await _service.GetActiveTableOrdersForUserAsync(userSpecific: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(dto.Id));
        });
    }

    [Test]
    public async Task GetActiveTableOrdersForUserAsync_ShouldFail_WhenNoGuestSessionFound()
    {
        // Arrange
        _currentUser.GetRawToken().Returns("invalid-token"); // Simulate that the current user has an invalid token
        _guestSessionRepo.GetByKeyAsync(Arg.Any<Expression<Func<GuestSession, bool>>>()).Returns((GuestSession?)null); // Simulate that no guest session was found for the given token

        // Act
        var ex = Assert.ThrowsAsync<NoActiveSessionFoundException>(async () => await _service.GetActiveTableOrdersForUserAsync());

        // Assert
        Assert.That(ex.Message, Is.EqualTo("There is currently no active session found"));
    }
}
