using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Mappings;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BartenderTests;


[TestFixture]
public class PlacesServiceTests
{
    private IRepository<Places> _repository;
    private ILogger<PlacesService> _logger;
    private ICurrentUserContext _userContext;
    private IMapper _mapper;
    private PlacesService _service;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Places>>();
        _logger = Substitute.For<ILogger<PlacesService>>();
        _userContext = Substitute.For<ICurrentUserContext>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PlacesProfile>(); 
        });
        _mapper = config.CreateMapper();

        _service = new PlacesService(_repository, _logger, _userContext, _mapper);
    }

    private static Businesses CreateValidBusiness(int id = 10) => new()
    {
        Id = id,
        OIB = "12345678901",
        Name = "Coffee Inc.",
        Headquarters = "Main St. 1",
        SubscriptionTier = SubscriptionTier.premium
    };

    private static Cities CreateValidCity(int id = 5) => new()
    {
        Id = id,
        Name = "Zagreb"
    };

    private static Products CreateValidProduct(int id = 1) => new()
    {
        Id = id,
        Name = "Espresso"
    };

    private static MenuItems CreateValidMenuItem(int placeId = 1, int productId = 1) => new()
    {
        PlaceId = placeId,
        ProductId = productId,
        Price = 1.50m,
        IsAvailable = true,
        Description = "Strong and black",
        Product = CreateValidProduct(productId)
    };

    private static Places CreateValidPlace(int id = 1) => new()
    {
        Id = id,
        BusinessId = 10,
        CityId = 5,
        Address = "Test Address",
        OpensAt = new TimeOnly(8, 0),
        ClosesAt = new TimeOnly(16, 0),
        Business = CreateValidBusiness(),
        City = CreateValidCity(),
        MenuItems = [CreateValidMenuItem(id)]
    };

    private static InsertPlaceDto CreateValidInsertPlaceDto() => new()
    {
        BusinessId = 10,
        CityId = 5,
        Address = "Test Address",
        OpensAt = "08:00",
        ClosesAt = "16:00"
    };

    private static UpdatePlaceDto CreateValidUpdatePlaceDto() => new()
    {
        Address = "Updated Address",
        OpensAt = "09:00",
        ClosesAt = "17:00"
    };

    private static Staff CreateValidStaff(int placeId = 10) => new()
    {
        Id = 99,
        PlaceId = placeId,
        OIB = "98765432100",
        Username = "admin",
        Password = "secure123",
        FullName = "Admin User",
        Role = EmployeeRole.admin
    };


    // --- Tests below ---

    [Test]
    public async Task AddAsync_Should_Add_Place_When_Authorized()
    {
        var dto = CreateValidInsertPlaceDto();
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(10));

        var result = await _service.AddAsync(dto);

        Assert.That(result.Success, Is.True);
        await _repository.Received(1).AddAsync(Arg.Any<Places>());
    }

    [Test]
    public async Task AddAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        var dto = CreateValidInsertPlaceDto();
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(99));

        var result = await _service.AddAsync(dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Places>());
    }

    [Test]
    public async Task DeleteAsync_Should_Delete_When_Found_And_Authorized()
    {
        var place = CreateValidPlace();
        _repository.GetByIdAsync(place.Id).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(10));

        var result = await _service.DeleteAsync(place.Id);

        Assert.That(result.Success, Is.True);
        await _repository.Received(1).DeleteAsync(place);
    }

    [Test]
    public async Task DeleteAsync_Should_Return_NotFound_When_Missing()
    {
        _repository.GetByIdAsync(1).Returns((Places?)null);

        var result = await _service.DeleteAsync(1);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task DeleteAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        var place = CreateValidPlace();
        place.BusinessId = 999;
        _repository.GetByIdAsync(1).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(10));

        var result = await _service.DeleteAsync(1);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Places>());
    }

    [Test]
    public async Task UpdateAsync_Should_Update_When_Found_And_Authorized()
    {
        var place = CreateValidPlace();
        _repository.GetByIdAsync(place.Id).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(10));

        var dto = CreateValidUpdatePlaceDto();
        var result = await _service.UpdateAsync(place.Id, dto);

        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(place);
    }

    [Test]
    public async Task UpdateAsync_Should_Return_NotFound_When_Missing()
    {
        _repository.GetByIdAsync(1).Returns((Places?)null);

        var result = await _service.UpdateAsync(1, CreateValidUpdatePlaceDto());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
    }

    [Test]
    public async Task UpdateAsync_Should_Return_Unauthorized_When_CrossBusiness()
    {
        var place = CreateValidPlace();
        place.BusinessId = 999;
        _repository.GetByIdAsync(1).Returns(place);
        _userContext.GetCurrentUserAsync().Returns(CreateValidStaff(10));

        var result = await _service.UpdateAsync(1, CreateValidUpdatePlaceDto());

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Places>());
    }
}

