using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.MenuItem;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace BartenderTests;

[TestFixture]
public class MenuItemServiceReadTests
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

        _menuService = new MenuItemService(_menuRepository, _placeRepository, _productRepository, _logger, _currentUser, _mapper);
    }

    //[Test]
    //public async Task GetByPlaceIdAsync_ValidPlaceId_ReturnsMenuItemsSortedByProductName()
    //{
    //    // Arrange
    //    var placeId = 1;
    //    var menuItems = new List<MenuItem>
    //    {
    //        new() { Id = 1, PlaceId = placeId, Product = new Product { Name = "Latte" } },
    //        new() { Id = 2, PlaceId = placeId, Product = new Product { Name = "Americano" } },
    //    };

    //    var expectedDtos = new List<MenuItemBaseDto>
    //    {
    //        TestDataFactory.CreateMenuItemBaseDto(2, 2, "Americano"),
    //        TestDataFactory.CreateMenuItemBaseDto(1, 1, "Latte")
    //    };

    //    _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
    //    _menuRepository.QueryIncluding(Arg.Any<Expression<Func<MenuItem, object>>>())
    //        .Returns(menuItems.AsQueryable());

    //    _mapper.Map<List<MenuItemBaseDto>>(Arg.Any<List<MenuItem>>()).Returns(expectedDtos);

    //    // Act
    //    var result = await _menuService.GetByPlaceIdAsync(placeId);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Has.Count.EqualTo(2));
    //    });
    //    Assert.That(result.Data![0].Product.Name, Is.EqualTo("Americano"));

    //    await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
    //    _menuRepository.Received(1).QueryIncluding(Arg.Any<Expression<Func<MenuItem, object>>>(), Arg.Any<Expression<Func<MenuItem, object>>>());
    //    _mapper.Received(1).Map<List<MenuItemBaseDto>>(Arg.Any<List<MenuItem>>());
    //}

    //[Test]
    //public async Task GetByPlaceIdAsync_OnlyAvailableTrue_ReturnsFilteredItems()
    //{
    //    // Arrange
    //    var placeId = 1;
    //    var menuItems = new List<MenuItem>
    //    {
    //        TestDataFactory.CreateMenuItem(1, placeId, "Espresso", isAvailable: true),
    //        TestDataFactory.CreateMenuItem(2, placeId, "Cappuccino", isAvailable: false)
    //    };

    //    var expectedDtos = new List<MenuItemBaseDto>
    //    {
    //        TestDataFactory.CreateMenuItemBaseDto(1, 1, "Espresso")
    //    };

    //    _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
    //    _menuRepository.QueryIncluding(Arg.Any<Expression<Func<MenuItem, object>>>())
    //        .Returns(menuItems.AsQueryable().Where(mi => mi.IsAvailable));

    //    _mapper.Map<List<MenuItemBaseDto>>(Arg.Any<List<MenuItem>>()).Returns(expectedDtos);

    //    // Act
    //    var result = await _menuService.GetByPlaceIdAsync(placeId, onlyAvailable: true);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Has.Count.EqualTo(1));
    //    });
    //    Assert.That(result.Data![0].Product.Name, Is.EqualTo("Espresso"));

    //    await _placeRepository.Received(1).ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>());
    //    _menuRepository.Received(1).QueryIncluding(Arg.Any<Expression<Func<MenuItem, object>>>(), Arg.Any<Expression<Func<MenuItem, object>>>());
    //    _mapper.Received(1).Map<List<MenuItemBaseDto>>(Arg.Any<List<MenuItem>>());
    //}

    [Test]
    public void GetByPlaceIdAsync_PlaceDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var placeId = 999;
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _menuService.GetByPlaceIdAsync(placeId));

        Assert.That(ex!.Message, Does.Contain($"Place with id {placeId}"));
    }

    [Test]
    public void GetByPlaceIdAsync_UnexpectedError_ThrowsException()
    {
        // Arrange
        var placeId = 1;
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>())
            .Throws(new Exception("DB Failure"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _menuService.GetByPlaceIdAsync(placeId));

        Assert.That(ex!.Message, Does.Contain("DB Failure"));
    }

    [Test]
    public void GetByPlaceIdGroupedAsync_PlaceNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var placeId = 99;
        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _menuService.GetByPlaceIdGroupedAsync(placeId));

        Assert.That(ex!.Message, Does.Contain($"Place with id {placeId}"));
    }

    [Test]
    public void GetByPlaceIdGroupedAsync_UnexpectedError_ThrowsException()
    {
        // Arrange
        var placeId = 1;

        _placeRepository.ExistsAsync(Arg.Any<Expression<Func<Place, bool>>>()).Returns(true);
        _menuRepository.QueryIncluding(Arg.Any<Expression<Func<MenuItem, object>>>(), Arg.Any<Expression<Func<MenuItem, object>>>()!)
            .Throws(new Exception("Database unreachable"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _menuService.GetByPlaceIdGroupedAsync(placeId));
        Assert.That(ex!.Message, Does.Contain("Database unreachable"));
    }

    [Test]
    public async Task GetByIdAsync_ValidIds_ReturnsExpectedMenuItem()
    {
        // Arrange
        var placeId = 1;
        var productId = 5;

        var menuItem = TestDataFactory.CreateValidMenuItem(1, placeId, productId, name: "Cappuccino");
        menuItem.Product!.Volume = "M";
        menuItem.Place = TestDataFactory.CreateValidPlace(placeId, businessid: 3);
        menuItem.Place.Business.Name = "Some Biz";

        var placeDto = TestDataFactory.CreatePlaceDtoFromPlace(menuItem.Place!);
        var baseProductDto = TestDataFactory.CreateProductBaseDtoFromProduct(menuItem.Product!);
        var expectedMenuItemDto = TestDataFactory.CreateMenuItemDto(
            id: menuItem.Id,
            product: baseProductDto,
            place: placeDto,
            price: menuItem.Price,
            description: menuItem.Description,
            isAvailable: menuItem.IsAvailable
        );

        _menuRepository.GetByKeyAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(),
            true,
            Arg.Any<Expression<Func<MenuItem, object>>>(),
            Arg.Any<Expression<Func<MenuItem, object>>>()
        ).Returns(menuItem);

        _mapper.Map<MenuItemDto>(menuItem).Returns(expectedMenuItemDto);

        // Act
        var result = await _menuService.GetByIdAsync(placeId, productId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Product.Name, Is.EqualTo("Cappuccino"));
            Assert.That(result.Product.Category, Is.Not.Null);
        });

        await _menuRepository.Received(1).GetByKeyAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(),
            true,
            Arg.Any<Expression<Func<MenuItem, object>>>(),
            Arg.Any<Expression<Func<MenuItem, object>>>()
        );

        _mapper.Received(1).Map<MenuItemDto>(menuItem);
    }

    [Test]
    public async Task GetByIdAsync_MenuItemNotFound_ThrowsMenuItemNotFoundException()
    {
        // Arrange
        var placeId = 1;
        var productId = 999;

        _menuRepository.GetByKeyAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(),
            true,
            Arg.Any<Expression<Func<MenuItem, object>>>(),
            Arg.Any<Expression<Func<MenuItem, object>>>())
            .Returns((MenuItem?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MenuItemNotFoundException>(() =>
            _menuService.GetByIdAsync(placeId, productId));

        Assert.That(ex!.Message, Does.Contain($"Menu item with place ID {placeId} and product ID {productId}"));

        await _menuRepository.Received(1).GetByKeyAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(),
            true,
            Arg.Any<Expression<Func<MenuItem, object>>>(),
            Arg.Any<Expression<Func<MenuItem, object>>>());
    }

    [Test]
    public async Task GetByIdAsync_UnexpectedError_ThrowsException()
    {
        // Arrange
        var placeId = 1;
        var productId = 5;

        _menuRepository.GetByKeyAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(),
            true,
            Arg.Any<Expression<Func<MenuItem, object>>>(),
            Arg.Any<Expression<Func<MenuItem, object>>>())
            .Throws(new Exception("Database failure"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() =>
            _menuService.GetByIdAsync(placeId, productId));

        Assert.That(ex!.Message, Does.Contain("Database failure"));

        await _menuRepository.Received(1).GetByKeyAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(),
            true,
            Arg.Any<Expression<Func<MenuItem, object>>>(),
            Arg.Any<Expression<Func<MenuItem, object>>>());
    }
}
