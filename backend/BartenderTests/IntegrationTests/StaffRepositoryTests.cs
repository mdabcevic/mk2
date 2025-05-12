using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class StaffRepositoryTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_ThenGetByIdAsync_ShouldReturnStaffWithPlaceAndBusiness()
    {
        using var scope = Factory.Services.CreateScope();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IRepository<Staff>>();
        var placeRepo = scope.ServiceProvider.GetRequiredService<IRepository<Place>>();
        var businessRepo = scope.ServiceProvider.GetRequiredService<IRepository<Business>>();
        var cityRepo = scope.ServiceProvider.GetRequiredService<IRepository<City>>();

        // Arrange - Seed related entities
        var city = TestDataFactory.CreateValidCity(1);
        await cityRepo.AddAsync(city);

        var business = TestDataFactory.CreateValidBusiness(1);
        await businessRepo.AddAsync(business);

        var place = TestDataFactory.CreateValidPlace(1, business.Id, city.Id);
        await placeRepo.AddAsync(place);

        var staff = TestDataFactory.CreateValidStaff(1, place.Id, business.Id, "janedoe");
        await staffRepo.AddAsync(staff);

        // Act
        var retrieved = await staffRepo.GetByIdAsync(staff.Id, includeNavigations: true);

        Assert.That(retrieved, Is.Not.Null, "Staff should be retrieved from database");
        Assert.Multiple(() =>
        {
            Assert.That(retrieved!.Username, Is.EqualTo("janedoe"), "Username should match");
            Assert.That(retrieved.Place, Is.Not.Null, "Staff should be linked to a Place");
        });
        Assert.Multiple(() =>
        {
            Assert.That(retrieved.Place!.BusinessId, Is.EqualTo(business.Id), "Place should be linked to correct business");
            Assert.That(retrieved.Place.Business, Is.Not.Null, "Place should include navigation to Business");
        });
        Assert.That(retrieved.Place.Business!.Name, Is.EqualTo("test name"), "Business name should match expected value");
    }
}
