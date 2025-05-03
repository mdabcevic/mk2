using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace BartenderTests;

[TestFixture]
public class MenuItemServiceMutationTests
{
    private IRepository<MenuItem> _menuRepository;
    private IRepository<Place> _placeRepository;
    private IRepository<Product> _productRepository;
    private ILogger<MenuItemService> _logger;
    private ICurrentUserContext _currentUser;
    private IMapper _mapper;
    private MenuItemService _menuService;

    [SetUp]
    public void SetUp()
    {
        _menuRepository = Substitute.For<IRepository<MenuItem>>();
        _placeRepository = Substitute.For<IRepository<Place>>();
        _productRepository = Substitute.For<IRepository<Product>>();
        _logger = Substitute.For<ILogger<MenuItemService>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _mapper = Substitute.For<IMapper>();

        _menuService = new MenuItemService(
            _menuRepository,
            _placeRepository,
            _productRepository,
            _logger,
            _currentUser,
            _mapper);
    }

    [Test]
    public async Task AddAsync_ValidMenuItem_AddsSuccessfully()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        var user = TestDataFactory.CreateValidStaff(placeid: dto.PlaceId, businessid: 1);
        var product = TestDataFactory.CreateValidProduct(dto.ProductId);

        _currentUser.GetCurrentUserAsync().Returns(user);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(product);
        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);
        _mapper.Map<MenuItem>(dto).Returns(new MenuItem { Id = 99 });

        // Act
        var result = await _menuService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Error, Is.Null);
        });

        await _menuRepository.Received(1).AddAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task AddAsync_InvalidPrice_ReturnsValidationError()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto(price: -5);
        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(TestDataFactory.CreateValidProduct(dto.ProductId));
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());

        // Act
        var result = await _menuService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error, Does.Contain("greater than zero"));
        });
        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        //await _productRepository.Received(1).GetByIdAsync(Arg.Any<int>());
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
    }

    [Test]
    public async Task AddAsync_ExistingMenuItem_ReturnsConflictError()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(true);
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(TestDataFactory.CreateValidProduct());

        // Act
        var result = await _menuService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
            Assert.That(result.Error, Does.Contain("already exists"));
        });
        await _menuRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task AddAsync_UnauthorizedUser_ReturnsUnauthorizedError()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        var staff = TestDataFactory.CreateValidStaff(placeid: 999); // different place
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _menuService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Does.Contain("Cross-business access"));
        });
        await _currentUser.Received(1).GetCurrentUserAsync();
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _productRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), true);
        await _placeRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
    }

    [Test]
    public async Task AddAsync_ProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns((Product?)null);

        // Act
        var result = await _menuService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Does.Contain("Product with id"));
        });

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _productRepository.Received(1).GetByIdAsync(dto.ProductId, true);
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
    }

    [Test]
    public async Task AddAsync_PlaceDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(false);

        // Act
        var result = await _menuService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Does.Contain("Place with id"));
        });

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _productRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), true);
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
    }

    [Test]
    public async Task AddMultipleAsync_AllValidItems_ReturnsOk()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>
    {
        new() { PlaceId = 1, ProductId = 1, Price = 2.0m, Description = "Strong", IsAvailable = true },
        new() { PlaceId = 1, ProductId = 2, Price = 2.5m, Description = "Mild", IsAvailable = true }
    };

        var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(currentUser);

        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<int>(), true).Returns(ci =>
            TestDataFactory.CreateValidProduct(ci.Arg<int>()));

        _mapper.Map<MenuItem>(Arg.Any<UpsertMenuItemDto>()).Returns(ci =>
            new MenuItem { PlaceId = ci.Arg<UpsertMenuItemDto>().PlaceId, ProductId = ci.Arg<UpsertMenuItemDto>().ProductId });

        // Act
        var result = await _menuService.AddMultipleAsync(menuItems);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Empty);
        });

        await _menuRepository.Received(1).AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    [Test]
    public async Task AddMultipleAsync_AllInvalidItems_ReturnsFailWithAllErrors()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>
    {
        new() { PlaceId = 1, ProductId = 1, Price = 2.0m },
        new() { PlaceId = 1, ProductId = 2, Price = 2.0m }
    };

        var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(currentUser);

        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(true); // simulate duplicates
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<int>(), true).Returns(ci =>
            TestDataFactory.CreateValidProduct(ci.Arg<int>()));

        // Act
        var result = await _menuService.AddMultipleAsync(menuItems);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
            Assert.That(result.Data, Has.Count.EqualTo(2));
        });

        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    //[Test]
    //public async Task AddMultipleAsync_SomeInvalidItems_ReturnsPartialSuccess()
    //{
    //    // Arrange
    //    var menuItems = new List<UpsertMenuItemDto>
    //{
    //    new() { PlaceId = 1, ProductId = 1, Price = 2.0m }, // valid
    //    new() { PlaceId = 1, ProductId = 2, Price = 2.0m }  // duplicate
    //};

    //    var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
    //    _currentUser.GetCurrentUserAsync().Returns(currentUser);

    //    _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
    //    _productRepository.GetByIdAsync(Arg.Any<int>(), true)
    //        .Returns(ci => TestDataFactory.CreateValidProduct(ci.Arg<int>()));

    //    _menuRepository.ExistsAsync(Arg.Do<Expression<Func<MenuItem, bool>>>(expr =>
    //    {
    //        // Simulate first one is not duplicate, second is
    //        var invoked = expr.Compile();
    //        var menuItem = new MenuItem { PlaceId = 1, ProductId = 2 };
    //        return invoked(menuItem);
    //    })).Returns(true); // causes second to be considered duplicate

    //    _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
    //        .Returns(ci => {
    //            var productId = Expression.Lambda<Func<MenuItem, bool>>(ci.Arg<Expression<Func<MenuItem, bool>>>().Body, ci.Arg<Expression<Func<MenuItem, bool>>>().Parameters[0])
    //                .Compile().Invoke(new MenuItem { ProductId = 2 });
    //            return productId;
    //        });

    //    _mapper.Map<MenuItem>(Arg.Any<UpsertMenuItemDto>()).Returns(ci =>
    //        new MenuItem { PlaceId = ci.Arg<UpsertMenuItemDto>().PlaceId, ProductId = ci.Arg<UpsertMenuItemDto>().ProductId });

    //    // Act
    //    var result = await _menuService.AddMultipleAsync(menuItems);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.False);
    //        Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
    //        Assert.That(result.Data, Has.Count.EqualTo(1));
    //        Assert.That(result.Data![0].MenuItem.ProductId, Is.EqualTo(2));
    //        Assert.That(result.Error, Does.Contain("Successfully added 1"));
    //    });

    //    await _menuRepository.Received(1).AddMultipleAsync(Arg.Is<List<MenuItem>>(l => l.Count == 1));
    //}

    [Test]
    public async Task AddMultipleAsync_ValidItems_AddThrows_ReturnsUnknownError()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>
    {
        new() { PlaceId = 1, ProductId = 1, Price = 2.0m }
    };

        var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(currentUser);

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<int>(), true).Returns(TestDataFactory.CreateValidProduct());

        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);

        _mapper.Map<MenuItem>(Arg.Any<UpsertMenuItemDto>())
            .Returns(ci => new MenuItem
            {
                PlaceId = ci.Arg<UpsertMenuItemDto>().PlaceId,
                ProductId = ci.Arg<UpsertMenuItemDto>().ProductId
            });

        _menuRepository.When(r => r.AddMultipleAsync(Arg.Any<List<MenuItem>>()))
            .Do(x => throw new Exception("DB error"));

        // Act
        var result = await _menuService.AddMultipleAsync(menuItems);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
            Assert.That(result.Error, Does.Contain("unexpected"));
        });

        await _menuRepository.Received(1).AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    [Test]
    public async Task CopyMenuAsync_SourcePlaceNotFound_ReturnsNotFound()
    {
        // Arrange
        var fromPlaceId = 100;
        var toPlaceId = 200;

        _placeRepository.ExistsAsync(p => p.Id == fromPlaceId).Returns(false);

        // Act
        var result = await _menuService.CopyMenuAsync(fromPlaceId, toPlaceId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Does.Contain($"Place with id {fromPlaceId} not found"));
        });

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    [Test]
    public async Task CopyMenuAsync_TargetPlaceNotFound_ReturnsNotFound()
    {
        // Arrange
        var fromPlaceId = 1;
        var toPlaceId = 2;
        var fromPlace = TestDataFactory.CreateValidPlace(fromPlaceId);
        var toPlace = TestDataFactory.CreateValidPlace(toPlaceId);

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>())
            .Returns(callInfo =>
    {
        var predicate = callInfo.Arg<Expression<Func<Place, bool>>>();
        var compiled = predicate.Compile();

        if (compiled(fromPlace)) return true;
        if (compiled(toPlace)) return false;

        return false;
    });

        // Act
        var result = await _menuService.CopyMenuAsync(fromPlaceId, toPlaceId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Does.Contain($"Place with id {toPlaceId} not found"));
        });

        await _placeRepository.Received(2).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    [Test]
    public async Task CopyMenuAsync_FromPlaceNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _placeRepository.ExistsAsync(p => p.Id == 1).Returns(false);

        // Act
        var result = await _menuService.CopyMenuAsync(1, 2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task CopyMenuAsync_UnexpectedException_ReturnsUnknownError()
    {
        // Arrange
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(role: EmployeeRole.admin));
        _menuRepository.Query().Throws(new ApplicationException("Unexpected DB failure"));

        // Act
        var result = await _menuService.CopyMenuAsync(1, 2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
            Assert.That(result.Error, Does.Contain("unexpected").IgnoreCase);
        });
    }

    [Test]
    public async Task UpdateAsync_ValidDto_UpdatesItemSuccessfully()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingMenuItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId, name: "Espresso");

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns(existingMenuItem);

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(existingMenuItem.Product);

        // Act
        var result = await _menuService.UpdateAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.errorType, Is.Null);
        });

        await _menuRepository.Received(1).GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.Received(1).UpdateAsync(existingMenuItem);
        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _productRepository.Received(1).GetByIdAsync(dto.ProductId, true);
        _mapper.Received(1).Map(dto, existingMenuItem);
    }

    [Test]
    public async Task UpdateAsync_ValidRequest_UpdatesMenuItemSuccessfully()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingMenuItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId, isAvailable: false);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns(existingMenuItem);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(existingMenuItem.Product);

        // Act
        var result = await _menuService.UpdateAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Error, Is.Null);
        });

        await _menuRepository.Received(1).GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.Received(1).UpdateAsync(existingMenuItem);
        _mapper.Received(1).Map(dto, existingMenuItem);
    }

    [Test]
    public async Task UpdateAsync_MenuItemNotFound_ReturnsNotFound()
    {
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns((MenuItem?)null);

        var result = await _menuService.UpdateAsync(dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });

        await _menuRepository.Received(1).GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_UnauthorizedAccess_ReturnsUnauthorized()
    {
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var otherPlaceId = dto.PlaceId + 1;
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: otherPlaceId)); // Different place

        var result = await _menuService.UpdateAsync(dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });

        await _menuRepository.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_InvalidPrice_ThrowsValidation()
    {
        var dto = TestDataFactory.CreateUpsertMenuItemDto(price: -5);
        var item = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId);
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(item);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(item.Product);

        var result = await _menuService.UpdateAsync(dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error, Does.Contain("Price must be greater than zero"));
        });

        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(existingItem);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns((Product?)null); // Simulate missing product

        // Act
        var result = await _menuService.UpdateAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Does.Contain("Product with id"));
        });

        await _productRepository.Received(1).GetByIdAsync(dto.ProductId, true);
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_SharedProductCrossBusiness_ReturnsUnauthorized()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId);
        var product = TestDataFactory.CreateValidProduct(dto.ProductId, businessId: 99); // Mismatched business ID
        var user = TestDataFactory.CreateValidStaff(placeid: dto.PlaceId, businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(user);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(existingItem);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(product);

        // Act
        var result = await _menuService.UpdateAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
            Assert.That(result.Error, Does.Contain("Access to product"));
        });

        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_ValidRequest_UpdatesAvailability()
    {
        // Arrange
        var placeId = 1;
        var productId = 10;
        var newAvailability = true;
        var menuItem = TestDataFactory.CreateValidMenuItem(1, placeId, productId, "Espresso", isAvailable: false);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(menuItem);

        // Act
        var result = await _menuService.UpdateItemAvailabilityAsync(placeId, productId, newAvailability);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(menuItem.IsAvailable, Is.EqualTo(newAvailability));
        });
        await _menuRepository.Received(1).UpdateAsync(menuItem);
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_CrossBusinessAccessDenied_ReturnsUnauthorized()
    {
        // Arrange
        var placeId = 1;
        var productId = 2;
        var user = TestDataFactory.CreateValidStaff(placeid: 99); // different place
        _currentUser.GetCurrentUserAsync().Returns(user);

        // Act
        var result = await _menuService.UpdateItemAvailabilityAsync(placeId, productId, true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });

        await _menuRepository.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_MenuItemNotFound_ReturnsNotFound()
    {
        // Arrange
        var placeId = 1;
        var productId = 2;
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns((MenuItem?)null);

        // Act
        var result = await _menuService.UpdateItemAvailabilityAsync(placeId, productId, true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_UnexpectedError_ReturnsUnknownError()
    {
        // Arrange
        var placeId = 1;
        var productId = 3;

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Throws(new Exception("Database is down"));

        // Act
        var result = await _menuService.UpdateItemAvailabilityAsync(placeId, productId, true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
        });
    }

    [Test]
    public async Task DeleteAsync_ValidRequest_DeletesItemSuccessfully()
    {
        // Arrange
        var placeId = 1;
        var productId = 5;
        var menuItem = TestDataFactory.CreateValidMenuItem(1, placeId, productId);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId, role: EmployeeRole.regular));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(menuItem);

        // Act
        var result = await _menuService.DeleteAsync(placeId, productId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Error, Is.Null);
        });

        await _menuRepository.Received(1).DeleteAsync(menuItem);
    }

    [Test]
    public async Task DeleteAsync_MenuItemNotFound_ReturnsNotFound()
    {
        // Arrange
        var placeId = 1;
        var productId = 5;

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns((MenuItem?)null);

        // Act
        var result = await _menuService.DeleteAsync(placeId, productId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Does.Contain("not found"));
        });

        await _menuRepository.DidNotReceive().DeleteAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task DeleteAsync_UnauthorizedAccess_ReturnsUnauthorized()
    {
        // Arrange
        var placeId = 99;
        var productId = 1;
        var otherPlaceStaff = TestDataFactory.CreateValidStaff(placeid: 1); // placeId mismatch

        _currentUser.GetCurrentUserAsync().Returns(otherPlaceStaff);

        // Act
        var result = await _menuService.DeleteAsync(placeId, productId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });

        await _menuRepository.DidNotReceive().DeleteAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task DeleteAsync_UnexpectedError_ReturnsUnknownError()
    {
        // Arrange
        var placeId = 1;
        var productId = 1;

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Throws(new Exception("DB crashed"));

        // Act
        var result = await _menuService.DeleteAsync(placeId, productId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
        });

        await _menuRepository.DidNotReceive().DeleteAsync(Arg.Any<MenuItem>());
    }
}
