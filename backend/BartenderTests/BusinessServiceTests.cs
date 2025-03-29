using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BartenderTests;

[TestFixture]
public class BusinessServiceTests
{
    private IRepository<Businesses> _repository;
    private ILogger<BusinessService> _logger;
    private ICurrentUserContext _currentUser;
    private IMapper _mapper;
    private BusinessService _businessService;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Businesses>>();
        _logger = Substitute.For<ILogger<BusinessService>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _mapper = Substitute.For<IMapper>();

        _businessService = new BusinessService(_repository, _logger, _currentUser, _mapper);
    }


    private static Businesses CreateValidBusiness(int id = 1) => new()
    {
        Id = id,
        OIB = "12345678901",
        Name = $"Business {id}",
        Headquarters = "HQ",
        SubscriptionTier = SubscriptionTier.basic,
        Places = []
    };

    private static Staff CreateValidStaff(int placeId = 10) => new()
    {
        Id = 1,
        PlaceId = placeId,
        OIB = "98765432100",
        Username = "testuser",
        Password = "pass",
        FullName = "Test User",
        Role = EmployeeRole.admin,
        Place = CreateValidPlace(placeId)
    };

    private static Places CreateValidPlace(int id = 1, int businessId = 1) => new()
    {
        Id = id,
        BusinessId = businessId,
        CityId = 1,
        Address = "Some St 5",
        OpensAt = new TimeOnly(7, 0),
        ClosesAt = new TimeOnly(17, 0),
        Business = CreateValidBusiness(businessId),
        City = new Cities { Id = 1, Name = "Zagreb" },
        MenuItems = []
    };

    private static InsertPlaceDto CreateValidInsertPlaceDto() => new()
    {
        BusinessId = 1,
        CityId = 1,
        Address = "Some St 5",
        OpensAt = "07:00",
        ClosesAt = "17:00"
    };

    private static UpdatePlaceDto CreateValidUpdatePlaceDto() => new()
    {
        Address = "Updated Address",
        OpensAt = "08:00",
        ClosesAt = "18:00"
    };

    private static UpsertBusinessDto CreateValidUpsertBusinessDto() => new()
    {
        OIB = "12345678901",
        Name = "New Biz",
        Headquarters = "Main HQ"
    };

    private static BusinessDto CreateBusinessDtoFromEntity(Businesses business) => new()
    {
        OIB = business.OIB,
        Name = business.Name,
        Headquarters = business.Headquarters,
        SubscriptionTier = business.SubscriptionTier,
        Places = []
    };



    [Test]
    public async Task GetByIdAsync_ReturnsBusiness_WhenAuthorized()
    {
        // Arrange
        var business = CreateValidBusiness(1);
        var dto = CreateBusinessDtoFromEntity(business);
        var staff = CreateValidStaff(placeId: 10);
        staff.Place.BusinessId = 1;

        _repository.GetByIdAsync(1, true).Returns(business);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _mapper.Map<BusinessDto>(business).Returns(dto);

        // Act
        var result = await _businessService.GetByIdAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data?.OIB, Is.EqualTo(dto.OIB));
        });
    }

    [Test]
    public async Task GetByIdAsync_ReturnsUnauthorized_WhenBusinessMismatch()
    {
        // Arrange
        var business = CreateValidBusiness(2);
        var staff = CreateValidStaff(placeId: 99);
        staff.Place.BusinessId = 1;

        _repository.GetByIdAsync(2, true).Returns(business);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _businessService.GetByIdAsync(2);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
    }

    [Test]
    public async Task AddAsync_ReturnsValidationError_WhenOIBTooShort()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "123", Name = "Invalid" };

        // Act
        var result = await _businessService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
        });
    }

    [Test]
    public async Task AddAsync_AddsBusiness_WhenValid()
    {
        // Arrange
        var dto = CreateValidUpsertBusinessDto();
        var entity = CreateValidBusiness();
        _mapper.Map<Businesses>(dto).Returns(entity);

        // Act
        var result = await _businessService.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).AddAsync(entity);
    }

    [Test]
    public async Task UpdateSubscriptionAsync_UpdatesTier_WhenAuthorized()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.Place.BusinessId = 1;
        var business = CreateValidBusiness(1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        var result = await _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(Arg.Is<Businesses>(b => b.SubscriptionTier == SubscriptionTier.premium));
    }

    [Test]
    public async Task UpdateSubscriptionAsync_ReturnsError_WhenNoPlaceAssigned()
    {
        // Arrange
        var staff = CreateValidStaff();
        staff.Place = null;

        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
    }


    [Test]
    public async Task UpdateAsync_UpdatesBusiness_WhenExists()
    {
        // Arrange
        var dto = CreateValidUpsertBusinessDto();
        var business = CreateValidBusiness(1);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        var result = await _businessService.UpdateAsync(1, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(business);
    }

    [Test]
    public async Task DeleteAsync_DeletesBusiness_WhenExists()
    {
        // Arrange
        var business = CreateValidBusiness(1);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        var result = await _businessService.DeleteAsync(1);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).DeleteAsync(business);
    }

    [Test]
    public async Task DeleteAsync_ReturnsError_WhenNotFound()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Businesses?)null);

        // Act
        var result = await _businessService.DeleteAsync(1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
    }
}
