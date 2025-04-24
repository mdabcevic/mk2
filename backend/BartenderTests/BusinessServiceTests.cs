using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Business;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BartenderTests;

[TestFixture]
public class BusinessServiceTests
{
    private IRepository<Business> _repository;
    private ILogger<BusinessService> _logger;
    private ICurrentUserContext _currentUser;
    private IMapper _mapper;
    private BusinessService _businessService;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Business>>();
        _logger = Substitute.For<ILogger<BusinessService>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _mapper = Substitute.For<IMapper>();

        _businessService = new BusinessService(_repository, _logger, _currentUser, _mapper);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsBusiness_WhenAuthorized()
    {
        // Arrange
        var business = TestDataFactory.CreateValidBusiness(1);
        var dto = TestDataFactory.CreateBusinessDtoFromEntity(business);
        var staff = TestDataFactory.CreateValidStaff( businessid: 1, placeid: 10);

        _repository.GetByIdAsync(1, true).Returns(business);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _mapper.Map<BusinessDto>(business).Returns(dto);

        // Act
        var result = await _businessService.GetByIdAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.TypeOf<BusinessDto>());
        });
        await _repository.Received(1).GetByIdAsync(1, true);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsUnauthorized_WhenBusinessMismatch()
    {
        // Arrange
        var business = TestDataFactory.CreateValidBusiness(2);
        var staff = TestDataFactory.CreateValidStaff(placeid: 99);

        _repository.GetByIdAsync(2, true).Returns(business);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _businessService.GetByIdAsync(2);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
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
        await _repository.DidNotReceive().AddAsync(Arg.Any<Business>());
    }

    [Test]
    public async Task AddAsync_AddsBusiness_WhenValid()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "12345678901", Name = "New Business", Headquarters = "HQ" };
        var entity = TestDataFactory.CreateValidBusiness(oib: "12345678901", name: "New Business", sub: SubscriptionTier.none);
        _mapper.Map<Business>(dto).Returns(entity);

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
        var staff = TestDataFactory.CreateValidStaff();
        var business = TestDataFactory.CreateValidBusiness(1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        var result = await _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(Arg.Is<Business>(b => b.SubscriptionTier == SubscriptionTier.premium));
    }

    [Test]
    public async Task UpdateSubscriptionAsync_ReturnsError_WhenNoPlaceAssigned()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff();
        staff.Place = null;

        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Business>());
    }


    [Test]
    public async Task UpdateAsync_UpdatesBusiness_WhenExists()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "123", Name = "Business", Headquarters = "Main HQ" };
        var business = TestDataFactory.CreateValidBusiness(1);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        var result = await _businessService.UpdateAsync(1, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).UpdateAsync(business);
    }

    [Test]
    public async Task UpdateAsync_ReturnsNotFound_WhenBusinessMissing()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "123", Name = "Nonexistent Business", Headquarters = "HQ" };
        _repository.GetByIdAsync(Arg.Any<int>()).Returns((Business?)null);

        // Act
        var result = await _businessService.UpdateAsync(1, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Business>());
    }

    [Test]
    public async Task DeleteAsync_DeletesBusiness_WhenExists()
    {
        // Arrange
        var business = TestDataFactory.CreateValidBusiness(1);
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
        _repository.GetByIdAsync(1).Returns((Business?)null);

        // Act
        var result = await _businessService.DeleteAsync(1);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Business>());
    }
}
