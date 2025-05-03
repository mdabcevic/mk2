using AutoMapper;
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


}
