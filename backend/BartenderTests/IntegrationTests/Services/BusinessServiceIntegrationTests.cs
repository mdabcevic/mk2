using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Business;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.AuthorizationExceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class BusinessServiceIntegrationTests : IntegrationTestBase
{
    private IBusinessService _service = null!;
    private IRepository<Business> _businessRepo = null!;
    private IRepository<Place> _placeRepo = null!;
    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void Setup()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IBusinessService>();
        _businessRepo = scope.ServiceProvider.GetRequiredService<IRepository<Business>>();
        _placeRepo = scope.ServiceProvider.GetRequiredService<IRepository<Place>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    [Test]
    public async Task AddAsync_ShouldAddBusiness()
    {
        var dto = new UpsertBusinessDto
        {
            Name = "New Biz",
            OIB = "99945678901"
        };

        Assert.DoesNotThrowAsync(async () => await _service.AddAsync(dto));
        var exists = await _businessRepo.ExistsAsync(b => b.OIB == dto.OIB);
        Assert.That(exists, Is.True);
    }

    [Test]
    public void AddAsync_ShouldFail_WhenOIBInvalid()
    {
        var dto = new UpsertBusinessDto
        {
            Name = "Invalid Biz",
            OIB = "1234" // invalid
        };

        var ex = Assert.ThrowsAsync<AppValidationException>(() => _service.AddAsync(dto));
        Assert.That(ex?.Message, Does.Contain("OIB must be 11 characters"));
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenOIBAlreadyExists()
    {
        var business = new Business { Name = "Dupe", OIB = "11111111111" };
        await _businessRepo.AddAsync(business);

        var dto = new UpsertBusinessDto
        {
            Name = "Conflict",
            OIB = "11111111111" // duplicate
        };

        var ex = Assert.ThrowsAsync<AppValidationException>(() => _service.AddAsync(dto));
        Assert.That(ex?.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateBusiness()
    {
        var business = new Business { Name = "Before", OIB = "12345678901"};
        await _businessRepo.AddAsync(business);

        var dto = new UpsertBusinessDto
        {
            Name = "After",
            OIB = "12345678901"
        };

        await _service.UpdateAsync(business.Id, dto);

        var updated = await _businessRepo.GetByIdAsync(business.Id);
        Assert.That(updated!.Name, Is.EqualTo("After"));
    }

    [Test]
    public void UpdateAsync_ShouldFail_WhenNotFound()
    {
        var dto = new UpsertBusinessDto
        {
            Name = "Ghost",
            OIB = "99999999999"
        };

        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(() => _service.UpdateAsync(9999, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteBusiness()
    {
        var business = new Business { Name = "RemoveMe", OIB = "32132132111"};
        await _businessRepo.AddAsync(business);

        await _service.DeleteAsync(business.Id);

        var exists = await _businessRepo.GetByIdAsync(business.Id);
        Assert.That(exists, Is.Null);
    }

    [Test]
    public void DeleteAsync_ShouldFail_WhenNotFound()
    {
        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(() => _service.DeleteAsync(9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateSubscriptionAsync_ShouldUpdateTier()
    {
        var business = new Business { Name = "Tiered", OIB = "99988877766"};
        await _businessRepo.AddAsync(business);

        var place = new Place { Address = "HQ", BusinessId = business.Id, CityId = 1 };
        await _placeRepo.AddAsync(place);

        _mockUser.Override(new Staff
        {
            Id = 11,
            OIB = "11111111111",
            Username = "tieruser",
            FullName = "Sub Manager",
            Role = EmployeeRole.manager,
            Password = "hashed",
            Place = place
        });

        await _service.UpdateSubscriptionAsync(SubscriptionTier.premium);

        var updated = await _businessRepo.GetByIdAsync(business.Id);
        Assert.That(updated!.SubscriptionTier, Is.EqualTo(SubscriptionTier.premium));
    }

    [Test]
    public void UpdateSubscriptionAsync_ShouldFail_WhenNoPlace()
    {
        _mockUser.Override(new Staff
        {
            Id = 12,
            OIB = "11111111111",
            Username = "noplacer",
            FullName = "No Place",
            Role = EmployeeRole.manager,
            Password = "hashed",
            Place = null
        });

        var ex = Assert.ThrowsAsync<UserPlaceAssignmentException>(() => _service.UpdateSubscriptionAsync(SubscriptionTier.standard));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void UpdateSubscriptionAsync_ShouldFail_WhenBusinessNotFound()
    {
        var fakePlace = new Place
        {
            Id = 999,
            Address = "Missing Biz",
            BusinessId = 9999
        };

        _mockUser.Override(new Staff
        {
            Id = 13,
            OIB = "11111111111",
            Username = "lostbiz",
            FullName = "Lost Biz User",
            Role = EmployeeRole.manager,
            Password = "hashed",
            Place = fakePlace
        });

        var ex = Assert.ThrowsAsync<BusinessNotFoundException>(() => _service.UpdateSubscriptionAsync(SubscriptionTier.standard));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldFail_WhenAccessingOtherBusiness()
    {
        // Arrange
        var business = new Business { Name = "Forbidden", OIB = "22222222222" };
        await _businessRepo.AddAsync(business);

        _mockUser.Override(new Staff
        {
            Id = 5,
            OIB = "11111111111",
            Username = "outsider",
            Password = "hashed",
            FullName = "Outsider",
            Role = EmployeeRole.manager,
            Place = new Place { Id = 1, Address = "Far", BusinessId = business.Id + 1 } // different business
        });

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedBusinessAccessException>(() => _service.GetByIdAsync(business.Id));
        Assert.That(ex, Is.Not.Null);
    }
}
