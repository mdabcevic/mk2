using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace BartenderTests.UnitTests;

[TestFixture]
public class MenuItemServiceMutationTests
{
    private IMenuItemRepository _menuRepository;
    private IRepository<Place> _placeRepository;
    private IRepository<Product> _productRepository;
    private ILogger<MenuItemService> _logger;
    private ICurrentUserContext _currentUser;
    private IMapper _mapper;
    private IValidationService _validationService;
    private MenuItemService _menuService;

    [SetUp]
    public void SetUp()
    {
        _menuRepository = Substitute.For<IMenuItemRepository>();
        _placeRepository = Substitute.For<IRepository<Place>>();
        _productRepository = Substitute.For<IRepository<Product>>();
        _logger = Substitute.For<ILogger<MenuItemService>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _mapper = Substitute.For<IMapper>();
        _validationService = Substitute.For<IValidationService>();

        _menuService = new MenuItemService(
            _menuRepository,
            _placeRepository,
            _productRepository,
            _logger,
            _currentUser,
            _validationService,
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
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(product);
        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);
        _mapper.Map<MenuItem>(dto).Returns(new MenuItem { Id = 99 });

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _menuService.AddAsync(dto));
        await _menuRepository.Received(1).AddAsync(Arg.Any<MenuItem>());
        _mapper.Received(1).Map<MenuItem>(dto);
    }

    [Test]
    public async Task AddAsync_InvalidPrice_ThrowsValidationException()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto(price: -5);
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(TestDataFactory.CreateValidProduct(dto.ProductId));
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(() => _menuService.AddAsync(dto));

        Assert.That(ex!.Message, Does.Contain("greater than zero"));

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _productRepository.Received(1).GetByIdAsync(dto.ProductId, true);
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
    }

    [Test]
    public async Task AddAsync_ExistingMenuItem_ThrowsConflictException()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(TestDataFactory.CreateValidProduct());
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(true); // duplicate

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(() => _menuService.AddAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists"));

        await _menuRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task AddAsync_UnauthorizedUser_ThrowsUnauthorizedPlaceAccessException()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        var staff = TestDataFactory.CreateValidStaff(placeid: 999); // different place

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _validationService.VerifyUserPlaceAccess(dto.PlaceId, staff).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _menuService.AddAsync(dto));
        Assert.That(ex.Message, Does.Contain("Access").IgnoreCase);
        Assert.That(ex.Message, Does.Contain("denied").IgnoreCase);

        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _productRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), true);
        await _placeRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
    }

    [Test]
    public async Task AddAsync_ProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1);
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: 1));
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns((Product?)null);
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _menuService.AddAsync(dto));
        Assert.That(ex.Message, Does.Contain($"Product with id {dto.ProductId}"));

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _productRepository.Received(1).GetByIdAsync(dto.ProductId, true);
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
    }

    [Test]
    public async Task AddAsync_PlaceDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertMenuItemDto();
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff());
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(false);
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _menuService.AddAsync(dto));

        Assert.That(ex.Message, Does.Contain($"Place with id {dto.PlaceId}"));

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _productRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), true);
        await _menuRepository.DidNotReceive().AddAsync(Arg.Any<MenuItem>());
        await _menuRepository.DidNotReceive().ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
    }

    [Test]
    public async Task AddMultipleAsync_AllValidItems_ReturnsEmptyFailureList()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>
        {
            TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 1, price: 2.0m),
            TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 2, price: 2.5m)
        };
        var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(currentUser);
        _validationService.VerifyUserPlaceAccess(1).Returns(true);

        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<int>(), true)
            .Returns(ci => TestDataFactory.CreateValidProduct(ci.Arg<int>()));

        _mapper.Map<MenuItem>(Arg.Any<UpsertMenuItemDto>()).Returns(ci =>
            new MenuItem
            {
                PlaceId = ci.Arg<UpsertMenuItemDto>().PlaceId,
                ProductId = ci.Arg<UpsertMenuItemDto>().ProductId
            });

        // Act
        var result = await _menuService.AddMultipleAsync(menuItems);

        // Assert
        Assert.That(result, Is.Empty);
        await _menuRepository.Received(1).AddMultipleAsync(Arg.Is<List<MenuItem>>(list => list.Count == 2));
        _mapper.Received(menuItems.Count).Map<MenuItem>(Arg.Any<UpsertMenuItemDto>());
    }

    [Test]
    public async Task AddMultipleAsync_AllInvalidItems_ThrowsConflictExceptionWithAllFailures()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>
        {
            TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 1, price: 2.0m),
            TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 2, price: 2.5m)
        };

        var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(currentUser);

        _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(true); // simulate duplicates
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<int>(), true)
            .Returns(ci => TestDataFactory.CreateValidProduct(ci.Arg<int>()));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(() => _menuService.AddMultipleAsync(menuItems));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Message, Does.Contain("Successfully added 0").IgnoreCase);
            Assert.That(ex.Data["AdditionalData"], Is.AssignableTo<List<FailedMenuItemDto>>());
            Assert.That((List<FailedMenuItemDto>)ex.Data["AdditionalData"], Has.Count.EqualTo(2));
        });

        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    [Test]
    public async Task AddMultipleAsync_SomeInvalidItems_ThrowsConflictExceptionWithPartialFailures()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>
        {
            TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 1, price: 2.0m),
            TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 2, price: 2.5m)
        };
        var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(currentUser);
        _validationService.VerifyUserPlaceAccess(1).Returns(true);

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(Arg.Any<int>(), true)
            .Returns(ci => TestDataFactory.CreateValidProduct(ci.Arg<int>()));

        // Simulate: first one is valid, second is duplicate
        // Match the second item (duplicate)
        _menuRepository.ExistsAsync(Arg.Is<Expression<Func<MenuItem, bool>>>(expr =>
            expr.Compile().Invoke(new MenuItem { PlaceId = 1, ProductId = 2 })
        )).Returns(true);

        // Match the first item (valid)
        _menuRepository.ExistsAsync(Arg.Is<Expression<Func<MenuItem, bool>>>(expr =>
            expr.Compile().Invoke(new MenuItem { PlaceId = 1, ProductId = 1 })
        )).Returns(false);

        _mapper.Map<MenuItem>(Arg.Any<UpsertMenuItemDto>())
            .Returns(ci => new MenuItem
            {
                PlaceId = ci.Arg<UpsertMenuItemDto>().PlaceId,
                ProductId = ci.Arg<UpsertMenuItemDto>().ProductId
            });

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(() => _menuService.AddMultipleAsync(menuItems));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.Message, Does.Contain("Successfully added 1"));
            Assert.That(ex.Data["AdditionalData"], Is.AssignableTo<List<FailedMenuItemDto>>());
            var failures = (List<FailedMenuItemDto>)ex.Data["AdditionalData"];
            Assert.That(failures.Count, Is.EqualTo(1));
            Assert.That(failures[0].MenuItem.ProductId, Is.EqualTo(2));
        });

        await _menuRepository.Received(1)
            .AddMultipleAsync(Arg.Is<List<MenuItem>>(list => list.Count == 1 && list.Any(m => m.ProductId == 1)));
    }

    //[Test]
    //public async Task AddMultipleAsync_ValidItems_AddThrows_ThrowsConflictExceptionWithGenericFailure()
    //{
    //    // Arrange
    //    var menuItems = new List<UpsertMenuItemDto>
    //{
    //    TestDataFactory.CreateValidUpsertMenuItemDto(placeId: 1, productId: 1, price: 2.0m)
    //};

    //    var currentUser = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
    //    _currentUser.GetCurrentUserAsync().Returns(currentUser);

    //    _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
    //    _productRepository.GetByIdAsync(Arg.Any<int>(), true).Returns(TestDataFactory.CreateValidProduct());
    //    _menuRepository.ExistsAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(false);

    //    _mapper.Map<MenuItem>(Arg.Any<UpsertMenuItemDto>())
    //        .Returns(ci => new MenuItem
    //        {
    //            PlaceId = ci.Arg<UpsertMenuItemDto>().PlaceId,
    //            ProductId = ci.Arg<UpsertMenuItemDto>().ProductId
    //        });

    //    _menuRepository.When(r => r.AddMultipleAsync(Arg.Any<List<MenuItem>>()))
    //        .Do(x => throw new Exception("DB error"));

    //    // Act & Assert
    //    var ex = Assert.ThrowsAsync<ConflictException>(() => _menuService.AddMultipleAsync(menuItems));

    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(ex!.Message, Does.Contain("Successfully added 0"));
    //        Assert.That(ex.Data["AdditionalData"], Is.AssignableTo<List<FailedMenuItemDto>>());
    //        var failures = (List<FailedMenuItemDto>)ex.Data["AdditionalData"];
    //        Assert.That(failures.Count, Is.EqualTo(1));
    //    });

    //    await _menuRepository.Received(1).AddMultipleAsync(Arg.Any<List<MenuItem>>());
    //}

    [Test]
    public async Task AddMultipleAsync_EmptyInputList_ReturnsImmediately()
    {
        // Arrange
        var menuItems = new List<UpsertMenuItemDto>();
        var currentUser = TestDataFactory.CreateValidStaff();
        _currentUser.GetCurrentUserAsync().Returns(currentUser);

        // Act
        var result = await _menuService.AddMultipleAsync(menuItems);

        // Assert
        Assert.That(result, Is.Empty);
        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }


    [Test]
    public async Task CopyMenuAsync_TargetPlaceNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var fromPlaceId = 1;
        var toPlaceId = 2;
        var fromPlace = TestDataFactory.CreateValidPlace(fromPlaceId);
        var toPlace = TestDataFactory.CreateValidPlace(toPlaceId);

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(callInfo =>
        {
            var predicate = callInfo.Arg<Expression<Func<Place, bool>>>();
            var compiled = predicate.Compile();

            if (compiled(fromPlace)) return true;
            if (compiled(toPlace)) return false;

            return false;
        });

        // Act & Assert
        var ex = Assert.ThrowsAsync<PlaceNotFoundException>(() => _menuService.CopyMenuAsync(fromPlaceId, toPlaceId));

        Assert.That(ex!.Message, Does.Contain($"Place with ID {toPlaceId}"));

        await _placeRepository.Received(2).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    [Test]
    public async Task CopyMenuAsync_FromPlaceNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var fromPlaceId = 1;
        var toPlaceId = 2;
        _placeRepository.ExistsAsync(p => p.Id == fromPlaceId).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<PlaceNotFoundException>(() => _menuService.CopyMenuAsync(fromPlaceId, toPlaceId));

        Assert.That(ex!.Message, Does.Contain($"Place with ID {fromPlaceId}"));

        await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
        await _menuRepository.DidNotReceive().AddMultipleAsync(Arg.Any<List<MenuItem>>());
    }

    //[Test]
    //public async Task CopyMenuAsync_SuccessfullyCopiesItems()
    //{
    //    // Arrange
    //    var fromPlaceId = 1;
    //    var toPlaceId = 2;

    //    var staff = TestDataFactory.CreateValidStaff(placeid: toPlaceId, role: EmployeeRole.admin);
    //    _currentUser.GetCurrentUserAsync().Returns(staff);

    //    // Simulate existence of both places
    //    _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);

    //    // Source menu items
    //    var sourceMenuItems = new List<MenuItem>
    //{
    //    TestDataFactory.CreateValidMenuItem(id: 1, placeId: fromPlaceId, productId: 1),
    //    TestDataFactory.CreateValidMenuItem(id: 2, placeId: fromPlaceId, productId: 2)
    //};

    //    // Simulate .Query() returning source items
    //    _menuRepository.Query().Returns(sourceMenuItems.AsQueryable());

    //    // Act
    //    await _menuService.CopyMenuAsync(fromPlaceId, toPlaceId);

    //    // Assert: ensure new items are copied with correct PlaceId and ProductId
    //    await _menuRepository.Received(1).AddMultipleAsync(
    //        Arg.Is<List<MenuItem>>(list =>
    //            list.Count == 2 &&
    //            list.All(i => i.PlaceId == toPlaceId) &&
    //            list.Select(i => i.ProductId).OrderBy(x => x).SequenceEqual(new[] { 1, 2 })
    //        )
    //    );
    //}

    //[Test]
    //public void CopyMenuAsync_UnexpectedException_ThrowsUnknownError()
    //{
    //    // Arrange
    //    _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
    //    _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(role: EmployeeRole.admin));
    //    _menuRepository.Query().Throws(new ApplicationException("Unexpected DB failure"));

    //    // Act & Assert
    //    var ex = Assert.ThrowsAsync<ApplicationException>(() => _menuService.CopyMenuAsync(1, 2));

    //    Assert.That(ex!.Message, Does.Contain("Unexpected DB failure").IgnoreCase);
    //}

    [Test]
    public async Task UpdateAsync_ValidDto_UpdatesItemSuccessfully()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingMenuItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId, name: "Espresso");

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns(existingMenuItem);

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(existingMenuItem.Product);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _menuService.UpdateAsync(dto));

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
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Returns(existingMenuItem);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(existingMenuItem.Product);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _menuService.UpdateAsync(dto));

        await _menuRepository.Received(1).GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.Received(1).UpdateAsync(existingMenuItem);
        _mapper.Received(1).Map(dto, existingMenuItem);
    }

    [Test]
    public async Task UpdateAsync_MenuItemNotFound_ThrowsMenuItemNotFoundException()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns((MenuItem?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MenuItemNotFoundException>(() => _menuService.UpdateAsync(dto));

        Assert.That(ex!.Message, Does.Contain($"not found"));

        await _menuRepository.Received(1).GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_UnauthorizedAccess_ThrowsUnauthorizedPlaceAccessException()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var otherPlaceId = dto.PlaceId + 1; // Simulate cross-place access
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: otherPlaceId));

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() => _menuService.UpdateAsync(dto));

        Assert.That(ex!.Message, Does.Contain("Access").IgnoreCase);
        Assert.That(ex!.Message, Does.Contain("denied").IgnoreCase);

        await _menuRepository.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_InvalidPrice_ThrowsValidationException()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto(price: -5);
        var item = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _validationService.VerifyUserPlaceAccess(item.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(item);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(item.Product);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(() => _menuService.UpdateAsync(dto));

        Assert.That(ex!.Message, Does.Contain("Price must be greater than zero"));

        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: dto.PlaceId));
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(existingItem);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns((Product?)null); // Simulate missing product

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _menuService.UpdateAsync(dto));

        Assert.That(ex!.Message, Does.Contain($"Product with id {dto.ProductId}"));

        await _productRepository.Received(1).GetByIdAsync(dto.ProductId, true);
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateAsync_SharedProductCrossBusiness_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto = TestDataFactory.CreateUpsertMenuItemDto();
        var existingItem = TestDataFactory.CreateValidMenuItem(1, dto.PlaceId, dto.ProductId);
        var product = TestDataFactory.CreateValidProduct(dto.ProductId, businessId: 99); // Mismatched business ID
        var user = TestDataFactory.CreateValidStaff(placeid: dto.PlaceId, businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(user);
        _validationService.VerifyUserPlaceAccess(dto.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(existingItem);
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _productRepository.GetByIdAsync(dto.ProductId, true).Returns(product);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _menuService.UpdateAsync(dto));

        Assert.That(ex!.Message, Does.Contain($"Access to product with id {dto.ProductId} denied"));

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
        _validationService.VerifyUserPlaceAccess(menuItem.PlaceId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(menuItem);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _menuService.UpdateItemAvailabilityAsync(placeId, productId, newAvailability));

        Assert.That(menuItem.IsAvailable, Is.EqualTo(newAvailability));
        await _menuRepository.Received(1).UpdateAsync(menuItem);
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_CrossBusinessAccessDenied_ThrowsUnauthorizedPlaceAccessException()
    {
        // Arrange
        var placeId = 1;
        var productId = 2;
        var user = TestDataFactory.CreateValidStaff(placeid: 99); // different place (unauthorized)
        _currentUser.GetCurrentUserAsync().Returns(user);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() =>
            _menuService.UpdateItemAvailabilityAsync(placeId, productId, true));

        Assert.That(ex!.Message, Does.Contain("access").IgnoreCase);

        await _menuRepository.DidNotReceive().GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>());
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_MenuItemNotFound_ThrowsMenuItemNotFoundException()
    {
        // Arrange
        var placeId = 1;
        var productId = 2;
        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns((MenuItem?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MenuItemNotFoundException>(() =>
            _menuService.UpdateItemAvailabilityAsync(placeId, productId, true));

        Assert.That(ex!.Message, Does.Contain($"not found"));

        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task UpdateItemAvailabilityAsync_UnexpectedError_ThrowsException()
    {
        // Arrange
        var placeId = 1;
        var productId = 3;

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Throws(new Exception("Database is down"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _menuService.UpdateItemAvailabilityAsync(placeId, productId, true));

        Assert.That(ex!.Message, Does.Contain("Database is down"));
        await _menuRepository.DidNotReceive().UpdateAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task DeleteAsync_ValidRequest_DeletesItemSuccessfully()
    {
        // Arrange
        var placeId = 1;
        var productId = 5;
        var menuItem = TestDataFactory.CreateValidMenuItem(1, placeId, productId);

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns(menuItem);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _menuService.DeleteAsync(placeId, productId));
        Assert.That(menuItem.DeletedAt, Is.Not.Null);
        await _menuRepository.Received(1).UpdateAsync(menuItem);
    }

    [Test]
    public async Task DeleteAsync_MenuItemNotFound_ThrowsMenuItemNotFoundException()
    {
        // Arrange
        var placeId = 1;
        var productId = 5;

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>()).Returns((MenuItem?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MenuItemNotFoundException>(() =>
            _menuService.DeleteAsync(placeId, productId));

        Assert.That(ex!.Message, Does.Contain($"not found"));
        await _menuRepository.DidNotReceive().DeleteAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task DeleteAsync_UnauthorizedAccess_ThrowsUnauthorizedPlaceAccessException()
    {
        // Arrange
        var placeId = 99;
        var productId = 1;
        var otherPlaceStaff = TestDataFactory.CreateValidStaff(placeid: 1); // placeId mismatch

        _currentUser.GetCurrentUserAsync().Returns(otherPlaceStaff);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedPlaceAccessException>(() =>
            _menuService.DeleteAsync(placeId, productId));

        Assert.That(ex!.Message, Does.Contain("access").IgnoreCase);

        await _menuRepository.DidNotReceive().DeleteAsync(Arg.Any<MenuItem>());
    }

    [Test]
    public async Task DeleteAsync_UnexpectedError_ThrowsException()
    {
        // Arrange
        var placeId = 1;
        var productId = 1;

        _currentUser.GetCurrentUserAsync().Returns(TestDataFactory.CreateValidStaff(placeid: placeId));
        _validationService.VerifyUserPlaceAccess(placeId).Returns(true);
        _menuRepository.GetByKeyAsync(Arg.Any<Expression<Func<MenuItem, bool>>>())
            .Throws(new Exception("DB crashed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _menuService.DeleteAsync(placeId, productId));

        Assert.That(ex!.Message, Does.Contain("DB crashed"));

        await _menuRepository.DidNotReceive().DeleteAsync(Arg.Any<MenuItem>());
    }
}
