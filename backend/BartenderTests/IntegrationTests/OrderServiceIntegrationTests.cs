using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class OrderServiceIntegrationTests : IntegrationTestBase
{
    private IOrderService _service = null!;
    private IRepository<Order> _orderRepo = null!;
    private IRepository<Table> _tableRepo = null!;
    private IRepository<MenuItem> _menuItemRepo = null!;
    private IRepository<GuestSession> _guestSessionRepo = null!;
    private IRepository<Product> _productRepo = null!;
    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IOrderService>();
        _orderRepo = scope.ServiceProvider.GetRequiredService<IRepository<Order>>();
        _tableRepo = scope.ServiceProvider.GetRequiredService<IRepository<Table>>();
        _menuItemRepo = scope.ServiceProvider.GetRequiredService<IRepository<MenuItem>>();
        _guestSessionRepo = scope.ServiceProvider.GetRequiredService<IRepository<GuestSession>>();
        _productRepo = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    [Test]
    public async Task AddAsync_ShouldCreateOrder_ForGuest_WhenValid()
    {
        // Arrange: Seed menu item and table
        var product = new Product { Name = "Guest Juice", Volume = "250ml", BusinessId = 1, CategoryId = 1 };
        await _productRepo.AddAsync(product);

        var menuItem = new MenuItem { ProductId = product.Id, PlaceId = 1, Price = 2.5m, IsAvailable = true };
        await _menuItemRepo.AddAsync(menuItem);

        var table = new Table { Label = "G1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "guest-token" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("guest-token");

        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            TotalPrice = 2.5m,
            Items =
        [
            new() { MenuItemId = menuItem.Id, Count = 1 }
        ]
        };

        // Act
        await _service.AddAsync(dto);

        // Assert
        var orderExists = await _orderRepo.ExistsAsync(o => o.TableId == table.Id);
        Assert.That(orderExists, Is.True);
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenMenuItemUnavailable()
    {
        // Arrange
        var product = new Product { Name = "Unavailable Item", BusinessId = 1, CategoryId = 1 };
        await _productRepo.AddAsync(product);

        var menuItem = new MenuItem { ProductId = product.Id, PlaceId = 1, IsAvailable = false, Price = 3.0m };
        await _menuItemRepo.AddAsync(menuItem);

        var table = new Table { Label = "U1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = new GuestSession { TableId = table.Id, Token = "bad-guest" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("bad-guest");

        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            TotalPrice = 3.0m,
            Items =
        [
            new() { MenuItemId = menuItem.Id, Count = 1 }
        ]
        };

        // Act & Arrange
        var ex = Assert.ThrowsAsync<AppValidationException>(() => _service.AddAsync(dto));
        Assert.That(ex!.Message, Does.Contain("unavailable"));
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenTableUnoccupied()
    {
        // Act
        var product = new Product { Name = "Still Juice", BusinessId = 1, CategoryId = 1 };
        await _productRepo.AddAsync(product);

        var menuItem = new MenuItem { ProductId = product.Id, PlaceId = 1, Price = 2.0m, IsAvailable = true };
        await _menuItemRepo.AddAsync(menuItem);

        var guest = new GuestSession { TableId = 1, Token = "wrong-status" };
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("wrong-status");

        var dto = new UpsertOrderDto
        {
            TableId = 1,
            TotalPrice = 2.0m,
            Items =
        [
            new() { MenuItemId = menuItem.Id, Count = 1 }
        ]
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.AddAsync(dto));
        Assert.That(ex!.Message, Does.Contain("unoccupied"));
    }

    [Test]
    public async Task AddAsync_ShouldCorrectTotalPrice_IfMismatch()
    {
        var table = new Table {Label = "TP1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var product = new Product { Name = "Mismatch Juice", BusinessId = 1, CategoryId = 1 };
        await _productRepo.AddAsync(product);

        var menuItem = new MenuItem { ProductId = product.Id, PlaceId = 1, Price = 5.0m, IsAvailable = true };
        await _menuItemRepo.AddAsync(menuItem);

        var guest = TestDataFactory.CreateValidGuestSession(table, token: "guest-mismatch");
        await _guestSessionRepo.AddAsync(guest);

        _mockUser.OverrideGuest("guest-mismatch");

        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            TotalPrice = 1.0m, // incorrect on purpose
            Items = [new() { MenuItemId = menuItem.Id, Count = 1 }]
        };

        await _service.AddAsync(dto);

        var orders = await _orderRepo.GetFilteredAsync(filterBy: o => o.TableId == table.Id);
        Assert.That(orders, Has.Count.EqualTo(1));
        Assert.That(orders[0].TotalPrice, Is.EqualTo(5.0m)); // corrected
    }


    //[Test]
    //public async Task AddAsync_ShouldFail_WhenTableAccessDenied()
    //{
    //    var product = new Product { Name = "Cross Juice", BusinessId = 1, CategoryId = 1 };
    //    await _productRepo.AddAsync(product);

    //    var menuItem = new MenuItem { ProductId = product.Id, PlaceId = 1, Price = 3.0m, IsAvailable = true };
    //    await _menuItemRepo.AddAsync(menuItem);

    //    var table = new Table { Label = "T3", PlaceId = 2, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 }; // mismatched place
    //    await _tableRepo.AddAsync(table);

    //    var guest = new GuestSession { TableId = table.Id, Token = "cross-guest" };
    //    await _guestSessionRepo.AddAsync(guest);

    //    _mockUser.OverrideGuest("cross-guest");

    //    var dto = new UpsertOrderDto
    //    {
    //        TableId = table.Id,
    //        TotalPrice = 3.0m,
    //        Items =
    //    [
    //        new() { MenuItemId = menuItem.Id, Count = 1 }
    //    ]
    //    };

    //    var ex = Assert.ThrowsAsync<TableAccessDeniedException>(() => _service.AddAsync(dto));
    //    Assert.That(ex, Is.Not.Null);
    //}

    [Test]
    public async Task UpdateAsync_ShouldAllowGuest_WhenOrderIsCancelled()
    {
        // Arrange
        var table = new Table { Label = "U1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "guest-cancel");
        await _guestSessionRepo.AddAsync(guest); // ✅ This is missing

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.cancelled,
            GuestSessionId = guest.Id,
            Products =
        [
            new() { MenuItemId = 1, Count = 1, Price = 2.5m }
        ]
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("guest-cancel");

        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            TotalPrice = 2.5m,
            Items = [new() { MenuItemId = 1, Count = 1 }]
        };

        // Act
        await _service.UpdateAsync(order.Id, dto);

        // Assert
        var updated = await _orderRepo.GetByIdAsync(order.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Products, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_ForGuest_WhenOrderIsApproved()
    {
        // Arrange
        var table = new Table { Label = "U1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "guest-locked");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.delivered,
            GuestSessionId = guest.Id,
            Products = []
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("guest-locked");

        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            TotalPrice = 1.0m,
            Items = []
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.UpdateAsync(order.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldAllowStaff_WhenOrderIsDelivered()
    {
        // Arrange
        var table = new Table { Label = "U1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.delivered,
            Products = [new ProductPerOrder { MenuItemId = 1, Count = 1, Price = 3.0m }]
        };
        await _orderRepo.AddAsync(order);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.manager));

        var dto = new UpsertOrderDto
        {
            TableId = table.Id,
            TotalPrice = 3.0m,
            Items = [new() { MenuItemId = 1, Count = 1 }]
        };

        // Act
        await _service.UpdateAsync(order.Id, dto);

        // Assert
        var updated = await _orderRepo.GetByIdAsync(order.Id);
        Assert.That(updated!.Products, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task DeleteAsync_ShouldDelete_WhenCancelledAndAuthorized()
    {
        var table = new Table { Label = "D1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table, token: "delete-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            GuestSessionId = guest.Id,
            Status = OrderStatus.cancelled,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 1.5m }]
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("delete-guest");

        await _service.DeleteAsync(order.Id);

        var exists = await _orderRepo.ExistsAsync(o => o.Id == order.Id);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenNotCancelled()
    {
        var table = new Table { Label = "D2", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table, token: "fail-delete-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            GuestSessionId = guest.Id,
            Status = OrderStatus.created
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("fail-delete-guest");

        var ex = Assert.ThrowsAsync<AppValidationException>(() => _service.DeleteAsync(order.Id));
        Assert.That(ex!.Message, Does.Contain("cancelled"));
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenGuestUnauthorized()
    {
        var table = new Table {Label = "D-Unauthorized", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table, token: "real-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            GuestSessionId = guest.Id,
            Status = OrderStatus.cancelled
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("fake-token");

        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(() => _service.DeleteAsync(order.Id));
        Assert.That(ex, Is.Not.Null);
    }


    [Test]
    public async Task UpdateStatusAsync_ShouldUpdateStatus_ForValidGuestChange()
    {
        var table = new Table {Label = "S1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table, token: "status-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            GuestSessionId = guest.Id,
            Status = OrderStatus.delivered
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("status-guest");

        var dto = new UpdateOrderStatusDto
        {
            Status = OrderStatus.payment_requested,
            PaymentType = PaymentType.cash
        };

        await _service.UpdateStatusAsync(order.Id, dto);

        var updated = await _orderRepo.GetByIdAsync(order.Id);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.Status, Is.EqualTo(OrderStatus.payment_requested));
            Assert.That(updated.PaymentType, Is.EqualTo(PaymentType.cash));
        });
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldFail_WhenGuestUnauthorizedTransition()
    {
        var table = new Table {Label = "S2", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table, token: "guest-unauth-status");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            GuestSessionId = guest.Id,
            Status = OrderStatus.created
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("guest-unauth-status");

        var dto = new UpdateOrderStatusDto { Status = OrderStatus.delivered };

        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.UpdateStatusAsync(order.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldAllowGuest_ToRequestPayment_WhenDelivered()
    {
        var table = new Table { Label = "GB1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);
        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "token-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.delivered,
            GuestSessionId = guest.Id,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 2.0m }]
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("token-guest");

        var update = new UpdateOrderStatusDto { Status = OrderStatus.payment_requested };
        await _service.UpdateStatusAsync(order.Id, update);

        var updated = await _orderRepo.GetByIdAsync(order.Id);
        Assert.That(updated!.Status, Is.EqualTo(OrderStatus.payment_requested));
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldFailGuest_IfStatusInvalid()
    {
        var table = new Table { Label = "GB1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);
        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "guest-reject");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.delivered,
            GuestSessionId = guest.Id,
            Products = []
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("guest-reject");

        var update = new UpdateOrderStatusDto { Status = OrderStatus.created }; // Invalid transition

        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.UpdateStatusAsync(order.Id, update));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldAllowStaff_UpdateToDelivered()
    {
        var table = new Table { Label = "GB1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);
        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.created,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 3.0m }]
        };
        await _orderRepo.AddAsync(order);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.manager));

        var update = new UpdateOrderStatusDto { Status = OrderStatus.delivered };
        await _service.UpdateStatusAsync(order.Id, update);

        var updated = await _orderRepo.GetByIdAsync(order.Id);
        Assert.That(updated!.Status, Is.EqualTo(OrderStatus.delivered));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenSkipValidationTrue()
    {
        var table = new Table {Label = "GB1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.created,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 2.0m }]
        };
        await _orderRepo.AddAsync(order);

        var result = await _service.GetByIdAsync(order.Id, skipValidation: true);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo(order.Id));
            Assert.That(result.Items, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void GetByIdAsync_ShouldThrow_WhenOrderNotFound()
    {
        var ex = Assert.ThrowsAsync<OrderNotFoundException>(() => _service.GetByIdAsync(-1, skipValidation: true));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldFail_WhenAccessDenied()
    {
        var table = new Table {Label = "GB2", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "wrong-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            GuestSessionId = guest.Id,
            Status = OrderStatus.created
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("wrong-token"); // mismatched token

        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(() => _service.GetByIdAsync(order.Id, skipValidation: false));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetCurrentOrdersByTableLabelAsync_ShouldReturnOrders_WhenAuthorized()
    {
        var table = new Table {Label = "TBL123", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table: table);
        await _guestSessionRepo.AddAsync(guest);

        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.owner);
        _mockUser.Override(staff);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.created,
            GuestSessionId = guest.Id,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 4.5m }]
        };
        await _orderRepo.AddAsync(order);

        var result = await _service.GetCurrentOrdersByTableLabelAsync("TBL123");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(order.Id));
    }

    //[Test]
    //public async Task GetCurrentOrdersByTableLabelAsync_ShouldFail_WhenUnauthorized()
    //{
    //    var table = new Table { Label = "UN1", PlaceId = 2, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
    //    await _tableRepo.AddAsync(table);

    //    var order = new Order { TableId = table.Id, Status = OrderStatus.created };
    //    await _orderRepo.AddAsync(order);

    //    _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1)); // wrong place

    //    var ex = Assert.ThrowsAsync<TableAccessDeniedException>(() => _service.GetCurrentOrdersByTableLabelAsync("UN1"));
    //    Assert.That(ex, Is.Not.Null);
    //}


    [Test]
    public async Task GetAllActiveOrdersByPlaceIdGroupedAsync_ShouldReturnGrouped_WhenAuthorized()
    {
        var table = new Table {Label = "TGR1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.owner);
        _mockUser.Override(staff);

        var order1 = new Order { TableId = table.Id, Status = OrderStatus.created };
        var order2 = new Order { TableId = table.Id, Status = OrderStatus.delivered };
        await _orderRepo.AddAsync(order1);
        await _orderRepo.AddAsync(order2);

        var result = await _service.GetAllActiveOrdersByPlaceIdGroupedAsync(placeId: 1, page: 1, size: 1);

        Assert.That(result.Items, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(result.Items.SelectMany(g => g.Orders), Has.Some.Matches<OrderDto>(o => o.Id == order1.Id || o.Id == order2.Id));
    }

    [Test]
    public async Task GetAllByBusinessIdAsync_ShouldReturnOrders()
    {
        var table = new Table {Label = "BGR1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.owner);
        _mockUser.Override(staff);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.closed,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 5.0m }]
        };
        await _orderRepo.AddAsync(order);

        var result = await _service.GetAllByBusinessIdAsync(businessId: 1);

        Assert.That(result, Has.Count.GreaterThan(0));
        Assert.That(result[0].Orders, Has.Some.Matches<OrderDto>(o => o.Id == order.Id));
    }

    [Test]
    public async Task GetActiveTableOrdersForUserAsync_ShouldReturnOrders_WhenGuestTokenValid()
    {
        var table = new Table { Label = "T-GA1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "guest-orders");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.created,
            GuestSessionId = guest.Id,
            Products = [new() { MenuItemId = 1, Count = 2, Price = 3.0m }]
        };
        await _orderRepo.AddAsync(order);

        _mockUser.OverrideGuest("guest-orders");

        var result = await _service.GetActiveTableOrdersForUserAsync();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(order.Id));
    }

    [Test]
    public async Task GetAllClosedOrdersByPlaceIdAsync_ShouldReturnPaginatedResult()
    {
        var table = new Table {Label = "T-CL1", PlaceId = 1, Status = TableStatus.occupied, Width = 100, Height = 100, X = 100, Y = 100 };
        await _tableRepo.AddAsync(table);

        var staff = TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1, role: EmployeeRole.owner);
        _mockUser.Override(staff);

        var closedOrder = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.closed,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 4.0m }]
        };
        await _orderRepo.AddAsync(closedOrder);

        var result = await _service.GetAllClosedOrdersByPlaceIdAsync(placeId: 1, page: 1, size: 1);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Some.Matches<OrderDto>(o => o.Id == closedOrder.Id));
            Assert.That(result.Total, Is.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldFail_WhenGuestNotAuthorized()
    {
        // Table belongs to place 2, but guest is tied to another table
        var table = new Table
        {
            Label = "XG123",
            PlaceId = 2,
            Status = TableStatus.occupied,
            Width = 100,
            Height = 100,
            X = 50,
            Y = 50
        };
        await _tableRepo.AddAsync(table);

        var guest = TestDataFactory.CreateValidGuestSession(table: table, token: "unauthorized-guest");
        await _guestSessionRepo.AddAsync(guest);

        var order = new Order
        {
            TableId = table.Id,
            Status = OrderStatus.delivered,
            GuestSessionId = guest.Id,
            Products = [new() { MenuItemId = 1, Count = 1, Price = 3.0m }]
        };
        await _orderRepo.AddAsync(order);

        // Override with a guest not tied to this session/table
        _mockUser.OverrideGuest("fake-other-token");

        var dto = new UpdateOrderStatusDto { Status = OrderStatus.payment_requested };

        var ex = Assert.ThrowsAsync<UnauthorizedOrderAccessException>(() => _service.UpdateStatusAsync(order.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }

}
