using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Order;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
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
}
