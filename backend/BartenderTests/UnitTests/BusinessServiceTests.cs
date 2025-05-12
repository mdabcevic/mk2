using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Business;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace BartenderTests.UnitTests;

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
        Assert.That(result, Is.EqualTo(dto));
        await _repository.Received(1).GetByIdAsync(1, true);
    }

    [Test]
    public void GetByIdAsync_ThrowsUnauthorized_WhenBusinessMismatch()
    {
        // Arrange
        var business = TestDataFactory.CreateValidBusiness(2);
        var staff = TestDataFactory.CreateValidStaff(businessid: 1); // mismatch

        _repository.GetByIdAsync(2, true).Returns(business);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(
            () => _businessService.GetByIdAsync(2)
        );

        Assert.That(ex!.Message, Is.Not.Null);
    }

    [Test]
    public void GetByIdAsync_ThrowsNotFound_WhenBusinessMissing()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(businessid: 999, placeid: 10, role: EmployeeRole.regular);
        staff.Place = TestDataFactory.CreateValidPlace(businessid: 999, id: 10);
        _repository.GetByIdAsync(999, true).Returns((Business?)null);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(
            () => _businessService.GetByIdAsync(999)
        );

        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public void GetByIdAsync_ThrowsUnauthorized_WhenNoUser()
    {
        // Arrange
        _currentUser.GetCurrentUserAsync().Returns(Task.FromResult<Staff?>(null));

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _businessService.GetByIdAsync(1));
    }

    [Test]
    public void AddAsync_ThrowsValidationError_WhenOIBTooShort()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "123", Name = "Invalid" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<AppValidationException>(() => _businessService.AddAsync(dto));

        Assert.That(ex!.Message, Is.EqualTo("OIB must be 11 characters"));
        _repository.DidNotReceive().AddAsync(Arg.Any<Business>());
    }

    [Test]
    public async Task AddAsync_AddsBusiness_WhenValid()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "12345678901", Name = "New Business", Headquarters = "HQ" };
        var entity = TestDataFactory.CreateValidBusiness(oib: dto.OIB, name: dto.Name, sub: SubscriptionTier.none);
        _mapper.Map<Business>(dto).Returns(entity);

        // Act
        await _businessService.AddAsync(dto);

        // Assert
        await _repository.Received(1).AddAsync(entity);
    }

    [Test]
    public void AddAsync_ThrowsConflict_WhenOIBAlreadyExists()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "12345678901", Name = "Duplicate Business" };
        _repository.ExistsAsync(Arg.Any<Expression<Func<Business, bool>>>()).Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AppValidationException>(() => _businessService.AddAsync(dto));
        Assert.That(ex!.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task UpdateSubscriptionAsync_UpdatesTier_WhenAuthorized()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(businessid: 999);
        var business = TestDataFactory.CreateValidBusiness(1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        await _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<Business>(b => b.SubscriptionTier == SubscriptionTier.premium));
    }

    [Test]
    public void UpdateSubscriptionAsync_Throws_WhenNoPlaceAssigned()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff();
        staff.Place = null;

        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserPlaceAssignmentException>(
            () => _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium)
        );

        Assert.That(ex!.Message, Does.Contain($"Error fetching"));
        _repository.DidNotReceive().UpdateAsync(Arg.Any<Business>());
    }

    [Test]
    public void UpdateSubscriptionAsync_Throws_WhenBusinessNotFound()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(businessid: 999);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(999).Returns((Business?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(
            () => _businessService.UpdateSubscriptionAsync(SubscriptionTier.premium)
        );

        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateAsync_UpdatesBusiness_WhenExists()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "123", Name = "Business", Headquarters = "Main HQ" };
        var business = TestDataFactory.CreateValidBusiness(1);

        _repository.GetByIdAsync(1).Returns(business);

        // Act
        await _businessService.UpdateAsync(1, dto);

        // Assert
        await _repository.Received(1).UpdateAsync(business);
        _mapper.Received(1).Map(dto, business);
    }

    [Test]
    public void UpdateAsync_ThrowsNotFound_WhenBusinessMissing()
    {
        // Arrange
        var dto = new UpsertBusinessDto { OIB = "123", Name = "Nonexistent Business", Headquarters = "HQ" };
        _repository.GetByIdAsync(Arg.Any<int>()).Returns((Business?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(
            () => _businessService.UpdateAsync(1, dto)
        );

        Assert.That(ex!.Message, Does.Contain("not found"));
        _repository.DidNotReceive().UpdateAsync(Arg.Any<Business>());
    }

    [Test]
    public async Task DeleteAsync_DeletesBusiness_WhenExists()
    {
        // Arrange
        var business = TestDataFactory.CreateValidBusiness(1);
        _repository.GetByIdAsync(1).Returns(business);

        // Act
        await _businessService.DeleteAsync(1);

        // Assert
        await _repository.Received(1).DeleteAsync(business);
    }

    [Test]
    public void DeleteAsync_Throws_WhenNotFound()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns((Business?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(
            () => _businessService.DeleteAsync(1)
        );

        Assert.That(ex!.Message, Does.Contain("not found"));
        _repository.DidNotReceive().DeleteAsync(Arg.Any<Business>());
    }
}