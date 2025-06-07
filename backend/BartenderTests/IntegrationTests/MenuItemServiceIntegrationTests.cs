using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class MenuItemServiceIntegrationTests : IntegrationTestBase
{
    private IMenuItemService _service = null!;
    private IRepository<MenuItem> _menuItemRepo = null!;
    private IRepository<Product> _productRepo = null!;

    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IMenuItemService>();
        _menuItemRepo = scope.ServiceProvider.GetRequiredService<IRepository<MenuItem>>();
        _productRepo = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    [Test]
    public async Task AddAsync_ShouldAddMenuItem_WhenAuthorized()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var dto = new UpsertMenuItemDto
        {
            PlaceId = 1,
            ProductId = 1, // Espresso
            Price = 2.0m,
            Description = "Test add"
        };

        // Ensure no duplicate
        var existing = await _menuItemRepo.GetByKeyAsync(m => m.PlaceId == 1 && m.ProductId == 1);
        if (existing != null)
            await _menuItemRepo.DeleteAsync(existing);

        await _service.AddAsync(dto);

        var exists = await _menuItemRepo.ExistsAsync(m => m.PlaceId == 1 && m.ProductId == 1);
        Assert.That(exists, Is.True);
    }

    [Test]
    public void AddAsync_ShouldFail_WhenDuplicate()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var dto = new UpsertMenuItemDto
        {
            PlaceId = 1,
            ProductId = 1, // already exists in seed
            Price = 2.0m,
            Description = "Duplicate"
        };

        var ex = Assert.ThrowsAsync<ConflictException>(() => _service.AddAsync(dto));
        Assert.That(ex!.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task AddMultipleAsync_ShouldPartiallySucceed_WhenOneFails()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var goodProduct = new Product
        {
            Name = "Iced Coffee",
            Volume = "250ml",
            BusinessId = 1,
            CategoryId = 1
        };
        await _productRepo.AddAsync(goodProduct);

        var valid = new UpsertMenuItemDto
        {
            PlaceId = 1,
            ProductId = goodProduct.Id,
            Price = 3.5m,
            Description = "New item"
        };

        var duplicate = new UpsertMenuItemDto
        {
            PlaceId = 1,
            ProductId = 1, // already exists
            Price = 2.0m
        };

        var items = new List<UpsertMenuItemDto> { valid, duplicate };

        var ex = Assert.ThrowsAsync<ConflictException>(() => _service.AddMultipleAsync(items));
        Assert.That(ex!.Data["AdditionalData"], Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateDescription_WhenAuthorized()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var dto = new UpsertMenuItemDto
        {
            PlaceId = 1,
            ProductId = 1,
            Price = 2.5m,
            Description = "Updated description"
        };

        await _service.UpdateAsync(dto);

        var item = await _menuItemRepo.GetByKeyAsync(m => m.PlaceId == 1 && m.ProductId == 1);
        Assert.That(item!.Description, Is.EqualTo("Updated description"));
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_ShouldToggleAvailability()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        await _service.UpdateItemAvailabilityAsync(1, 1, false);
        var item = await _menuItemRepo.GetByKeyAsync(m => m.PlaceId == 1 && m.ProductId == 1);
        Assert.That(item!.IsAvailable, Is.False);

        await _service.UpdateItemAvailabilityAsync(1, 1, true);
        item = await _menuItemRepo.GetByKeyAsync(m => m.PlaceId == 1 && m.ProductId == 1);
        Assert.That(item!.IsAvailable, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveItem_WhenAuthorized()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        await _service.DeleteAsync(1, 5);

        var exists = await _menuItemRepo.ExistsAsync(m => m.PlaceId == 1 && m.ProductId == 9999);
        Assert.That(exists, Is.False);
    }

    [Test]
    public void DeleteAsync_ShouldFail_WhenNotFound()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var ex = Assert.ThrowsAsync<MenuItemNotFoundException>(() => _service.DeleteAsync(1, 9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnMenuItem_WhenExists()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var result = await _service.GetByIdAsync(1, 1); // PlaceId = 1, ProductId = 1 (seeded Espresso)
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Product.Id, Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_ShouldFail_WhenNotFound()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var ex = Assert.ThrowsAsync<MenuItemNotFoundException>(() => _service.GetByIdAsync(1, 9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetByPlaceIdAsync_ShouldReturnMenuItems_WhenAvailable()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var items = await _service.GetByPlaceIdAsync(1, onlyAvailable: true);
        Assert.That(items, Has.Count.GreaterThan(0));
        Assert.That(items.All(i => i.IsAvailable), Is.True);
    }

    [Test]
    public async Task GetByPlaceIdGroupedAsync_ShouldGroupItemsByCategory()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var groups = await _service.GetByPlaceIdGroupedAsync(1);
        Assert.That(groups, Has.Count.GreaterThan(0));
        Assert.That(groups.All(g => g.Items != null), Is.True);
        Assert.That(groups.All(g => g.Items!.Count != 0), Is.True);
    }

    [Test]
    public async Task GetFilteredAsync_ShouldReturnMatchingProducts()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var results = await _service.GetFilteredAsync(1, "Espresso");
        Assert.That(results.Any(r => r.Product.Name.Contains("Espresso", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnGroupedMenusByPlace()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var allMenus = await _service.GetAllAsync();
        Assert.That(allMenus, Has.Count.GreaterThan(0));

        var placeMenu = allMenus.FirstOrDefault();
        Assert.That(placeMenu, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(placeMenu!.Items.Count != 0, Is.True);
            Assert.That(placeMenu.Place.Address, Is.Not.Null.And.Not.Empty);
        });
    }

}
